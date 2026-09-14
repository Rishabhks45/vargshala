-- Table: public.MessageReads

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
