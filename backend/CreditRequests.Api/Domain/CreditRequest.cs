namespace CreditRequests.Api.Domain;

public enum CreditRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public sealed class CreditRequest
{
    private CreditRequest() { }

    public CreditRequest(string documentNumber, string fullName, decimal requestedAmount, int termMonths)
    {
        DocumentNumber = documentNumber;
        FullName = fullName;
        RequestedAmount = requestedAmount;
        TermMonths = termMonths;
        Status = CreditRequestStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public decimal RequestedAmount { get; private set; }
    public int TermMonths { get; private set; }
    public CreditRequestStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public bool TrySetStatus(CreditRequestStatus status)
    {
        if (Status != CreditRequestStatus.Pending ||
            status is not (CreditRequestStatus.Approved or CreditRequestStatus.Rejected))
            return false;

        Status = status;
        return true;
    }
}
