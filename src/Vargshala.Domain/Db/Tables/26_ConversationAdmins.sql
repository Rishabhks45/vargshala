-- Table: public.ConversationAdmins

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
