namespace Parking.Application.Contracts.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);
}
