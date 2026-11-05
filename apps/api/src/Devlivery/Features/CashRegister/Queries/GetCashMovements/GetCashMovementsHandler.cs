using Devlivery.Common.Errors;
using Devlivery.Common.Handlers;
using Devlivery.Common.Pagination;
using Devlivery.Domain.Aggregates.CashRegister.Enums;
using Devlivery.Domain.Common.Enums;
using Devlivery.Infrastructure.Persistence.Context;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Queries.GetCashMovements;

public sealed class GetCashMovementsHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetCashMovementsQuery, Result<PaginatedResult<GetCashMovementsResponse>>>
{
    public async Task<Result<PaginatedResult<GetCashMovementsResponse>>> HandleAsync(GetCashMovementsQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Page < 1 || query.PageSize is < 1 or > 100 || (long)(query.Page - 1) * query.PageSize > int.MaxValue)
            return Result.Fail(new ValidationError("Página deve ser positiva e tamanho deve estar entre 1 e 100."));

        var session = await dbContext.CashSessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.SessionId, cancellationToken);
        if (session is null) return Result.Fail(new NotFoundError("Caixa não encontrado."));

        var movements = dbContext.CashSessionMovements.AsNoTracking()
            .Where(m => m.CashSessionId == query.SessionId)
            .Select(m => new
            {
                m.Id,
                m.CreatedAt,
                IsOpening = false,
                Type = m.EntryType == CashSessionEntryType.Payment ? "Payment"
                    : m.EntryType == CashSessionEntryType.Change ? "Change"
                    : m.EntryType == CashSessionEntryType.Refund ? "Refund"
                    : m.EntryType == CashSessionEntryType.Deposit ? "Deposit" : "Adjustment",
                m.Amount,
                Method = m.PaymentMethod == PaymentMethod.Cash || m.EntryType == CashSessionEntryType.Deposit ? "Cash"
                    : m.PaymentMethod == PaymentMethod.Pix ? "Pix"
                    : m.PaymentMethod == PaymentMethod.CreditCard ? "CreditCard"
                    : m.PaymentMethod == PaymentMethod.DebitCard ? "DebitCard" : null,
                m.RelatedOrderId,
                m.OrderPaymentId,
                m.CreatedBy,
                m.Reason
            });
        var opening = dbContext.CashSessions.AsNoTracking().Where(s => s.Id == query.SessionId)
            .Select(s => new
            {
                s.Id,
                CreatedAt = s.StartAt,
                IsOpening = true,
                Type = "Opening",
                Amount = s.OpeningAmount,
                Method = (string?)"Cash",
                RelatedOrderId = (Guid?)null,
                OrderPaymentId = (Guid?)null,
                CreatedBy = s.AttendantId,
                Reason = (string?)null
            });
        var all = movements.Concat(opening);
        var total = await all.CountAsync(cancellationToken);
        var page = await all.OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.Id).ThenBy(m => m.IsOpening)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToArrayAsync(cancellationToken);

        var authors = page.Select(m => m.CreatedBy).Distinct().ToArray();
        var names = await dbContext.Users.AsNoTracking().Where(u => authors.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);
        var rows = page.Select(m => new GetCashMovementsResponse(
            m.IsOpening ? $"opening:{m.Id}" : m.Id.ToString(), m.CreatedAt,
            m.Type,
            m.Type is "Change" or "Refund" ? -m.Amount : m.Amount,
            m.Method is null ? null : Enum.Parse<PaymentMethod>(m.Method),
            m.RelatedOrderId, m.OrderPaymentId, m.CreatedBy,
            m.IsOpening ? session.AttendantName : names.GetValueOrDefault(m.CreatedBy, "Usuário indisponível"),
            m.Reason)).ToArray();

        return Result.Ok(new PaginatedResult<GetCashMovementsResponse>(rows, total, query.Page, query.PageSize));
    }
}
