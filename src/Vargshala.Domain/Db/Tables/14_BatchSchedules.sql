-- Table: public.BatchSchedules

CREATE TABLE IF NOT EXISTS public."BatchSchedules"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "BatchId" UUID NOT NULL,
    "DayOfWeek" INTEGER NOT NULL,
    "StartTime" TIME NOT NULL,
    "EndTime" TIME NOT NULL,
    "RoomOrLocation" VARCHAR(150),

    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_BatchSchedules"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_BatchSchedules_Batches_BatchId"
        FOREIGN KEY ("BatchId")
        REFERENCES public."Batches" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS "IX_BatchSchedules_BatchId"
    ON public."BatchSchedules" ("BatchId");

CREATE INDEX IF NOT EXISTS "IX_BatchSchedules_DayOfWeek"
    ON public."BatchSchedules" ("DayOfWeek");
