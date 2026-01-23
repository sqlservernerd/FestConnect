-- =======================================================
-- Table: notifications.NotificationPreference
-- Description: User notification preferences
-- =======================================================
CREATE TABLE [notifications].[NotificationPreference]
(
    [NotificationPreferenceId]  UNIQUEIDENTIFIER    NOT NULL,
    [UserId]                    UNIQUEIDENTIFIER    NOT NULL,
    [PushEnabled]               BIT                 NOT NULL    CONSTRAINT [DF_NotificationPreference_PushEnabled] DEFAULT (1),
    [EmailEnabled]              BIT                 NOT NULL    CONSTRAINT [DF_NotificationPreference_EmailEnabled] DEFAULT (1),
    [ScheduleChangesEnabled]    BIT                 NOT NULL    CONSTRAINT [DF_NotificationPreference_ScheduleChangesEnabled] DEFAULT (1),
    [RemindersEnabled]          BIT                 NOT NULL    CONSTRAINT [DF_NotificationPreference_RemindersEnabled] DEFAULT (1),
    [ReminderMinutesBefore]     INT                 NOT NULL    CONSTRAINT [DF_NotificationPreference_ReminderMinutesBefore] DEFAULT (30),
    [AnnouncementsEnabled]      BIT                 NOT NULL    CONSTRAINT [DF_NotificationPreference_AnnouncementsEnabled] DEFAULT (1),
    [QuietHoursStart]           TIME(0)             NULL,
    [QuietHoursEnd]             TIME(0)             NULL,
    [TimeZoneId]                NVARCHAR(100)       NOT NULL    CONSTRAINT [DF_NotificationPreference_TimeZoneId] DEFAULT ('UTC'),
    [CreatedAtUtc]              DATETIME2(7)        NOT NULL    CONSTRAINT [DF_NotificationPreference_CreatedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy]                 UNIQUEIDENTIFIER    NULL,
    [ModifiedAtUtc]             DATETIME2(7)        NOT NULL    CONSTRAINT [DF_NotificationPreference_ModifiedAtUtc] DEFAULT (SYSUTCDATETIME()),
    [ModifiedBy]                UNIQUEIDENTIFIER    NULL,

    CONSTRAINT [PK_NotificationPreference] PRIMARY KEY CLUSTERED ([NotificationPreferenceId]),
    CONSTRAINT [FK_NotificationPreference_User] FOREIGN KEY ([UserId]) REFERENCES [identity].[User]([UserId]),
    CONSTRAINT [CK_NotificationPreference_ReminderMinutesBefore] CHECK ([ReminderMinutesBefore] >= 5 AND [ReminderMinutesBefore] <= 120)
);
GO

-- Unique constraint: one preference record per user
CREATE UNIQUE NONCLUSTERED INDEX [UQ_NotificationPreference_UserId]
    ON [notifications].[NotificationPreference]([UserId]);
GO
