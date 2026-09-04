-- Table: public.Organizations

-- DROP TABLE IF EXISTS public."Organizations";

CREATE TABLE IF NOT EXISTS public."Organizations"
(
    "Id" uuid NOT NULL,
    "Name" character varying(200) COLLATE pg_catalog."default" NOT NULL,
    "Code" character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "LogoUrl" character varying(500) COLLATE pg_catalog."default",
    "Email" character varying(150) COLLATE pg_catalog."default",
    "Mobile" character varying(20) COLLATE pg_catalog."default",
    "Address" character varying(500) COLLATE pg_catalog."default",
    "City" character varying(100) COLLATE pg_catalog."default",
    "State" character varying(100) COLLATE pg_catalog."default",
    "Pincode" character varying(10) COLLATE pg_catalog."default",
    "AcademicSession" character varying(20) COLLATE pg_catalog."default",
    "IsActive" boolean NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone,
    CONSTRAINT "PK_Organizations" PRIMARY KEY ("Id")
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Organizations"
    OWNER to postgres;
-- Index: IX_Organizations_Code

-- DROP INDEX IF EXISTS public."IX_Organizations_Code";

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Organizations_Code"
    ON public."Organizations" USING btree
    ("Code" COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default
    WHERE "IsDeleted" = false;