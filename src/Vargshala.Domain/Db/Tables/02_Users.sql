-- Table: public.Users

-- DROP TABLE IF EXISTS public."Users";

CREATE TABLE IF NOT EXISTS public."Users"
(
    "Id" uuid NOT NULL,
    "OrganizationId" uuid,
    "FirstName" character varying(100) COLLATE pg_catalog."default" NOT NULL,
    "LastName" character varying(100) COLLATE pg_catalog."default" NOT NULL,
    "Email" character varying(150) COLLATE pg_catalog."default",
    "Mobile" character varying(20) COLLATE pg_catalog."default",
    "PasswordHash" character varying(500) COLLATE pg_catalog."default" NOT NULL,
    "Role" integer NOT NULL,
    "EmailVerified" boolean NOT NULL,
    "MobileVerified" boolean NOT NULL,
    "LastLoginAt" timestamp with time zone,
    "RefreshToken" character varying(500) COLLATE pg_catalog."default",
    "RefreshTokenExpiryTime" timestamp with time zone,
    "IsActive" boolean NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "ProfilePictureUrl" character varying(max),
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Users_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE RESTRICT
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Users"
    OWNER to postgres;
-- Index: IX_Users_Email_OrganizationId

-- DROP INDEX IF EXISTS public."IX_Users_Email_OrganizationId";

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email_OrganizationId"
    ON public."Users" USING btree
    ("Email" COLLATE pg_catalog."default" ASC NULLS LAST, "OrganizationId" ASC NULLS LAST)
    TABLESPACE pg_default
    WHERE "Email" IS NOT NULL AND "IsDeleted" = false;
-- Index: IX_Users_OrganizationId

-- DROP INDEX IF EXISTS public."IX_Users_OrganizationId";

CREATE INDEX IF NOT EXISTS "IX_Users_OrganizationId"
    ON public."Users" USING btree
    ("OrganizationId" ASC NULLS LAST)
    TABLESPACE pg_default;