-- ============================================================================
-- VARGSHALA - MESSAGING MODULE DDL SCRIPT (POSTGRESQL)
-- Clean Architecture & Multi-Tenant Isolated (OrganizationId on all entities)
-- 7 Tables:
--   1. Conversations
--   2. ConversationParticipants
--   3. ConversationAdmins
--   4. Messages
--   5. MessageAttachments
--   6. MessageReads
--   7. AnnouncementReplyPermissions
-- ============================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

START TRANSACTION;

-- ============================================================================
-- 1. Conversations
-- ============================================================================
-- Type Enum:
--   'Direct'             : 1-to-1 chat between two users
--   'Group'              : Custom group chat created by admin/teacher
--   'BatchGroup'         : Automatically synced group for a Batch
--   'BranchGroup'        : Branch-wide broadcast / discussion group
--   'OrganizationGroup'  : Institute-wide announcement group
--
-- WhoCanReply Enum:
--   'Everyone'           : Standard group / chat (all members can reply)
--   'AdminsOnly'         : Broadcast channel / Announcement (only admins post)
--   'TeachersAndAdmins'  : Classroom channel (teachers & staff post, students read)
-- ============================================================================

CREATE TABLE IF NOT EXISTS public."Conversations"
(
    "Id"                     UUID                     NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId"         UUID                     NOT NULL,
    "BranchId"               UUID                     NULL,
    "BatchId"                UUID                     NULL,
    "CreatedBy"              UUID                     NOT NULL,

    "Type"                   VARCHAR(30)              NOT NULL,
    "Name"                   VARCHAR(200)             NULL,
    "GroupPhotoUrl"          TEXT                     NULL,
    "Description"            TEXT                     NULL,

    -- Canonical User IDs for 1-to-1 Direct Chat Unique Lookup (User1Id < User2Id)
    "DirectUser1Id"          UUID                     NULL,
    "DirectUser2Id"          UUID                     NULL,

    "IsAnnouncement"         BOOLEAN                  NOT NULL DEFAULT FALSE,
    "AllowReplies"           BOOLEAN                  NOT NULL DEFAULT TRUE,
    "WhoCanReply"            VARCHAR(30)              NOT NULL DEFAULT 'Everyone',
    "IsActive"               BOOLEAN                  NOT NULL DEFAULT TRUE,

    -- Denormalized Last Message Cache for Sub-Millisecond Inbox Sorting
    "LastMessageId"          UUID                     NULL,
    "LastMessageAt"          TIMESTAMP WITH TIME ZONE NULL,
    "LastMessageText"        TEXT                     NULL,
    "LastMessageSenderId"    UUID                     NULL,

    -- Audit & Soft Delete
    "CreatedAt"              TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"              UUID                     NULL,
    "UpdatedAt"              TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"              BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"              UUID                     NULL,
    "DeletedAt"              TIMESTAMP WITH TIME ZONE NULL,

    CONSTRAINT "PK_Conversations"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_Conversations_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_Conversations_Branches_BranchId"
        FOREIGN KEY ("BranchId")
        REFERENCES public."Branches" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "FK_Conversations_Batches_BatchId"
        FOREIGN KEY ("BatchId")
        REFERENCES public."Batches" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "FK_Conversations_Users_CreatedBy"
        FOREIGN KEY ("CreatedBy")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_Conversations_OrganizationId"
    ON public."Conversations" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_Conversations_Org_LastMessageAt"
    ON public."Conversations" ("OrganizationId", "LastMessageAt" DESC)
    WHERE "IsDeleted" = FALSE;

CREATE INDEX IF NOT EXISTS "IX_Conversations_BranchId"
    ON public."Conversations" ("BranchId");

CREATE INDEX IF NOT EXISTS "IX_Conversations_BatchId"
    ON public."Conversations" ("BatchId");

CREATE INDEX IF NOT EXISTS "IX_Conversations_CreatedBy"
    ON public."Conversations" ("CreatedBy");

-- Unique 1-to-1 Direct Conversation Per Organization
CREATE UNIQUE INDEX IF NOT EXISTS "UQ_Conversations_DirectUsers"
    ON public."Conversations" ("OrganizationId", "DirectUser1Id", "DirectUser2Id")
    WHERE "Type" = 'Direct' AND "IsDeleted" = FALSE;


-- ============================================================================
-- 2. ConversationParticipants
-- ============================================================================

CREATE TABLE IF NOT EXISTS public."ConversationParticipants"
(
    "Id"                     UUID                     NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId"         UUID                     NOT NULL,
    "ConversationId"         UUID                     NOT NULL,
    "UserId"                 UUID                     NOT NULL,

    -- Participant Role in this conversation
    "Role"                   VARCHAR(20)              NOT NULL DEFAULT 'Member',
    "IsAdmin"                BOOLEAN                  NOT NULL DEFAULT FALSE,

    "JoinedAt"               TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LeftAt"                 TIMESTAMP WITH TIME ZONE NULL,

    -- Read Watermark & Unread Tracking
    "LastReadAt"             TIMESTAMP WITH TIME ZONE NULL,
    "LastReadMessageId"      UUID                     NULL,

    -- User Preferences
    "IsMuted"                BOOLEAN                  NOT NULL DEFAULT FALSE,
    "IsPinned"               BOOLEAN                  NOT NULL DEFAULT FALSE,
    "IsActive"               BOOLEAN                  NOT NULL DEFAULT TRUE,

    -- Audit & Soft Delete
    "CreatedBy"              UUID                     NULL,
    "CreatedAt"              TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"              UUID                     NULL,
    "UpdatedAt"              TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"              BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"              UUID                     NULL,
    "DeletedAt"              TIMESTAMP WITH TIME ZONE NULL,

    CONSTRAINT "PK_ConversationParticipants"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_ConversationParticipants_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_ConversationParticipants_Conversations_ConversationId"
        FOREIGN KEY ("ConversationId")
        REFERENCES public."Conversations" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_ConversationParticipants_Users_UserId"
        FOREIGN KEY ("UserId")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_ConversationParticipants_OrganizationId"
    ON public."ConversationParticipants" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_ConversationParticipants_ConversationId"
    ON public."ConversationParticipants" ("ConversationId");

CREATE INDEX IF NOT EXISTS "IX_ConversationParticipants_UserId"
    ON public."ConversationParticipants" ("UserId");

CREATE INDEX IF NOT EXISTS "IX_ConversationParticipants_User_Active"
    ON public."ConversationParticipants" ("UserId", "IsActive")
    WHERE "IsDeleted" = FALSE;

-- Partial unique constraint so re-joining a group does not crash on soft-deleted history
CREATE UNIQUE INDEX IF NOT EXISTS "UQ_ConversationParticipants_Active"
    ON public."ConversationParticipants" ("ConversationId", "UserId")
    WHERE "IsDeleted" = FALSE;


-- ============================================================================
-- 3. ConversationAdmins
-- ============================================================================

CREATE TABLE IF NOT EXISTS public."ConversationAdmins"
(
    "Id"                     UUID                     NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId"         UUID                     NOT NULL,
    "ConversationId"         UUID                     NOT NULL,
    "UserId"                 UUID                     NOT NULL,

    "AssignedBy"             UUID                     NULL,
    "AssignedAt"             TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "RemovedAt"              TIMESTAMP WITH TIME ZONE NULL,

    "IsActive"               BOOLEAN                  NOT NULL DEFAULT TRUE,

    -- Audit & Soft Delete
    "CreatedBy"              UUID                     NULL,
    "CreatedAt"              TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"              UUID                     NULL,
    "UpdatedAt"              TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"              BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"              UUID                     NULL,
    "DeletedAt"              TIMESTAMP WITH TIME ZONE NULL,

    CONSTRAINT "PK_ConversationAdmins"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_ConversationAdmins_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_ConversationAdmins_Conversations_ConversationId"
        FOREIGN KEY ("ConversationId")
        REFERENCES public."Conversations" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_ConversationAdmins_Users_UserId"
        FOREIGN KEY ("UserId")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_ConversationAdmins_Users_AssignedBy"
        FOREIGN KEY ("AssignedBy")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_ConversationAdmins_OrganizationId"
    ON public."ConversationAdmins" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_ConversationAdmins_ConversationId"
    ON public."ConversationAdmins" ("ConversationId");

CREATE INDEX IF NOT EXISTS "IX_ConversationAdmins_UserId"
    ON public."ConversationAdmins" ("UserId");

CREATE UNIQUE INDEX IF NOT EXISTS "UQ_ConversationAdmins_Active"
    ON public."ConversationAdmins" ("ConversationId", "UserId")
    WHERE "IsDeleted" = FALSE;


-- ============================================================================
-- 4. Messages
-- ============================================================================

CREATE TABLE IF NOT EXISTS public."Messages"
(
    "Id"                     UUID                     NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId"         UUID                     NOT NULL,
    "ConversationId"         UUID                     NOT NULL,
    "SenderId"               UUID                     NOT NULL,
    "ReplyToMessageId"       UUID                     NULL,

    "MessageType"            VARCHAR(30)              NOT NULL DEFAULT 'Text',
    "MessageText"            TEXT                     NULL,

    -- Important Notice / Pin Feature
    "IsPinned"               BOOLEAN                  NOT NULL DEFAULT FALSE,
    "PinnedAt"               TIMESTAMP WITH TIME ZONE NULL,
    "PinnedBy"               UUID                     NULL,

    -- System message event information
    "SystemEventType"        VARCHAR(50)              NULL,
    "SystemEventUserId"      UUID                     NULL,
    "TargetUserId"           UUID                     NULL,

    "SentAt"                 TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "EditedAt"               TIMESTAMP WITH TIME ZONE NULL,

    -- Soft Delete ("Delete for everyone")
    "IsDeleted"              BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"              UUID                     NULL,
    "DeletedAt"              TIMESTAMP WITH TIME ZONE NULL,

    CONSTRAINT "PK_Messages"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_Messages_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_Messages_Conversations_ConversationId"
        FOREIGN KEY ("ConversationId")
        REFERENCES public."Conversations" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_Messages_Users_SenderId"
        FOREIGN KEY ("SenderId")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_Messages_Messages_ReplyToMessageId"
        FOREIGN KEY ("ReplyToMessageId")
        REFERENCES public."Messages" ("Id")
        ON UPDATE NO ACTION
        ON DELETE SET NULL,

    CONSTRAINT "FK_Messages_Users_PinnedBy"
        FOREIGN KEY ("PinnedBy")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "FK_Messages_Users_SystemEventUserId"
        FOREIGN KEY ("SystemEventUserId")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "FK_Messages_Users_TargetUserId"
        FOREIGN KEY ("TargetUserId")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_Messages_ConversationId_SentAt"
    ON public."Messages" ("ConversationId", "SentAt" DESC)
    WHERE "IsDeleted" = FALSE;

CREATE INDEX IF NOT EXISTS "IX_Messages_OrganizationId"
    ON public."Messages" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_Messages_SenderId"
    ON public."Messages" ("SenderId");

CREATE INDEX IF NOT EXISTS "IX_Messages_ReplyToMessageId"
    ON public."Messages" ("ReplyToMessageId");

CREATE INDEX IF NOT EXISTS "IX_Messages_SystemEventUserId"
    ON public."Messages" ("SystemEventUserId");

CREATE INDEX IF NOT EXISTS "IX_Messages_TargetUserId"
    ON public."Messages" ("TargetUserId");


-- ============================================================================
-- 5. MessageAttachments
-- ============================================================================

CREATE TABLE IF NOT EXISTS public."MessageAttachments"
(
    "Id"                     UUID                     NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId"         UUID                     NOT NULL,
    "MessageId"              UUID                     NOT NULL,

    "FileName"               VARCHAR(255)             NOT NULL,
    "ContentType"            VARCHAR(150)             NULL,
    "FileSize"               BIGINT                   NULL,
    "FileUrl"                TEXT                     NOT NULL,
    "ThumbnailUrl"           TEXT                     NULL,
    "StorageKey"             VARCHAR(500)             NULL,

    "CreatedBy"              UUID                     NULL,
    "CreatedAt"              TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,

    "IsDeleted"              BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"              UUID                     NULL,
    "DeletedAt"              TIMESTAMP WITH TIME ZONE NULL,

    CONSTRAINT "PK_MessageAttachments"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_MessageAttachments_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_MessageAttachments_Messages_MessageId"
        FOREIGN KEY ("MessageId")
        REFERENCES public."Messages" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_MessageAttachments_Users_CreatedBy"
        FOREIGN KEY ("CreatedBy")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_MessageAttachments_OrganizationId"
    ON public."MessageAttachments" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_MessageAttachments_MessageId"
    ON public."MessageAttachments" ("MessageId");


-- ============================================================================
-- 6. MessageReads
-- ============================================================================

CREATE TABLE IF NOT EXISTS public."MessageReads"
(
    "Id"                     UUID                     NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId"         UUID                     NOT NULL,
    "MessageId"              UUID                     NOT NULL,
    "UserId"                 UUID                     NOT NULL,

    "ReadAt"                 TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "PK_MessageReads"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_MessageReads_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_MessageReads_Messages_MessageId"
        FOREIGN KEY ("MessageId")
        REFERENCES public."Messages" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_MessageReads_Users_UserId"
        FOREIGN KEY ("UserId")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_MessageReads_MessageId_UserId"
        UNIQUE ("MessageId", "UserId")
);

CREATE INDEX IF NOT EXISTS "IX_MessageReads_OrganizationId"
    ON public."MessageReads" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_MessageReads_MessageId"
    ON public."MessageReads" ("MessageId");

CREATE INDEX IF NOT EXISTS "IX_MessageReads_UserId"
    ON public."MessageReads" ("UserId");


-- ============================================================================
-- 7. AnnouncementReplyPermissions
-- ============================================================================
-- Role Enum:
--   1001 = SuperAdmin
--   1002 = BackOffice Staff
--   1    = OrganizationAdmin (Institute Admin)
--   2    = Teacher
--   3    = Student
--   4    = BranchAdmin (Head of Branch)
-- ============================================================================

CREATE TABLE IF NOT EXISTS public."AnnouncementReplyPermissions"
(
    "Id"                     UUID                     NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId"         UUID                     NOT NULL,
    "ConversationId"         UUID                     NOT NULL,

    "Role"                   INTEGER                  NOT NULL,
    "CanReply"               BOOLEAN                  NOT NULL DEFAULT FALSE,

    "CreatedBy"              UUID                     NULL,
    "CreatedAt"              TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"              UUID                     NULL,
    "UpdatedAt"              TIMESTAMP WITH TIME ZONE NULL,

    "IsDeleted"              BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"              UUID                     NULL,
    "DeletedAt"              TIMESTAMP WITH TIME ZONE NULL,

    CONSTRAINT "PK_AnnouncementReplyPermissions"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_AnnouncementReplyPermissions_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_AnnouncementReplyPermissions_Conversations_ConversationId"
        FOREIGN KEY ("ConversationId")
        REFERENCES public."Conversations" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_AnnouncementReplyPermissions_Users_CreatedBy"
        FOREIGN KEY ("CreatedBy")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "FK_AnnouncementReplyPermissions_Users_UpdatedBy"
        FOREIGN KEY ("UpdatedBy")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "UQ_AnnouncementReplyPermissions_ConversationId_Role"
        UNIQUE ("ConversationId", "Role")
);

CREATE INDEX IF NOT EXISTS "IX_AnnouncementReplyPermissions_OrganizationId"
    ON public."AnnouncementReplyPermissions" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_AnnouncementReplyPermissions_ConversationId"
    ON public."AnnouncementReplyPermissions" ("ConversationId");

COMMIT;
