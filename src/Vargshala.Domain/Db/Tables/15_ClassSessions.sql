-- Table: public.ClassSessions

CREATE TABLE IF NOT EXISTS public."ClassSessions"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "BatchId" UUID NOT NULL,
    "BatchScheduleId" UUID,
    "TeacherId" UUID,

    "SessionDate" DATE NOT NULL,
    "StartTime" TIME NOT NULL,
    "EndTime" TIME NOT NULL,

    "Topic" VARCHAR(250),
    "Notes" TEXT,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Scheduled',

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_ClassSessions"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_ClassSessions_Batches_BatchId"
        FOREIGN KEY ("BatchId")
        REFERENCES public."Batches" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_ClassSessions_BatchSchedules_BatchScheduleId"
        FOREIGN KEY ("BatchScheduleId")
        REFERENCES public."BatchSchedules" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "FK_ClassSessions_Teachers_TeacherId"
        FOREIGN KEY ("TeacherId")
        REFERENCES public."Teachers" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_ClassSessions_BatchId"
    ON public."ClassSessions" ("BatchId");

CREATE INDEX IF NOT EXISTS "IX_ClassSessions_SessionDate"
    ON public."ClassSessions" ("SessionDate");

CREATE INDEX IF NOT EXISTS "IX_ClassSessions_TeacherId"
    ON public."ClassSessions" ("TeacherId");

CREATE INDEX IF NOT EXISTS "IX_ClassSessions_BatchScheduleId"
    ON public."ClassSessions" ("BatchScheduleId");

CREATE INDEX IF NOT EXISTS "IX_ClassSessions_BatchId_SessionDate"
    ON public."ClassSessions" ("BatchId", "SessionDate");
