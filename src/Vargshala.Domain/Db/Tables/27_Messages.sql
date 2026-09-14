-- Table: public.Messages
-- MessageType Enum:
--   'Text'         : Standard plain text message
--   'Image'        : Picture or photo
--   'File'         : Document, PDF, or attachment
--   'System'       : Automated event (MemberAdded, GroupCreated, etc.)
--   'Announcement' : Official broadcast announcement

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
