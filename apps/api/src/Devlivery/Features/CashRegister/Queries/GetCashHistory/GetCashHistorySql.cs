namespace Devlivery.Features.CashRegister.Queries.GetCashHistory;

internal static class GetCashHistorySql
{
    // Same accounting rules as CashSession.TotalRevenue, TotalCashPayments and CountOrders.
    // Shared by the period summary and page; only the selected page is returned to the application.
    private const string SessionTotals = """
                                         with filtered_sessions as (
                                             select id, attendant_name, start_at, end_at, status, opening_amount, closing_amount
                                             from public.cash_sessions
                                             where establishment_id = @EstablishmentId
                                               and start_at >= @StartAt
                                               and start_at < @EndAt
                                         ),
                                         movements as (
                                             select m.cash_session_id, m.entry_type, m.amount, m.payment_method,
                                                    m.related_order_id, m.order_payment_id
                                             from public.cash_session_movements m
                                             join filtered_sessions s on s.id = m.cash_session_id
                                             where m.establishment_id = @EstablishmentId
                                         ),
                                         unrefunded_payments as (
                                             select p.cash_session_id, p.related_order_id
                                             from movements p
                                             where p.entry_type = @Payment
                                               and p.related_order_id is not null
                                               and not exists (
                                                   select 1 from movements r
                                                   where r.cash_session_id = p.cash_session_id
                                                     and r.entry_type = @Refund
                                                     and r.order_payment_id is not distinct from p.order_payment_id
                                               )
                                         ),
                                         movement_totals as (
                                             select cash_session_id,
                                                    coalesce(sum(amount) filter (where entry_type = @Payment), 0)
                                                      - coalesce(sum(amount) filter (where entry_type in (@Refund, @Change)), 0) as revenue,
                                                    coalesce(sum(amount) filter (where entry_type = @Deposit), 0) as deposits,
                                                    coalesce(sum(amount) filter (where entry_type = @Payment and payment_method = @Cash), 0)
                                                      - coalesce(sum(amount) filter (where entry_type = @Refund and payment_method = @Cash), 0)
                                                      - coalesce(sum(amount) filter (where entry_type = @Change), 0) as net_cash
                                             from movements
                                             group by cash_session_id
                                         ),
                                         order_counts as (
                                             select cash_session_id, count(distinct related_order_id)::int as total_orders
                                             from unrefunded_payments
                                             group by cash_session_id
                                         ),
                                         session_totals as (
                                             select s.*, coalesce(m.revenue, 0) as revenue, coalesce(o.total_orders, 0) as total_orders,
                                                    s.closing_amount - (s.opening_amount + coalesce(m.deposits, 0) + coalesce(m.net_cash, 0)) as difference
                                             from filtered_sessions s
                                             left join movement_totals m on m.cash_session_id = s.id
                                             left join order_counts o on o.cash_session_id = s.id
                                         )
                                         """;

    public const string Query = $"""
                                 {SessionTotals}
                                 select count(*)::int as "TotalCount",
                                        count(*) filter (where status = @Closed)::int as "ClosedCount",
                                        count(*) filter (where status = @Open)::int as "OpenCount",
                                        coalesce(sum(revenue) filter (where status = @Closed), 0) as "TotalRevenue",
                                        (select count(distinct p.related_order_id)::int
                                         from unrefunded_payments p
                                         join filtered_sessions s on s.id = p.cash_session_id
                                         where s.status = @Closed) as "TotalOrders",
                                        coalesce(sum(difference) filter (where status = @Closed), 0) as "TotalDifference",
                                        count(*) filter (where status = @Closed and difference <> 0)::int as "SessionsWithDifference"
                                 from session_totals;

                                 {SessionTotals}
                                 select id as "Id",
                                        attendant_name as "Attendant",
                                        start_at as "StartAt",
                                        end_at as "EndAt",
                                        status as "Status",
                                        total_orders as "TotalOrders",
                                        revenue as "TotalRevenue",
                                        closing_amount as "ClosingAmount",
                                        difference as "Difference"
                                 from session_totals
                                 order by start_at desc, id desc
                                 limit @Limit offset @Offset;
                                 """;
}
