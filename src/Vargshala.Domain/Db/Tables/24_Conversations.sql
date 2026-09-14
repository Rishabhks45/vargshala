-- Table: public.Conversations
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
