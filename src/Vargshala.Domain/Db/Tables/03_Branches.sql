-- Table: public.Branches

-- DROP TABLE IF EXISTS public."Branches";

CREATE TABLE IF NOT EXISTS public."Branches"
(
    "Id" uuid NOT NULL,
    "OrganizationId" uuid NOT NULL,
    "Name" character varying(200) COLLATE pg_catalog."default" NOT NULL,
    "Code" character varying(50) COLLATE pg_catalog."default" NOT NULL,
    "LogoUrl" text COLLATE pg_catalog."default",
    "Email" character varying(150) COLLATE pg_catalog."default",
    "Mobile" character varying(20) COLLATE pg_catalog."default",
    "AlternateMobile" character varying(20) COLLATE pg_catalog."default",
    "Address" character varying(500) COLLATE pg_catalog."default",
    "City" character varying(100) COLLATE pg_catalog."default",
    "State" character varying(100) COLLATE pg_catalog."default",
    "Pincode" character varying(10) COLLATE pg_catalog."default",
    "Country" character varying(100) COLLATE pg_catalog."default",
    "IsMainBranch" boolean NOT NULL DEFAULT false,
    "UseBranchName" boolean NOT NULL DEFAULT true,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone,
    CONSTRAINT "PK_Branches" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Branches_OrganizationId_Code" UNIQUE ("OrganizationId", "Code"),
    CONSTRAINT "FK_Branches_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE RESTRICT
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Branches"
    OWNER to postgres;

-- Indexes
CREATE INDEX IF NOT EXISTS "IX_Branches_OrganizationId"
    ON public."Branches" USING btree
    ("OrganizationId" ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE INDEX IF NOT EXISTS "IX_Branches_City"
    ON public."Branches" USING btree
    ("City" COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
