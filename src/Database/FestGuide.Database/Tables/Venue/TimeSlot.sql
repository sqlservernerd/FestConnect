-- =======================================================
-- Table: venue.TimeSlot
-- Description: Blocks of time on a stage for performances
-- =======================================================
CREATE TABLE [venue].[TimeSlot]
(
    [TimeSlotId]            UNIQUEIDENTIFIER    NOT NULL,
    [StageId]               UNIQUEIDENTIFIER    NOT NULL,
    [EditionId]             UNIQUEIDENTIFIER    NOT NULL,
    [StartTimeUtc]          DATETIME2(7)        NOT NULL,
    [EndTimeUtc]            DATETIME2(7)        NOT NULL,
    [SlotType]              NVARCHAR(20)        NOT NULL    CONSTRAINT [DF_TimeSlot_SlotType] DEFAULT ('performance'), -- 'performance' or 'changeover'
    [IsDeleted]             BIT                 NOT NULL    CONSTRAINT [DF_TimeSlot_IsDeleted] DEFAULT (0),
    [DeletedAtUtc]          DATETIME2(7)        NULL,
    [CreatedAtUtc]          DATETIME2(7)        NOT NULL    CONSTRAINT [DF_TimeSlot_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]             UNIQUEIDENTIFIER    NULL,
    [ModifiedAtUtc]         DATETIME2(7)        NOT NULL    CONSTRAINT [DF_TimeSlot_ModifiedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedBy]            UNIQUEIDENTIFIER    NULL,

    CONSTRAINT [PK_TimeSlot] PRIMARY KEY CLUSTERED ([TimeSlotId]),
    CONSTRAINT [FK_TimeSlot_Stage] FOREIGN KEY ([StageId]) REFERENCES [venue].[Stage]([StageId]),
    CONSTRAINT [FK_TimeSlot_Edition] FOREIGN KEY ([EditionId]) REFERENCES [core].[FestivalEdition]([EditionId]),
    CONSTRAINT [CK_TimeSlot_Times] CHECK ([EndTimeUtc] > [StartTimeUtc]),
    CONSTRAINT [CK_TimeSlot_SlotType] CHECK ([SlotType] IN ('performance', 'changeover'))
);
GO

-- Index for edition time slot lookups
CREATE NONCLUSTERED INDEX [IX_TimeSlot_EditionId]
    ON [venue].[TimeSlot]([EditionId])
    WHERE [IsDeleted] = 0;
GO

-- Index for stage time slot lookups
CREATE NONCLUSTERED INDEX [IX_TimeSlot_StageId]
    ON [venue].[TimeSlot]([StageId])
    WHERE [IsDeleted] = 0;
GO

-- Index for time-based queries (schedule display)
CREATE NONCLUSTERED INDEX [IX_TimeSlot_StartTimeUtc]
    ON [venue].[TimeSlot]([EditionId], [StartTimeUtc], [EndTimeUtc])
    WHERE [IsDeleted] = 0;
GO
