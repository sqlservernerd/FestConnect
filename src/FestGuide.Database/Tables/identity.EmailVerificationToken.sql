-- Email verification tokens for user account verification
CREATE TABLE [identity].[EmailVerificationToken]
(
    [TokenId]       UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT [DF_EmailVerificationToken_TokenId] DEFAULT (NEWSEQUENTIALID()),
    [UserId]        UNIQUEIDENTIFIER    NOT NULL,
    [TokenHash]     NVARCHAR(500)       NOT NULL,
    [ExpiresAtUtc]  DATETIME2(7)        NOT NULL,
    [IsUsed]        BIT                 NOT NULL    CONSTRAINT [DF_EmailVerificationToken_IsUsed] DEFAULT (0),
    [UsedAtUtc]     DATETIME2(7)        NULL,
    [CreatedAtUtc]  DATETIME2(7)        NOT NULL    CONSTRAINT [DF_EmailVerificationToken_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]     UNIQUEIDENTIFIER    NULL,
    [ModifiedAtUtc] DATETIME2(7)        NOT NULL    CONSTRAINT [DF_EmailVerificationToken_ModifiedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedBy]    UNIQUEIDENTIFIER    NULL,

    CONSTRAINT [PK_EmailVerificationToken] PRIMARY KEY CLUSTERED ([TokenId]),
    CONSTRAINT [FK_EmailVerificationToken_User] FOREIGN KEY ([UserId]) REFERENCES [identity].[User] ([UserId])
);
GO

-- Index for token lookups by hash
CREATE NONCLUSTERED INDEX [IX_EmailVerificationToken_TokenHash]
ON [identity].[EmailVerificationToken] ([TokenHash])
WHERE ([IsUsed] = 0);
GO

-- Index for cleanup of expired tokens
CREATE NONCLUSTERED INDEX [IX_EmailVerificationToken_ExpiresAtUtc]
ON [identity].[EmailVerificationToken] ([ExpiresAtUtc])
WHERE ([IsUsed] = 0);
GO
