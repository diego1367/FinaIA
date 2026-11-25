using CreditRequests.Application.Interfaces;
using CreditRequests.Domain;
using Microsoft.EntityFrameworkCore;

namespace CreditRequests.Infrastructure.Persistence;

public sealed class EfCreditRequestRepository(CreditRequestsDbContext db) : ICreditRequestRepository
{
    public async Task<IReadOnlyList<CreditRequest>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await db.CreditRequests.AsNoTracking()
            .OrderByDescending(request => request.CreatedAtUtc)
            .ThenByDescending(request => request.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<CreditRequest?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        db.CreditRequests.AsNoTracking().FirstOrDefaultAsync(request => request.Id == id, cancellationToken);

    public async Task AddAsync(CreditRequest request, CancellationToken cancellationToken)
    {
        db.CreditRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> TryUpdateStatusAsync(CreditRequest request, CancellationToken cancellationToken)
    {
        // Attach preserves the row version read from SQL Server for the concurrency check.
        var entry = db.CreditRequests.Attach(request);
        entry.Property(item => item.Status).IsModified = true;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            entry.State = EntityState.Detached;
            return false;
        }
    }
}
