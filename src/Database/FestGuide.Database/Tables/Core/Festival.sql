-- =======================================================
-- Table: core.Festival
-- Description: Recurring festival brands
-- =======================================================
CREATE TABLE [core].[Festival]
(
    [FestivalId]            UNIQUEIDENTIFIER    NOT NULL,
    [Name]                  NVARCHAR(200)       NOT NULL,
    [Description]           NVARCHAR(MAX)       NULL,
    [ImageUrl]              NVARCHAR(2000)      NULL,
    [WebsiteUrl]            NVARCHAR(2000)      NULL,
    [OwnerUserId]           UNIQUEIDENTIFIER    NOT NULL,
    [IsDeleted]             BIT                 NOT NULL    CONSTRAINT [DF_Festival_IsDeleted] DEFAULT (0),
    [DeletedAtUtc]          DATETIME2(7)        NULL,
    [CreatedAtUtc]          DATETIME2(7)        NOT NULL    CONSTRAINT [DF_Festival_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]             UNIQUEIDENTIFIER    NULL,
    [ModifiedAtUtc]         DATETIME2(7)        NOT NULL    CONSTRAINT [DF_Festival_ModifiedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedBy]            UNIQUEIDENTIFIER    NULL,

    CONSTRAINT [PK_Festival] PRIMARY KEY CLUSTERED ([FestivalId]),
    CONSTRAINT [FK_Festival_OwnerUser] FOREIGN KEY ([OwnerUserId]) REFERENCES [identity].[User]([UserId])
);
GO

-- Index for owner lookups
CREATE NONCLUSTERED INDEX [IX_Festival_OwnerUserId]
    ON [core].[Festival]([OwnerUserId])
    WHERE [IsDeleted] = 0;
GO

-- Index for active festivals
CREATE NONCLUSTERED INDEX [IX_Festival_IsDeleted]
    ON [core].[Festival]([IsDeleted])
    INCLUDE ([FestivalId], [Name], [OwnerUserId]);
GO

-- Full-text search support (name)
CREATE NONCLUSTERED INDEX [IX_Festival_Name]
    ON [core].[Festival]([Name])
    WHERE [IsDeleted] = 0;
GO
