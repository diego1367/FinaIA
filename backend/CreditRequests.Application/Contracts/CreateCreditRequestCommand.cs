namespace CreditRequests.Application.Contracts;

public sealed record CreateCreditRequestCommand(
    string DocumentNumber,
    string FullName,
    decimal RequestedAmount,
    int TermMonths);
