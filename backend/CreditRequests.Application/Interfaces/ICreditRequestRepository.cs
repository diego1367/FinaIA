using CreditRequests.Domain;

namespace CreditRequests.Application.Interfaces;

public interface ICreditRequestRepository
{
    Task<IReadOnlyList<CreditRequest>> GetAllAsync(CancellationToken cancellationToken);
    Task<CreditRequest?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(CreditRequest request, CancellationToken cancellationToken);

    /// <returns>False if the request has changed in storage since it was read.</returns>
    Task<bool> TryUpdateStatusAsync(CreditRequest request, CancellationToken cancellationToken);
}
