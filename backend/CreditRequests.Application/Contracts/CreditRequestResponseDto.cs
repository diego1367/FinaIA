namespace CreditRequests.Application.Contracts;

public sealed record CreditRequestResponseDto(
    int Id,
    string DocumentNumber,
    string FullName,
    decimal RequestedAmount,
    int TermMonths,
    string Status,
    DateTime CreatedAtUtc);
