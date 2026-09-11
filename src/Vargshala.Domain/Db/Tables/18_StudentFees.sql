-- Table: public.StudentFees

-- DROP TABLE IF EXISTS public."StudentFees";

CREATE TABLE IF NOT EXISTS public."StudentFees"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL,
    "StudentId" UUID NOT NULL,
    "FeeStructureId" UUID NOT NULL,

    "OriginalAmount" NUMERIC(12, 2) NOT NULL,
    "DiscountAmount" NUMERIC(12, 2) NOT NULL DEFAULT 0.00,
    "FinalAmount" NUMERIC(12, 2) NOT NULL,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Pending',
    "AssignedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_StudentFees"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_StudentFees_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_StudentFees_Students_StudentId"
        FOREIGN KEY ("StudentId")
        REFERENCES public."Students" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_StudentFees_FeeStructures_FeeStructureId"
        FOREIGN KEY ("FeeStructureId")
        REFERENCES public."FeeStructures" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "CK_StudentFees_Amounts"
        CHECK ("FinalAmount" = ("OriginalAmount" - "DiscountAmount")),

    CONSTRAINT "CK_StudentFees_Status"
        CHECK ("Status" IN ('Pending', 'PartiallyPaid', 'Paid', 'Overdue', 'Cancelled'))
);

CREATE INDEX IF NOT EXISTS "IX_StudentFees_OrganizationId"
    ON public."StudentFees" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_StudentFees_StudentId"
    ON public."StudentFees" ("StudentId");

CREATE INDEX IF NOT EXISTS "IX_StudentFees_FeeStructureId"
    ON public."StudentFees" ("FeeStructureId");

CREATE INDEX IF NOT EXISTS "IX_StudentFees_Org_Status"
    ON public."StudentFees" ("OrganizationId", "Status");
