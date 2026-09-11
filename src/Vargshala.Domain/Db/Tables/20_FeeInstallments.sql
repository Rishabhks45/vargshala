-- Table: public.FeeInstallments

-- DROP TABLE IF EXISTS public."FeeInstallments";

CREATE TABLE IF NOT EXISTS public."FeeInstallments"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "StudentFeeId" UUID NOT NULL,

    "InstallmentNumber" INT NOT NULL,
    "Amount" NUMERIC(12, 2) NOT NULL,
    "DueDate" DATE NOT NULL,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Pending',

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    CONSTRAINT "PK_FeeInstallments"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_FeeInstallments_StudentFees_StudentFeeId"
        FOREIGN KEY ("StudentFeeId")
        REFERENCES public."StudentFees" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "UQ_FeeInstallments_StudentFeeId_InstallmentNumber"
        UNIQUE ("StudentFeeId", "InstallmentNumber"),

    CONSTRAINT "CK_FeeInstallments_Amount"
        CHECK ("Amount" > 0),

    CONSTRAINT "CK_FeeInstallments_Status"
        CHECK ("Status" IN ('Pending', 'PartiallyPaid', 'Paid', 'Overdue', 'Cancelled'))
);

CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_StudentFeeId"
    ON public."FeeInstallments" ("StudentFeeId");

CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_DueDate"
    ON public."FeeInstallments" ("DueDate");

CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_Status"
    ON public."FeeInstallments" ("Status");

CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_StudentFee_DueDate"
    ON public."FeeInstallments" ("StudentFeeId", "DueDate");
