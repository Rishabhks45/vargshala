-- Table: public.PaymentAllocations

-- DROP TABLE IF EXISTS public."PaymentAllocations";

CREATE TABLE IF NOT EXISTS public."PaymentAllocations"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "PaymentId" UUID NOT NULL,
    "FeeInstallmentId" UUID NOT NULL,

    "AllocatedAmount" NUMERIC(12, 2) NOT NULL,

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_PaymentAllocations"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_PaymentAllocations_Payments_PaymentId"
        FOREIGN KEY ("PaymentId")
        REFERENCES public."Payments" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_PaymentAllocations_FeeInstallments_FeeInstallmentId"
        FOREIGN KEY ("FeeInstallmentId")
        REFERENCES public."FeeInstallments" ("Id")
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT "UQ_PaymentAllocations_PaymentId_FeeInstallmentId"
        UNIQUE ("PaymentId", "FeeInstallmentId"),

    CONSTRAINT "CK_PaymentAllocations_Amount"
        CHECK ("AllocatedAmount" > 0)
);

CREATE INDEX IF NOT EXISTS "IX_PaymentAllocations_PaymentId"
    ON public."PaymentAllocations" ("PaymentId");

CREATE INDEX IF NOT EXISTS "IX_PaymentAllocations_FeeInstallmentId"
    ON public."PaymentAllocations" ("FeeInstallmentId");
