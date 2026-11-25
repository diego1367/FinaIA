using CreditRequests.Application.Contracts;
using CreditRequests.Application.Interfaces;
using CreditRequests.Domain;

namespace CreditRequests.Application.Services;

public sealed class CreditRequestService(ICreditRequestRepository repository) : ICreditRequestService
{
    public async Task<IReadOnlyList<CreditRequestResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var requests = await repository.GetAllAsync(cancellationToken);
        return requests.Select(ToResponse).ToArray();
    }

    public async Task<CreditRequestResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var request = await repository.GetByIdAsync(id, cancellationToken);
        return request is null ? null : ToResponse(request);
    }

    public async Task<CreditRequestResponseDto> CreateAsync(
        CreateCreditRequestCommand command,
        CancellationToken cancellationToken)
    {
        var request = new CreditRequest(
            command.DocumentNumber,
            command.FullName.Trim(),
            command.RequestedAmount,
            command.TermMonths);

        await repository.AddAsync(request, cancellationToken);
        return ToResponse(request);
    }

    public async Task<SetCreditRequestStatusResult> SetStatusAsync(
        int id,
        string? status,
        CancellationToken cancellationToken)
    {
        var decision = status switch
        {
            nameof(CreditRequestStatus.Approved) => CreditRequestStatus.Approved,
            nameof(CreditRequestStatus.Rejected) => CreditRequestStatus.Rejected,
            _ => (CreditRequestStatus?)null
        };

        if (decision is null)
            return new SetCreditRequestStatusResult.InvalidStatus();

        var request = await repository.GetByIdAsync(id, cancellationToken);
        if (request is null)
            return new SetCreditRequestStatusResult.NotFound();

        if (!request.TrySetStatus(decision.Value))
            return new SetCreditRequestStatusResult.AlreadyResolved();

        if (!await repository.TryUpdateStatusAsync(request, cancellationToken))
            return new SetCreditRequestStatusResult.ConcurrentChange();

        return new SetCreditRequestStatusResult.Updated(ToResponse(request));
    }

    private static CreditRequestResponseDto ToResponse(CreditRequest request) => new(
        request.Id,
        request.DocumentNumber,
        request.FullName,
        request.RequestedAmount,
        request.TermMonths,
        request.Status.ToString(),
        DateTime.SpecifyKind(request.CreatedAtUtc, DateTimeKind.Utc));
}
