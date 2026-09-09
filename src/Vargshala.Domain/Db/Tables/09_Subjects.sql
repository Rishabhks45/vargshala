-- Table: public.Subjects

-- DROP TABLE IF EXISTS public."Subjects";

CREATE TABLE IF NOT EXISTS public."Subjects"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL,

    "Name" VARCHAR(150) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "Description" TEXT,

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_Subjects"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_Subjects_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_Subjects_OrganizationId_Code"
        UNIQUE ("OrganizationId", "Code")
);

CREATE INDEX IF NOT EXISTS "IX_Subjects_OrganizationId"
    ON public."Subjects" ("OrganizationId");
