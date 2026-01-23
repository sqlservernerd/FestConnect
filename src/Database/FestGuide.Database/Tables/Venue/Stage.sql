-- =======================================================
-- Table: venue.Stage
-- Description: Performance areas within a venue
-- =======================================================
CREATE TABLE [venue].[Stage]
(
    [StageId]               UNIQUEIDENTIFIER    NOT NULL,
    [VenueId]               UNIQUEIDENTIFIER    NOT NULL,
    [Name]                  NVARCHAR(200)       NOT NULL,
    [Description]           NVARCHAR(MAX)       NULL,
    [SortOrder]             INT                 NOT NULL    CONSTRAINT [DF_Stage_SortOrder] DEFAULT (0),
    [IsDeleted]             BIT                 NOT NULL    CONSTRAINT [DF_Stage_IsDeleted] DEFAULT (0),
    [DeletedAtUtc]          DATETIME2(7)        NULL,
    [CreatedAtUtc]          DATETIME2(7)        NOT NULL    CONSTRAINT [DF_Stage_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]             UNIQUEIDENTIFIER    NULL,
    [ModifiedAtUtc]         DATETIME2(7)        NOT NULL    CONSTRAINT [DF_Stage_ModifiedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedBy]            UNIQUEIDENTIFIER    NULL,

    CONSTRAINT [PK_Stage] PRIMARY KEY CLUSTERED ([StageId]),
    CONSTRAINT [FK_Stage_Venue] FOREIGN KEY ([VenueId]) REFERENCES [venue].[Venue]([VenueId])
);
GO

-- Index for venue stage lookups
CREATE NONCLUSTERED INDEX [IX_Stage_VenueId]
    ON [venue].[Stage]([VenueId])
    WHERE [IsDeleted] = 0;
GO

-- Index for ordering stages
CREATE NONCLUSTERED INDEX [IX_Stage_SortOrder]
    ON [venue].[Stage]([VenueId], [SortOrder])
    WHERE [IsDeleted] = 0;
GO
