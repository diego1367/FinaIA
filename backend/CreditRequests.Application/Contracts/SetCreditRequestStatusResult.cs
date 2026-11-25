namespace CreditRequests.Application.Contracts;

public abstract record SetCreditRequestStatusResult
{
    private SetCreditRequestStatusResult() { }

    public sealed record Updated(CreditRequestResponseDto Request) : SetCreditRequestStatusResult;
    public sealed record InvalidStatus : SetCreditRequestStatusResult;
    public sealed record NotFound : SetCreditRequestStatusResult;
    public sealed record AlreadyResolved : SetCreditRequestStatusResult;
    public sealed record ConcurrentChange : SetCreditRequestStatusResult;
}
