-- RefreshToken table for JWT refresh token management
CREATE TABLE [identity].[RefreshToken]
(
    [RefreshTokenId]        UNIQUEIDENTIFIER    NOT NULL    CONSTRAINT [DF_RefreshToken_RefreshTokenId] DEFAULT (NEWSEQUENTIALID()),
    [UserId]                UNIQUEIDENTIFIER    NOT NULL,
    [TokenHash]             NVARCHAR(100)       NOT NULL,
    [ExpiresAtUtc]          DATETIME2(7)        NOT NULL,
    [IsRevoked]             BIT                 NOT NULL    CONSTRAINT [DF_RefreshToken_IsRevoked] DEFAULT (0),
    [RevokedAtUtc]          DATETIME2(7)        NULL,
    [ReplacedByTokenId]     UNIQUEIDENTIFIER    NULL,
    [CreatedAtUtc]          DATETIME2(7)        NOT NULL    CONSTRAINT [DF_RefreshToken_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedByIp]           NVARCHAR(45)        NULL,

    CONSTRAINT [PK_RefreshToken] PRIMARY KEY CLUSTERED ([RefreshTokenId]),
    CONSTRAINT [FK_RefreshToken_User] FOREIGN KEY ([UserId]) REFERENCES [identity].[User] ([UserId]) ON DELETE CASCADE,
    CONSTRAINT [FK_RefreshToken_ReplacedBy] FOREIGN KEY ([ReplacedByTokenId]) REFERENCES [identity].[RefreshToken] ([RefreshTokenId])
);
GO

-- Index for token hash lookups
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefreshToken_TokenHash]
ON [identity].[RefreshToken] ([TokenHash]);
GO

-- Index for user token lookups
CREATE NONCLUSTERED INDEX [IX_RefreshToken_UserId_IsRevoked]
ON [identity].[RefreshToken] ([UserId], [IsRevoked])
INCLUDE ([ExpiresAtUtc]);
GO
