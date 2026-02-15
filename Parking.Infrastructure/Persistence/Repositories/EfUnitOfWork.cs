using Parking.Application.Contracts.Interfaces;
using Parking.Infrastructure.Persistence;

namespace Parking.Infrastructure.Persistence.Repositories;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public EfUnitOfWork(AppDbContext db) => _db = db;

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
