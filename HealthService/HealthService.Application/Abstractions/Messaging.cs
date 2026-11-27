using MediatR;

namespace HealthService.Application.Abstractions;

/// <summary>Команда без значення, що повертається; змінює стан.</summary>
public interface ICommand : IRequest<Unit>
{
}

/// <summary>Команда зі значенням, що повертається; змінює стан.</summary>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>Запит: лише читає, стан не змінює.</summary>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
