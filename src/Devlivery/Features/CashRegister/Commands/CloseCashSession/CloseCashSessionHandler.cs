using Devlivery.Features.CashRegister.Domain;
using Devlivery.Features.CashRegister.Infrastructure;
using Devlivery.Features.Orders.Domain;
using Devlivery.Shared.Application.Errors;
using Devlivery.Shared.Infrastructure.Persistence;
using Devlivery.Shared.Infrastructure.Persistence.Context;

using FluentResults;

using Mediator;

using Microsoft.EntityFrameworkCore;

namespace Devlivery.Features.CashRegister.Commands.CloseCashSession;

public sealed class CloseCashSessionHandler(
    ICashSessionRepository cashSessionRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CloseCashSessionCommand, Result<CloseCashSessionResponse>>
{
    public async ValueTask<Result<CloseCashSessionResponse>> Handle(CloseCashSessionCommand command,
        CancellationToken cancellationToken)
    {
        var cashSession = await cashSessionRepository.GetByIdAsync(command.Id, cancellationToken);

        if (cashSession is null)
        {
            return Result.Fail<CloseCashSessionResponse>(new NotFoundError("Caixa não encontrado."));
        }

        if (cashSession.Status == CashSessionStatus.Closed)
        {
            return Result.Fail<CloseCashSessionResponse>(new DomainRuleError("O caixa já está fechado."));
        }

        // Get all orders within the cash session period (exclude canceled)
        var sessionStart = cashSession.StartAt;
        var sessionEnd = DateTime.UtcNow;

        var sessionOrders = await orderRepository.GetOrdersInPeriodAsync(
            sessionStart,
            sessionEnd,
            cancellationToken);

        // Calculate totals
        var totalRevenue = sessionOrders.Sum(o => o.Total);
        var totalOrders = sessionOrders.Count;

        // Calculate payment breakdown
        var paymentBreakdownItems = sessionOrders
            .GroupBy(o => o.PaymentMethod.ToString())
            .Select(g => new PaymentBreakdownItem(
                g.Key,
                g.Sum(o => o.Total),
                g.Count()))
            .ToList();

        var paymentBreakdown = paymentBreakdownItems
            .Select(pb => new Domain.PaymentBreakdownItem(pb.Method, pb.Amount, pb.Count))
            .ToList();

        var totalDeposits = cashSession.TotalDeposits();

        var cashSales = paymentBreakdown
            .Where(pb => pb.Method.Equals(nameof(PaymentMethod.Cash), StringComparison.OrdinalIgnoreCase))
            .Sum(pb => pb.Amount);

        var expectedCashAmount = cashSession.OpeningAmount + totalDeposits + cashSales;
        cashSession.UpdateExpectedCashAmount(expectedCashAmount);

        cashSession.UpdateTotals(totalRevenue, totalOrders, paymentBreakdown);
        cashSession.Close(command.ClosingAmount, command.Notes);

        await cashSessionRepository.UpdateAsync(cashSession, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok(CloseCashSessionResponse.FromDomain(cashSession, expectedCashAmount));
    }
}