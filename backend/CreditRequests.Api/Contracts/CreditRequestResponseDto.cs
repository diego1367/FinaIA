using CreditRequests.Api.Domain;

namespace CreditRequests.Api.Contracts;

public sealed record CreditRequestResponseDto(
    int Id,
    string DocumentNumber,
    string FullName,
    decimal RequestedAmount,
    int TermMonths,
    string Status,
    DateTime CreatedAtUtc)
{
    public static CreditRequestResponseDto FromEntity(CreditRequest request) => new(
        request.Id,
        request.DocumentNumber,
        request.FullName,
        request.RequestedAmount,
        request.TermMonths,
        request.Status.ToString(),
        DateTime.SpecifyKind(request.CreatedAtUtc, DateTimeKind.Utc));
}
