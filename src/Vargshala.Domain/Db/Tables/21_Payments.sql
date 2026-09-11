-- Table: public.Payments

-- DROP TABLE IF EXISTS public."Payments";

CREATE TABLE IF NOT EXISTS public."Payments"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL,
    "StudentId" UUID NOT NULL,

    "Amount" NUMERIC(12, 2) NOT NULL,
    "PaymentDate" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "PaymentMethod" VARCHAR(50) NOT NULL,
    "TransactionReference" VARCHAR(100),
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Completed',
    "Remarks" TEXT,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_Payments"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_Payments_Organizations_OrganizationId"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_Payments_Students_StudentId"
        FOREIGN KEY ("StudentId")
        REFERENCES public."Students" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_Payments_Users_CreatedBy"
        FOREIGN KEY ("CreatedBy")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "CK_Payments_Amount"
        CHECK ("Amount" > 0),

    CONSTRAINT "CK_Payments_Status"
        CHECK ("Status" IN ('Completed', 'Pending', 'Failed', 'Refunded'))
);

CREATE INDEX IF NOT EXISTS "IX_Payments_OrganizationId"
    ON public."Payments" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_Payments_StudentId"
    ON public."Payments" ("StudentId");

CREATE INDEX IF NOT EXISTS "IX_Payments_PaymentDate"
    ON public."Payments" ("PaymentDate");

CREATE INDEX IF NOT EXISTS "IX_Payments_TransactionReference"
    ON public."Payments" ("TransactionReference");

CREATE INDEX IF NOT EXISTS "IX_Payments_Org_Date"
    ON public."Payments" ("OrganizationId", "PaymentDate");
