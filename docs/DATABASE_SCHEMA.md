# 🎵 FestGuide - Database Schema

---

## Document Control

| **Document Title** | FestGuide - Database Schema |
|---|---|
| **Version** | 1.0 |
| **Date** | 2026-01-20 |
| **Status** | Draft |
| **Author** | Project Team |
| **Document Owner** | sqlservernerd |

---

## 1. Overview

### 1.1 Database Platform

| **Attribute** | **Value** |
|---|---|
| Database Engine | SQL Server 2022 |
| Schema Management | SQL Server Data Tools (SSDT) |
| ORM | Dapper (Micro-ORM) |
| Portability | PostgreSQL-compatible design |

### 1.2 Design Principles

| **Principle** | **Description** |
|---|---|
| UTC Timestamps | All datetime columns store UTC values |
| IANA Timezones | Timezone identifiers stored as IANA strings |
| Soft Deletes | Where applicable, use `IsDeleted` flag |
| Audit Columns | All tables include `CreatedAtUtc`, `CreatedBy`, `ModifiedAtUtc`, `ModifiedBy` |
| GUID Primary Keys | Use `NEWSEQUENTIALID()` for clustered index performance |

---

## 2. Schema Organization

| **Schema** | **Purpose** | **Tables** |
|---|---|---|
| `identity` | User accounts and authentication | User, RefreshToken |
| `core` | Core domain entities | Festival, FestivalEdition, Artist |
| `venue` | Venue and stage management | Venue, Stage, TimeSlot |
| `schedule` | Schedule and engagements | Schedule, Engagement |
| `permissions` | Authorization | FestivalPermission, PermissionScope |
| `attendee` | Attendee-specific data | PersonalSchedule, PersonalScheduleEntry |
| `notifications` | Push notification management | DeviceToken, NotificationLog |
| `integrations` | External integrations | ApiKey, WebhookSubscription |
| `audit` | Audit logging | AuditLog |

---

## 3. Naming Conventions

| **Object Type** | **Convention** | **Example** |
|---|---|---|
| Tables | PascalCase, singular | `Festival`, `TimeSlot` |
| Columns | PascalCase | `FestivalId`, `CreatedAtUtc` |
| DateTime columns | Suffix `Utc` | `StartTimeUtc`, `EndTimeUtc` |
| Timezone columns | `TimezoneId` | IANA identifier |
| Primary keys | `{Table}Id` | `FestivalId`, `UserId` |
| Foreign keys | `{ReferencedTable}Id` | `FestivalId` in `FestivalEdition` |
| Indexes | `IX_{Table}_{Columns}` | `IX_Festival_Name` |
| Unique constraints | `UQ_{Table}_{Columns}` | `UQ_User_Email` |
| Check constraints | `CK_{Table}_{Description}` | `CK_TimeSlot_EndAfterStart` |
| Default constraints | `DF_{Table}_{Column}` | `DF_Festival_CreatedAtUtc` |
| Foreign key constraints | `FK_{Child}_{Parent}` | `FK_FestivalEdition_Festival` |

---

## 4. Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              IDENTITY SCHEMA                                     │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│   ┌──────────────┐              ┌───────────────────┐                           │
│   │     User     │──────1:N────►│   RefreshToken    │                           │
│   └──────────────┘              └───────────────────┘                           │
│          │                                                                       │
└──────────┼───────────────────────────────────────────────────────────────────────┘
           │
           │ 1:N
           ▼
┌─────────────────────────────────────────────────────────────────────────────────┐
│                                CORE SCHEMA                                       │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│   ┌──────────────┐              ┌───────────────────┐       ┌──────────────┐    │
│   │   Festival   │──────1:N────►│ FestivalEdition   │◄──N:M─│    Venue     │    │
│   └──────────────┘              └───────────────────┘       └──────────────┘    │
│          │                              │                          │            │
│          │                              │                          │            │
│          │ 1:N                          │ 1:1                      │ 1:N        │
│          ▼                              ▼                          ▼            │
│   ┌──────────────┐              ┌───────────────────┐       ┌──────────────┐    │
│   │    Artist    │              │     Schedule      │       │    Stage     │    │
│   └──────────────┘              └───────────────────┘       └──────────────┘    │
│          │                                                         │            │
│          │                                                         │ 1:N        │
│          │                                                         ▼            │
│          │                                                  ┌──────────────┐    │
│          └────────────────────────────N:1──────────────────►│   TimeSlot   │    │
│                                                             └──────────────┘    │
│                                                                    │            │
│                                                                    │ 1:1        │
│                                                                    ▼            │
│                                                             ┌──────────────┐    │
│                                                             │  Engagement  │    │
│                                                             └──────────────┘    │
└─────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────┐
│                             ATTENDEE SCHEMA                                      │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                  │
│   ┌──────────────────────┐              ┌─────────────────────────┐             │
│   │   PersonalSchedule   │──────1:N────►│  PersonalScheduleEntry  │             │
│   │   (User + Edition)   │              │  (references Engagement)│             │
│   └──────────────────────┘              └─────────────────────────┘             │
│                                                                                  │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## 5. Table Definitions

### 5.1 Identity Schema

#### 5.1.1 User

Stores user account information.

```sql
CREATE TABLE [identity].[User]
(
    [UserId]                UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [Email]                 NVARCHAR(256)       NOT NULL,
    [EmailNormalized]       NVARCHAR(256)       NOT NULL,
    [EmailVerified]         BIT                 NOT NULL    DEFAULT 0,
    [PasswordHash]          NVARCHAR(500)       NOT NULL,
    [DisplayName]           NVARCHAR(100)       NOT NULL,
    [UserType]              NVARCHAR(20)        NOT NULL,   -- 'attendee' or 'organizer'
    [PreferredTimezoneId]   NVARCHAR(100)       NULL,       -- IANA timezone
    [IsDeleted]             BIT                 NOT NULL    DEFAULT 0,
    [DeletedAtUtc]          DATETIME2(7)        NULL,
    [CreatedAtUtc]          DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]             UNIQUEIDENTIFIER    NULL,
    [ModifiedAtUtc]         DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [ModifiedBy]            UNIQUEIDENTIFIER    NULL,

    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([UserId]),
    CONSTRAINT [UQ_User_Email] UNIQUE ([EmailNormalized]),
    CONSTRAINT [CK_User_UserType] CHECK ([UserType] IN ('attendee', 'organizer'))
);

CREATE INDEX [IX_User_Email] ON [identity].[User] ([EmailNormalized]);
CREATE INDEX [IX_User_UserType] ON [identity].[User] ([UserType]) WHERE [IsDeleted] = 0;
```

| **Column** | **Type** | **Description** |
|---|---|---|
| UserId | UNIQUEIDENTIFIER | Primary key |
| Email | NVARCHAR(256) | User's email address |
| EmailNormalized | NVARCHAR(256) | Lowercase email for uniqueness |
| EmailVerified | BIT | Email verification status |
| PasswordHash | NVARCHAR(500) | Argon2id password hash |
| DisplayName | NVARCHAR(100) | User's display name |
| UserType | NVARCHAR(20) | Account type: attendee or organizer |
| PreferredTimezoneId | NVARCHAR(100) | IANA timezone identifier |

---

#### 5.1.2 RefreshToken

Stores refresh tokens for JWT authentication.

```sql
CREATE TABLE [identity].[RefreshToken]
(
    [RefreshTokenId]    UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [UserId]            UNIQUEIDENTIFIER    NOT NULL,
    [TokenHash]         NVARCHAR(500)       NOT NULL,
    [ExpiresAtUtc]      DATETIME2(7)        NOT NULL,
    [IsRevoked]         BIT                 NOT NULL    DEFAULT 0,
    [RevokedAtUtc]      DATETIME2(7)        NULL,
    [ReplacedByTokenId] UNIQUEIDENTIFIER    NULL,
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedByIp]       NVARCHAR(45)        NULL,

    CONSTRAINT [PK_RefreshToken] PRIMARY KEY CLUSTERED ([RefreshTokenId]),
    CONSTRAINT [FK_RefreshToken_User] FOREIGN KEY ([UserId]) REFERENCES [identity].[User]([UserId])
);

CREATE INDEX [IX_RefreshToken_UserId] ON [identity].[RefreshToken] ([UserId]);
CREATE INDEX [IX_RefreshToken_TokenHash] ON [identity].[RefreshToken] ([TokenHash]);
```

---

### 5.2 Core Schema

#### 5.2.1 Festival

Represents a recurring festival brand.

```sql
CREATE TABLE [core].[Festival]
(
    [FestivalId]        UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [Name]              NVARCHAR(200)       NOT NULL,
    [Description]       NVARCHAR(MAX)       NULL,
    [ImageUrl]          NVARCHAR(500)       NULL,
    [WebsiteUrl]        NVARCHAR(500)       NULL,
    [OwnerUserId]       UNIQUEIDENTIFIER    NOT NULL,
    [IsDeleted]         BIT                 NOT NULL    DEFAULT 0,
    [DeletedAtUtc]      DATETIME2(7)        NULL,
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
    [ModifiedAtUtc]     DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [ModifiedBy]        UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT [PK_Festival] PRIMARY KEY CLUSTERED ([FestivalId]),
    CONSTRAINT [FK_Festival_Owner] FOREIGN KEY ([OwnerUserId]) REFERENCES [identity].[User]([UserId])
);

CREATE INDEX [IX_Festival_Name] ON [core].[Festival] ([Name]) WHERE [IsDeleted] = 0;
CREATE INDEX [IX_Festival_Owner] ON [core].[Festival] ([OwnerUserId]) WHERE [IsDeleted] = 0;
```

---

#### 5.2.2 FestivalEdition

Represents a specific instance of a festival with dates.

```sql
CREATE TABLE [core].[FestivalEdition]
(
    [EditionId]         UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [FestivalId]        UNIQUEIDENTIFIER    NOT NULL,
    [Name]              NVARCHAR(200)       NOT NULL,
    [StartDateUtc]      DATETIME2(7)        NOT NULL,
    [EndDateUtc]        DATETIME2(7)        NOT NULL,
    [TimezoneId]        NVARCHAR(100)       NOT NULL,   -- IANA timezone
    [TicketUrl]         NVARCHAR(500)       NULL,
    [Status]            NVARCHAR(20)        NOT NULL    DEFAULT 'draft',
    [IsDeleted]         BIT                 NOT NULL    DEFAULT 0,
    [DeletedAtUtc]      DATETIME2(7)        NULL,
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
    [ModifiedAtUtc]     DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [ModifiedBy]        UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT [PK_FestivalEdition] PRIMARY KEY CLUSTERED ([EditionId]),
    CONSTRAINT [FK_FestivalEdition_Festival] FOREIGN KEY ([FestivalId]) REFERENCES [core].[Festival]([FestivalId]),
    CONSTRAINT [CK_FestivalEdition_Dates] CHECK ([EndDateUtc] >= [StartDateUtc]),
    CONSTRAINT [CK_FestivalEdition_Status] CHECK ([Status] IN ('draft', 'published', 'archived'))
);

CREATE INDEX [IX_FestivalEdition_Festival] ON [core].[FestivalEdition] ([FestivalId]) WHERE [IsDeleted] = 0;
CREATE INDEX [IX_FestivalEdition_Dates] ON [core].[FestivalEdition] ([StartDateUtc], [EndDateUtc]) WHERE [IsDeleted] = 0;
```

---

#### 5.2.3 Artist

Represents a performer, scoped to a festival.

```sql
CREATE TABLE [core].[Artist]
(
    [ArtistId]          UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [FestivalId]        UNIQUEIDENTIFIER    NOT NULL,
    [Name]              NVARCHAR(200)       NOT NULL,
    [Genre]             NVARCHAR(100)       NULL,
    [Bio]               NVARCHAR(MAX)       NULL,
    [ImageUrl]          NVARCHAR(500)       NULL,
    [WebsiteUrl]        NVARCHAR(500)       NULL,
    [SpotifyUrl]        NVARCHAR(500)       NULL,
    [IsDeleted]         BIT                 NOT NULL    DEFAULT 0,
    [DeletedAtUtc]      DATETIME2(7)        NULL,
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
    [ModifiedAtUtc]     DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [ModifiedBy]        UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT [PK_Artist] PRIMARY KEY CLUSTERED ([ArtistId]),
    CONSTRAINT [FK_Artist_Festival] FOREIGN KEY ([FestivalId]) REFERENCES [core].[Festival]([FestivalId])
);

CREATE INDEX [IX_Artist_Festival] ON [core].[Artist] ([FestivalId]) WHERE [IsDeleted] = 0;
CREATE INDEX [IX_Artist_Name] ON [core].[Artist] ([Name]) WHERE [IsDeleted] = 0;
```

---

### 5.3 Venue Schema

#### 5.3.1 Venue

Represents a physical location with stages.

```sql
CREATE TABLE [venue].[Venue]
(
    [VenueId]           UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [FestivalId]        UNIQUEIDENTIFIER    NOT NULL,
    [Name]              NVARCHAR(200)       NOT NULL,
    [Description]       NVARCHAR(MAX)       NULL,
    [Address]           NVARCHAR(500)       NULL,
    [Latitude]          DECIMAL(9,6)        NULL,
    [Longitude]         DECIMAL(9,6)        NULL,
    [IsDeleted]         BIT                 NOT NULL    DEFAULT 0,
    [DeletedAtUtc]      DATETIME2(7)        NULL,
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
    [ModifiedAtUtc]     DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [ModifiedBy]        UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT [PK_Venue] PRIMARY KEY CLUSTERED ([VenueId]),
    CONSTRAINT [FK_Venue_Festival] FOREIGN KEY ([FestivalId]) REFERENCES [core].[Festival]([FestivalId])
);

CREATE INDEX [IX_Venue_Festival] ON [venue].[Venue] ([FestivalId]) WHERE [IsDeleted] = 0;
```

---

#### 5.3.2 EditionVenue

Junction table linking editions to venues.

```sql
CREATE TABLE [venue].[EditionVenue]
(
    [EditionVenueId]    UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [EditionId]         UNIQUEIDENTIFIER    NOT NULL,
    [VenueId]           UNIQUEIDENTIFIER    NOT NULL,
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT [PK_EditionVenue] PRIMARY KEY CLUSTERED ([EditionVenueId]),
    CONSTRAINT [FK_EditionVenue_Edition] FOREIGN KEY ([EditionId]) REFERENCES [core].[FestivalEdition]([EditionId]),
    CONSTRAINT [FK_EditionVenue_Venue] FOREIGN KEY ([VenueId]) REFERENCES [venue].[Venue]([VenueId]),
    CONSTRAINT [UQ_EditionVenue] UNIQUE ([EditionId], [VenueId])
);
```

---

#### 5.3.3 Stage

Represents a performance area within a venue.

```sql
CREATE TABLE [venue].[Stage]
(
    [StageId]           UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [VenueId]           UNIQUEIDENTIFIER    NOT NULL,
    [Name]              NVARCHAR(200)       NOT NULL,
    [Description]       NVARCHAR(MAX)       NULL,
    [SortOrder]         INT                 NOT NULL    DEFAULT 0,
    [IsDeleted]         BIT                 NOT NULL    DEFAULT 0,
    [DeletedAtUtc]      DATETIME2(7)        NULL,
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
    [ModifiedAtUtc]     DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [ModifiedBy]        UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT [PK_Stage] PRIMARY KEY CLUSTERED ([StageId]),
    CONSTRAINT [FK_Stage_Venue] FOREIGN KEY ([VenueId]) REFERENCES [venue].[Venue]([VenueId])
);

CREATE INDEX [IX_Stage_Venue] ON [venue].[Stage] ([VenueId]) WHERE [IsDeleted] = 0;
```

---

#### 5.3.4 TimeSlot

Represents a block of time on a stage.

```sql
CREATE TABLE [venue].[TimeSlot]
(
    [TimeSlotId]        UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [StageId]           UNIQUEIDENTIFIER    NOT NULL,
    [EditionId]         UNIQUEIDENTIFIER    NOT NULL,
    [StartTimeUtc]      DATETIME2(7)        NOT NULL,
    [EndTimeUtc]        DATETIME2(7)        NOT NULL,
    [SlotType]          NVARCHAR(20)        NOT NULL    DEFAULT 'performance',
    [IsDeleted]         BIT                 NOT NULL    DEFAULT 0,
    [DeletedAtUtc]      DATETIME2(7)        NULL,
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
    [ModifiedAtUtc]     DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [ModifiedBy]        UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT [PK_TimeSlot] PRIMARY KEY CLUSTERED ([TimeSlotId]),
    CONSTRAINT [FK_TimeSlot_Stage] FOREIGN KEY ([StageId]) REFERENCES [venue].[Stage]([StageId]),
    CONSTRAINT [FK_TimeSlot_Edition] FOREIGN KEY ([EditionId]) REFERENCES [core].[FestivalEdition]([EditionId]),
    CONSTRAINT [CK_TimeSlot_Times] CHECK ([EndTimeUtc] > [StartTimeUtc]),
    CONSTRAINT [CK_TimeSlot_Type] CHECK ([SlotType] IN ('performance', 'changeover'))
);

CREATE INDEX [IX_TimeSlot_Stage_Edition] ON [venue].[TimeSlot] ([StageId], [EditionId]) WHERE [IsDeleted] = 0;
CREATE INDEX [IX_TimeSlot_Times] ON [venue].[TimeSlot] ([StartTimeUtc], [EndTimeUtc]) WHERE [IsDeleted] = 0;
```

---

### 5.4 Schedule Schema

#### 5.4.1 Schedule

Represents a master schedule for an edition.

```sql
CREATE TABLE [schedule].[Schedule]
(
    [ScheduleId]            UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [EditionId]             UNIQUEIDENTIFIER    NOT NULL,
    [Status]                NVARCHAR(20)        NOT NULL    DEFAULT 'draft',
    [PublishedAtUtc]        DATETIME2(7)        NULL,
    [PublishedBy]           UNIQUEIDENTIFIER    NULL,
    [Version]               INT                 NOT NULL    DEFAULT 1,
    [CreatedAtUtc]          DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]             UNIQUEIDENTIFIER    NOT NULL,
    [ModifiedAtUtc]         DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [ModifiedBy]            UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT [PK_Schedule] PRIMARY KEY CLUSTERED ([ScheduleId]),
    CONSTRAINT [FK_Schedule_Edition] FOREIGN KEY ([EditionId]) REFERENCES [core].[FestivalEdition]([EditionId]),
    CONSTRAINT [UQ_Schedule_Edition] UNIQUE ([EditionId]),
    CONSTRAINT [CK_Schedule_Status] CHECK ([Status] IN ('draft', 'published'))
);
```

---

#### 5.4.2 Engagement

Links an artist to a time slot.

```sql
CREATE TABLE [schedule].[Engagement]
(
    [EngagementId]      UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [TimeSlotId]        UNIQUEIDENTIFIER    NOT NULL,
    [ArtistId]          UNIQUEIDENTIFIER    NOT NULL,
    [IsHeadliner]       BIT                 NOT NULL    DEFAULT 0,
    [Notes]             NVARCHAR(500)       NULL,
    [IsDeleted]         BIT                 NOT NULL    DEFAULT 0,
    [DeletedAtUtc]      DATETIME2(7)        NULL,
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
    [ModifiedAtUtc]     DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [ModifiedBy]        UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT [PK_Engagement] PRIMARY KEY CLUSTERED ([EngagementId]),
    CONSTRAINT [FK_Engagement_TimeSlot] FOREIGN KEY ([TimeSlotId]) REFERENCES [venue].[TimeSlot]([TimeSlotId]),
    CONSTRAINT [FK_Engagement_Artist] FOREIGN KEY ([ArtistId]) REFERENCES [core].[Artist]([ArtistId]),
    CONSTRAINT [UQ_Engagement_TimeSlot] UNIQUE ([TimeSlotId])  -- One artist per time slot
);

CREATE INDEX [IX_Engagement_Artist] ON [schedule].[Engagement] ([ArtistId]) WHERE [IsDeleted] = 0;
```

---

### 5.5 Permissions Schema

#### 5.5.1 FestivalPermission

Stores user permissions for festivals.

```sql
CREATE TABLE [permissions].[FestivalPermission]
(
    [PermissionId]      UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [FestivalId]        UNIQUEIDENTIFIER    NOT NULL,
    [UserId]            UNIQUEIDENTIFIER    NOT NULL,
    [Role]              NVARCHAR(20)        NOT NULL,
    [InvitedByUserId]   UNIQUEIDENTIFIER    NULL,
    [AcceptedAtUtc]     DATETIME2(7)        NULL,
    [IsDeleted]         BIT                 NOT NULL    DEFAULT 0,
    [DeletedAtUtc]      DATETIME2(7)        NULL,
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,
    [ModifiedAtUtc]     DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [ModifiedBy]        UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT [PK_FestivalPermission] PRIMARY KEY CLUSTERED ([PermissionId]),
    CONSTRAINT [FK_FestivalPermission_Festival] FOREIGN KEY ([FestivalId]) REFERENCES [core].[Festival]([FestivalId]),
    CONSTRAINT [FK_FestivalPermission_User] FOREIGN KEY ([UserId]) REFERENCES [identity].[User]([UserId]),
    CONSTRAINT [UQ_FestivalPermission_User] UNIQUE ([FestivalId], [UserId]),
    CONSTRAINT [CK_FestivalPermission_Role] CHECK ([Role] IN ('owner', 'administrator', 'manager', 'viewer'))
);

CREATE INDEX [IX_FestivalPermission_User] ON [permissions].[FestivalPermission] ([UserId]) WHERE [IsDeleted] = 0;
```

---

#### 5.5.2 PermissionScope

Defines specific scopes for manager/viewer roles.

```sql
CREATE TABLE [permissions].[PermissionScope]
(
    [PermissionScopeId] UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [PermissionId]      UNIQUEIDENTIFIER    NOT NULL,
    [Scope]             NVARCHAR(50)        NOT NULL,

    CONSTRAINT [PK_PermissionScope] PRIMARY KEY CLUSTERED ([PermissionScopeId]),
    CONSTRAINT [FK_PermissionScope_Permission] FOREIGN KEY ([PermissionId]) REFERENCES [permissions].[FestivalPermission]([PermissionId]),
    CONSTRAINT [CK_PermissionScope_Scope] CHECK ([Scope] IN ('venues', 'schedule', 'artists', 'editions', 'integrations', 'all'))
);

CREATE INDEX [IX_PermissionScope_Permission] ON [permissions].[PermissionScope] ([PermissionId]);
```

---

### 5.6 Attendee Schema

#### 5.6.1 PersonalSchedule

Attendee's personal schedule for an edition.

```sql
CREATE TABLE [attendee].[PersonalSchedule]
(
    [PersonalScheduleId]    UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [UserId]                UNIQUEIDENTIFIER    NOT NULL,
    [EditionId]             UNIQUEIDENTIFIER    NOT NULL,
    [CreatedAtUtc]          DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [ModifiedAtUtc]         DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_PersonalSchedule] PRIMARY KEY CLUSTERED ([PersonalScheduleId]),
    CONSTRAINT [FK_PersonalSchedule_User] FOREIGN KEY ([UserId]) REFERENCES [identity].[User]([UserId]),
    CONSTRAINT [FK_PersonalSchedule_Edition] FOREIGN KEY ([EditionId]) REFERENCES [core].[FestivalEdition]([EditionId]),
    CONSTRAINT [UQ_PersonalSchedule_UserEdition] UNIQUE ([UserId], [EditionId])
);

CREATE INDEX [IX_PersonalSchedule_User] ON [attendee].[PersonalSchedule] ([UserId]);
```

---

#### 5.6.2 PersonalScheduleEntry

Individual entries in a personal schedule.

```sql
CREATE TABLE [attendee].[PersonalScheduleEntry]
(
    [EntryId]               UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [PersonalScheduleId]    UNIQUEIDENTIFIER    NOT NULL,
    [EngagementId]          UNIQUEIDENTIFIER    NOT NULL,
    [AddedAtUtc]            DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_PersonalScheduleEntry] PRIMARY KEY CLUSTERED ([EntryId]),
    CONSTRAINT [FK_PersonalScheduleEntry_Schedule] FOREIGN KEY ([PersonalScheduleId]) REFERENCES [attendee].[PersonalSchedule]([PersonalScheduleId]),
    CONSTRAINT [FK_PersonalScheduleEntry_Engagement] FOREIGN KEY ([EngagementId]) REFERENCES [schedule].[Engagement]([EngagementId]),
    CONSTRAINT [UQ_PersonalScheduleEntry] UNIQUE ([PersonalScheduleId], [EngagementId])
);
```

---

### 5.7 Notifications Schema

#### 5.7.1 DeviceToken

Stores FCM device tokens for push notifications.

```sql
CREATE TABLE [notifications].[DeviceToken]
(
    [DeviceTokenId]     UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [UserId]            UNIQUEIDENTIFIER    NOT NULL,
    [Token]             NVARCHAR(500)       NOT NULL,
    [Platform]          NVARCHAR(20)        NOT NULL,
    [IsActive]          BIT                 NOT NULL    DEFAULT 1,
    [LastUsedAtUtc]     DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_DeviceToken] PRIMARY KEY CLUSTERED ([DeviceTokenId]),
    CONSTRAINT [FK_DeviceToken_User] FOREIGN KEY ([UserId]) REFERENCES [identity].[User]([UserId]),
    CONSTRAINT [UQ_DeviceToken_Token] UNIQUE ([Token]),
    CONSTRAINT [CK_DeviceToken_Platform] CHECK ([Platform] IN ('ios', 'android', 'web'))
);

CREATE INDEX [IX_DeviceToken_User] ON [notifications].[DeviceToken] ([UserId]) WHERE [IsActive] = 1;
```

---

#### 5.7.2 NotificationLog

Audit log for sent notifications.

```sql
CREATE TABLE [notifications].[NotificationLog]
(
    [NotificationLogId] UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [UserId]            UNIQUEIDENTIFIER    NOT NULL,
    [NotificationType]  NVARCHAR(50)        NOT NULL,
    [Title]             NVARCHAR(200)       NOT NULL,
    [Body]              NVARCHAR(500)       NOT NULL,
    [Data]              NVARCHAR(MAX)       NULL,
    [Status]            NVARCHAR(20)        NOT NULL,
    [SentAtUtc]         DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [DeliveredAtUtc]    DATETIME2(7)        NULL,
    [FailureReason]     NVARCHAR(500)       NULL,

    CONSTRAINT [PK_NotificationLog] PRIMARY KEY CLUSTERED ([NotificationLogId]),
    CONSTRAINT [CK_NotificationLog_Status] CHECK ([Status] IN ('pending', 'sent', 'delivered', 'failed'))
);

CREATE INDEX [IX_NotificationLog_User] ON [notifications].[NotificationLog] ([UserId], [SentAtUtc]);
```

---

### 5.8 Integrations Schema

#### 5.8.1 ApiKey

Stores API keys for external integrations.

```sql
CREATE TABLE [integrations].[ApiKey]
(
    [ApiKeyId]          UNIQUEIDENTIFIER    NOT NULL    DEFAULT NEWSEQUENTIALID(),
    [FestivalId]        UNIQUEIDENTIFIER    NOT NULL,
    [Name]              NVARCHAR(100)       NOT NULL,
    [KeyHash]           NVARCHAR(500)       NOT NULL,   -- Hashed key
    [KeyPrefix]         NVARCHAR(10)        NOT NULL,   -- First 8 chars for identification
    [ExpiresAtUtc]      DATETIME2(7)        NULL,
    [IsRevoked]         BIT                 NOT NULL    DEFAULT 0,
    [RevokedAtUtc]      DATETIME2(7)        NULL,
    [LastUsedAtUtc]     DATETIME2(7)        NULL,
    [CreatedAtUtc]      DATETIME2(7)        NOT NULL    DEFAULT SYSUTCDATETIME(),
    [CreatedBy]         UNIQUEIDENTIFIER    NOT NULL,

    CONSTRAINT [PK_ApiKey] PRIMARY KEY CLUSTERED ([ApiKeyId]),
    CONSTRAINT [FK_ApiKey_Festival] FOREIGN KEY ([FestivalId]) REFERENCES [core].[Festival]([FestivalId])
);

CREATE INDEX [IX_ApiKey_Festival] ON [integrations].[ApiKey] ([FestivalId]) WHERE [IsRevoked] = 0;
CREATE INDEX [IX_ApiKey_Prefix] ON [integrations].[ApiKey] ([KeyPrefix]);
```

---

### 5.9 Audit Schema

#### 5.9.1 AuditLog

Tracks changes to data for compliance.

```sql
CREATE TABLE [audit].[AuditLog]
(
    [AuditLogId]        BIGINT IDENTITY(1,1)    NOT NULL,
    [TableName]         NVARCHAR(128)           NOT NULL,
    [RecordId]          UNIQUEIDENTIFIER        NOT NULL,
    [Action]            NVARCHAR(10)            NOT NULL,
    [OldValues]         NVARCHAR(MAX)           NULL,
    [NewValues]         NVARCHAR(MAX)           NULL,
    [UserId]            UNIQUEIDENTIFIER        NULL,
    [IpAddress]         NVARCHAR(45)            NULL,
    [UserAgent]         NVARCHAR(500)           NULL,
    [CreatedAtUtc]      DATETIME2(7)            NOT NULL    DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED ([AuditLogId]),
    CONSTRAINT [CK_AuditLog_Action] CHECK ([Action] IN ('INSERT', 'UPDATE', 'DELETE'))
);

CREATE INDEX [IX_AuditLog_Table_Record] ON [audit].[AuditLog] ([TableName], [RecordId]);
CREATE INDEX [IX_AuditLog_User] ON [audit].[AuditLog] ([UserId], [CreatedAtUtc]);
CREATE INDEX [IX_AuditLog_Date] ON [audit].[AuditLog] ([CreatedAtUtc]);
```

---

## 6. Data Retention Policies

| **Data Type** | **Retention Period** | **Action** |
|---|---|---|
| Active user accounts | Indefinite | Until user requests deletion |
| Deleted user data | 30 days | Hard delete after grace period |
| Archived editions (attendee) | 3 months | Hide from attendee view |
| Archived editions (organizer) | Unlimited | Always accessible |
| Audit logs | 2 years | Archive to cold storage |
| Refresh tokens (expired) | 30 days | Purge |
| Notification logs | 90 days | Archive |

---

## 7. Migration Strategy

### 7.1 SSDT Deployment

- Schema changes managed through SQL Server Data Tools
- Publish profiles for each environment
- Pre/post deployment scripts for data migrations

### 7.2 Database Portability

| **Strategy** | **Implementation** |
|---|---|
| Repository abstraction | All access through `IRepository<T>` interfaces |
| Standard SQL | ANSI-compliant queries where possible |
| Minimal vendor features | Avoid SQL Server-specific features |
| Dapper flexibility | Works with any ADO.NET provider |

---

*This document is a living artifact and will be updated as the schema evolves.*
