using CreditRequests.Application.Contracts;

namespace CreditRequests.Application.Interfaces;

public interface ICreditRequestService
{
    Task<IReadOnlyList<CreditRequestResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<CreditRequestResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CreditRequestResponseDto> CreateAsync(CreateCreditRequestCommand command, CancellationToken cancellationToken);
    Task<SetCreditRequestStatusResult> SetStatusAsync(int id, string? status, CancellationToken cancellationToken);
}
