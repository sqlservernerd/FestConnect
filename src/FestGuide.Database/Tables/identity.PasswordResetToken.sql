-- Password reset tokens for user password recovery
CREATE TABLE [identity].[PasswordResetToken]
(
    [TokenId]       UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT [DF_PasswordResetToken_TokenId] DEFAULT (NEWSEQUENTIALID()),
    [UserId]        UNIQUEIDENTIFIER    NOT NULL,
    [TokenHash]     NVARCHAR(500)       NOT NULL,
    [ExpiresAtUtc]  DATETIME2(7)        NOT NULL,
    [IsUsed]        BIT                 NOT NULL    CONSTRAINT [DF_PasswordResetToken_IsUsed] DEFAULT (0),
    [UsedAtUtc]     DATETIME2(7)        NULL,
    [CreatedAtUtc]  DATETIME2(7)        NOT NULL    CONSTRAINT [DF_PasswordResetToken_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]     UNIQUEIDENTIFIER    NULL,
    [ModifiedAtUtc] DATETIME2(7)        NOT NULL    CONSTRAINT [DF_PasswordResetToken_ModifiedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedBy]    UNIQUEIDENTIFIER    NULL,

    CONSTRAINT [PK_PasswordResetToken] PRIMARY KEY CLUSTERED ([TokenId]),
    CONSTRAINT [FK_PasswordResetToken_User] FOREIGN KEY ([UserId]) REFERENCES [identity].[User] ([UserId])
);
GO

-- Index for token lookups by hash
CREATE NONCLUSTERED INDEX [IX_PasswordResetToken_TokenHash]
ON [identity].[PasswordResetToken] ([TokenHash])
WHERE ([IsUsed] = 0);
GO

-- Index for cleanup of expired tokens
CREATE NONCLUSTERED INDEX [IX_PasswordResetToken_ExpiresAtUtc]
ON [identity].[PasswordResetToken] ([ExpiresAtUtc])
WHERE ([IsUsed] = 0);
GO
