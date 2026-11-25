IF DB_ID(N'CreditRequestsDemo') IS NULL
    CREATE DATABASE [CreditRequestsDemo];
GO

USE [CreditRequestsDemo];
GO

IF OBJECT_ID(N'dbo.CreditRequests', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CreditRequests (
        Id int IDENTITY(1, 1) NOT NULL CONSTRAINT PK_CreditRequests PRIMARY KEY,
        DocumentNumber nvarchar(20) NOT NULL,
        FullName nvarchar(100) NOT NULL,
        RequestedAmount decimal(15, 2) NOT NULL,
        TermMonths int NOT NULL,
        Status nvarchar(20) NOT NULL,
        CreatedAtUtc datetime2 NOT NULL,
        RowVersion rowversion NOT NULL,
        CONSTRAINT CK_CreditRequests_Amount CHECK (RequestedAmount > 0),
        CONSTRAINT CK_CreditRequests_Term CHECK (TermMonths BETWEEN 1 AND 120),
        CONSTRAINT CK_CreditRequests_Status CHECK (Status IN (N'Pending', N'Approved', N'Rejected'))
    );

    CREATE INDEX IX_CreditRequests_CreatedAtUtc_Id ON dbo.CreditRequests (CreatedAtUtc DESC, Id DESC);
END;
GO
