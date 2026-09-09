-- Table: public.Batches

-- DROP TABLE IF EXISTS public."Batches";

CREATE TABLE IF NOT EXISTS public."Batches"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "ClassId" UUID NOT NULL,
    "SubjectId" UUID NOT NULL,

    "Name" VARCHAR(150) NOT NULL,
    "Code" VARCHAR(50) NOT NULL,
    "StartTime" TIME,
    "EndTime" TIME,

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_Batches"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_Batches_Classes_ClassId"
        FOREIGN KEY ("ClassId")
        REFERENCES public."Classes" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_Batches_Subjects_SubjectId"
        FOREIGN KEY ("SubjectId")
        REFERENCES public."Subjects" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_Batches_ClassId_Code"
        UNIQUE ("ClassId", "Code")
);

CREATE INDEX IF NOT EXISTS "IX_Batches_ClassId"
    ON public."Batches" ("ClassId");

CREATE INDEX IF NOT EXISTS "IX_Batches_SubjectId"
    ON public."Batches" ("SubjectId");
