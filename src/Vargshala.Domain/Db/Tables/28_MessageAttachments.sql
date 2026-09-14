-- Table: public.MessageAttachments

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
