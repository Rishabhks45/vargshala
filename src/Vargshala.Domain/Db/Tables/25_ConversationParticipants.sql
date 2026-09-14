-- Table: public.ConversationParticipants

CREATE TABLE IF NOT EXISTS public."ConversationParticipants"
(
    "Id"                     UUID                     NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId"         UUID                     NOT NULL,
    "ConversationId"         UUID                     NOT NULL,
    "UserId"                 UUID                     NOT NULL,

    -- Participant Role in this conversation ('Admin', 'Member')
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
