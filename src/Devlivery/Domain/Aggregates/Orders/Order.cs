using Devlivery.Domain.Aggregates.Orders.Entities;
using Devlivery.Domain.Aggregates.Orders.Enums;
using Devlivery.Domain.Aggregates.Orders.Events;
using Devlivery.Domain.Aggregates.Orders.ValueObjects;
using Devlivery.Domain.SeedWork;

namespace Devlivery.Domain.Aggregates.Orders;

public sealed class Order : Entity
{
    private readonly List<OrderItem> _items = [];
    private readonly List<OrderPayment> _payments = [];

    public CustomerInfo Customer { get; private set; } = null!;
    public DeliveryAddress DeliveryAddress { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public decimal Total { get; private set; }
    public decimal Change { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public Guid EstablishmentId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<OrderPayment> Payments => _payments.AsReadOnly();

    private Order()
    {
    }

    public Order(
        CustomerInfo customer,
        DeliveryAddress deliveryAddress,
        decimal deliveryFee,
        Guid establishmentId,
        List<OrderItem> items,
        List<OrderPayment> payments,
        string? notes = null
    )
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Pedido deve ter pelo menos um item", nameof(items));
        if (payments == null || payments.Count == 0)
            throw new ArgumentException("Pedido deve ter pelo menos uma forma pagamento", nameof(payments));

        if (deliveryFee < 0)
            throw new ArgumentException("Taxa de entrega não pode ser negativa", nameof(deliveryFee));

        Customer = customer;
        DeliveryAddress = deliveryAddress;
        Status = OrderStatus.Pending;
        DeliveryFee = deliveryFee;
        EstablishmentId = establishmentId;

        _items = items;
        _payments = payments;

        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Notes = notes;
        Change = 0m;

        CalculateTotal();
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (Status == OrderStatus.Canceled)
        {
            throw new DomainException("Não é possível alterar o status de um pedido cancelado.");
        }

        if (Status == OrderStatus.Delivered && newStatus != OrderStatus.Delivered)
        {
            throw new DomainException("Não é possível alterar o status de um pedido já entregue.");
        }

        if (newStatus == OrderStatus.Delivered)
        {
            var paymentsTotal = _payments.Where(x => x.PaymentStatus != PaymentStatus.Cancelled).Sum(x => x.Amount);
            if (paymentsTotal < Total)
            {
                throw new InvalidOperationException(
                    $"O total dos pagamentos ({paymentsTotal:C}) é menor que o total do pedido ({Total:C}).");
            }

            Change = paymentsTotal - Total;

            _payments.Where(p => p.PaymentStatus == PaymentStatus.Pending)
                .ToList()
                .ForEach(ConfirmPayment);

            AddDomainEvent(new OrderChangeCalculatedEvent(Id, EstablishmentId, Change, DateTime.UtcNow));
        }

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderStatusChangedEvent(
            Id,
            EstablishmentId,
            oldStatus,
            newStatus,
            Total,
            UpdatedAt
        ));
    }

    private void ConfirmPayment(OrderPayment payment)
    {
        payment.Confirm();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderPaymentConfirmedEvent(
            Id,
            payment.Id,
            EstablishmentId,
            payment.PaymentMethod,
            payment.Amount,
            Total
        ));
    }

    public void AddPayment(OrderPayment payment)
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Canceled)
            throw new InvalidOperationException("Não é possível adicionar pagamentos a um pedido finalizado.");

        _payments.Add(payment);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderPaymentAddedEvent(Id, payment.Id, EstablishmentId, payment.PaymentMethod,
            payment.Amount, payment.CreatedAt));
    }

    public void RemovePayment(Guid paymentId)
    {
        if (Status == OrderStatus.Delivered || Status == OrderStatus.Canceled)
            throw new DomainException("Não é possível remover pagamentos de um pedido finalizado.");

        var payment = _payments.FirstOrDefault(p => p.Id == paymentId);
        if (payment == null) return;

        _payments.Remove(payment);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        AddDomainEvent(new OrderDeletedEvent(
            Id,
            EstablishmentId,
            Total,
            Status,
            DateTime.UtcNow
        ));
    }

    public void UpdateDetails(
        CustomerInfo customer,
        DeliveryAddress deliveryAddress,
        decimal deliveryFee,
        string? notes = null,
        List<OrderItem>? items = null)
    {
        Customer = customer;
        DeliveryAddress = deliveryAddress;
        DeliveryFee = deliveryFee;
        UpdatedAt = DateTime.UtcNow;
        Notes = notes;
        if (items is not null)
        {
            _items.Clear();
            _items.AddRange(items);
        }

        CalculateTotal();
    }

    private void CalculateTotal()
    {
        Total = _items.Sum(i => i.TotalPrice) + DeliveryFee;
    }

    public void ReconcilePayments(IEnumerable<OrderPaymentUpdate> incoming)
    {
        incoming ??= [];

        var incomingList = incoming.ToList();
        var existingById = _payments.ToDictionary(p => p.Id, p => p);

        foreach (var p in incomingList)
        {
            if (p.Id is not null && existingById.TryGetValue(p.Id.Value, out var existing))
            {
                if (existing.PaymentStatus == PaymentStatus.Confirmed)
                {
                    if (existing.Amount != p.Amount || existing.PaymentMethod != p.Method)
                        throw new DomainException(
                            "Não é possível alterar pagamento já confirmado. Realize estorno antes de alterar.");
                }
                else
                {
                    existing.Update(p.Method, p.Amount);
                    AddDomainEvent(new OrderPaymentUpdatedEvent(Id, existing.Id, EstablishmentId,
                        existing.PaymentMethod, existing.Amount, existing.UpdatedAt));
                }

                existingById.Remove(p.Id.Value);
            }
            else
            {
                var newPayment = new OrderPayment(EstablishmentId, p.Method, p.Amount);
                AddPayment(newPayment);
            }
        }

        foreach (var payment in existingById.Select(kv => kv.Value))
        {
            if (payment.PaymentStatus == PaymentStatus.Pending)
            {
                payment.Cancel();
                AddDomainEvent(new OrderPaymentCancelledEvent(Id, payment.Id, EstablishmentId, DateTime.UtcNow));
            }
            else
            {
                throw new DomainException(
                    "Não é possível remover pagamento já confirmado. Realize estorno antes de remover.");
            }
        }

        UpdatedAt = DateTime.UtcNow;
    }
}