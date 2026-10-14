namespace Devlivery.Common.Handlers;

public interface ICommandHandler<in TCommand, TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
}