-- Table: public.BatchStudents

-- DROP TABLE IF EXISTS public."BatchStudents";

CREATE TABLE IF NOT EXISTS public."BatchStudents"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "BatchId" UUID NOT NULL,
    "StudentId" UUID NOT NULL,

    "IsPrimary" BOOLEAN NOT NULL DEFAULT TRUE,
    "EnrollmentType" VARCHAR(50) NOT NULL DEFAULT 'Regular',

    "JoinedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "LeftAt" TIMESTAMPTZ,

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_BatchStudents"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_BatchStudents_Batches_BatchId"
        FOREIGN KEY ("BatchId")
        REFERENCES public."Batches" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_BatchStudents_Students_StudentId"
        FOREIGN KEY ("StudentId")
        REFERENCES public."Students" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_BatchStudents_BatchId_StudentId"
        UNIQUE ("BatchId", "StudentId")
);

CREATE INDEX IF NOT EXISTS "IX_BatchStudents_BatchId"
    ON public."BatchStudents" ("BatchId");

CREATE INDEX IF NOT EXISTS "IX_BatchStudents_StudentId"
    ON public."BatchStudents" ("StudentId");
