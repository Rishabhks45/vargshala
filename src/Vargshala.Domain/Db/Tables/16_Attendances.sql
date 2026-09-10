CREATE TABLE IF NOT EXISTS public."Attendances"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),

    "ClassSessionId" UUID NOT NULL,
    "StudentId" UUID NOT NULL,

    "Status" VARCHAR(20) NOT NULL DEFAULT 'Present',

    "MarkedAt" TIMESTAMPTZ,
    "Remarks" TEXT,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_Attendances"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_Attendances_ClassSessions_ClassSessionId"
        FOREIGN KEY ("ClassSessionId")
        REFERENCES public."ClassSessions" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "FK_Attendances_Students_StudentId"
        FOREIGN KEY ("StudentId")
        REFERENCES public."Students" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_Attendances_ClassSessionId_StudentId"
        UNIQUE ("ClassSessionId", "StudentId"),

    CONSTRAINT "CK_Attendances_Status"
        CHECK ("Status" IN ('Present', 'Absent', 'Late', 'Excused'))
);

CREATE INDEX IF NOT EXISTS "IX_Attendances_ClassSessionId"
    ON public."Attendances" ("ClassSessionId");

CREATE INDEX IF NOT EXISTS "IX_Attendances_StudentId"
    ON public."Attendances" ("StudentId");
