-- Table: public.BatchTeachers

-- DROP TABLE IF EXISTS public."BatchTeachers";

CREATE TABLE IF NOT EXISTS public."BatchTeachers"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "BatchId" UUID NOT NULL,
    "TeacherId" UUID NOT NULL,

    "AssignedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "RemovedAt" TIMESTAMPTZ,

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_BatchTeachers"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_BatchTeachers_Batches_BatchId"
        FOREIGN KEY ("BatchId")
        REFERENCES public."Batches" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_BatchTeachers_Teachers_TeacherId"
        FOREIGN KEY ("TeacherId")
        REFERENCES public."Teachers" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_BatchTeachers_BatchId_TeacherId"
        UNIQUE ("BatchId", "TeacherId")
);

CREATE INDEX IF NOT EXISTS "IX_BatchTeachers_BatchId"
    ON public."BatchTeachers" ("BatchId");

CREATE INDEX IF NOT EXISTS "IX_BatchTeachers_TeacherId"
    ON public."BatchTeachers" ("TeacherId");
