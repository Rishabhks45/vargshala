-- Table: public.AnnouncementReplyPermissions
-- Role Enum:
--   1001 = SuperAdmin
--   1002 = BackOffice Staff
--   1    = OrganizationAdmin (Institute Admin)
--   2    = Teacher
--   3    = Student
--   4    = BranchAdmin (Head of Branch)

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
