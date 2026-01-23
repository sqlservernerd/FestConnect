-- =======================================================
-- Table: schedule.Schedule
-- Description: Master schedule for a festival edition
-- =======================================================
CREATE TABLE [schedule].[Schedule]
(
    [ScheduleId]            UNIQUEIDENTIFIER    NOT NULL,
    [EditionId]             UNIQUEIDENTIFIER    NOT NULL,
    [Status]                NVARCHAR(20)        NOT NULL    CONSTRAINT [DF_Schedule_Status] DEFAULT ('draft'), -- 'draft' or 'published'
    [Version]               INT                 NOT NULL    CONSTRAINT [DF_Schedule_Version] DEFAULT (1),
    [PublishedAtUtc]        DATETIME2(7)        NULL,
    [PublishedBy]           UNIQUEIDENTIFIER    NULL,
    [LastPublishedVersion]  INT                 NULL,       -- Last version that was published
    [CreatedAtUtc]          DATETIME2(7)        NOT NULL    CONSTRAINT [DF_Schedule_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]             UNIQUEIDENTIFIER    NULL,
    [ModifiedAtUtc]         DATETIME2(7)        NOT NULL    CONSTRAINT [DF_Schedule_ModifiedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedBy]            UNIQUEIDENTIFIER    NULL,

    CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED ([ScheduleId]),
    CONSTRAINT [FK_Schedule_Edition] FOREIGN KEY ([EditionId]) REFERENCES [core].[FestivalEdition]([EditionId]),
    CONSTRAINT [FK_Schedule_PublishedBy] FOREIGN KEY ([PublishedBy]) REFERENCES [identity].[User]([UserId]),
    CONSTRAINT [CK_Schedule_Status] CHECK ([Status] IN ('draft', 'published'))
);
GO

-- Unique constraint: one schedule per edition
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Schedule_EditionId]
    ON [schedule].[Schedule]([EditionId]);
GO

-- Index for published schedules
CREATE NONCLUSTERED INDEX [IX_Schedule_PublishedAtUtc]
    ON [schedule].[Schedule]([PublishedAtUtc])
    WHERE [PublishedAtUtc] IS NOT NULL;
GO
