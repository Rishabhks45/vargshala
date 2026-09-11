-- ============================================================================
-- VARGSHALA FEE MANAGEMENT MODULE - ENHANCEMENTS MIGRATION SCRIPT
-- Migration: Add ClassId, PaidAmount, OrganizationId, ReceiptNumber, BranchId
-- Target Database: PostgreSQL
-- ============================================================================

START TRANSACTION;

-- ----------------------------------------------------------------------------
-- 1. FeeStructures: Add ClassId for course/class mapping
-- ----------------------------------------------------------------------------
ALTER TABLE public."FeeStructures" 
    ADD COLUMN IF NOT EXISTS "ClassId" UUID NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_FeeStructures_Classes_ClassId'
    ) THEN
        ALTER TABLE public."FeeStructures"
            ADD CONSTRAINT "FK_FeeStructures_Classes_ClassId" 
            FOREIGN KEY ("ClassId") 
            REFERENCES public."Classes"("Id") 
            ON UPDATE CASCADE 
            ON DELETE SET NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS "IX_FeeStructures_ClassId" 
    ON public."FeeStructures" ("ClassId");


-- ----------------------------------------------------------------------------
-- 2. StudentFees: Add PaidAmount for instant KPI / summary calculation
-- ----------------------------------------------------------------------------
ALTER TABLE public."StudentFees" 
    ADD COLUMN IF NOT EXISTS "PaidAmount" NUMERIC(12, 2) NOT NULL DEFAULT 0.00;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'CK_StudentFees_PaidAmount'
    ) THEN
        ALTER TABLE public."StudentFees"
            ADD CONSTRAINT "CK_StudentFees_PaidAmount" 
            CHECK ("PaidAmount" >= 0 AND "PaidAmount" <= "FinalAmount");
    END IF;
END $$;


-- ----------------------------------------------------------------------------
-- 3. FeeDiscounts: Add OrganizationId for multi-tenancy global query filter
-- ----------------------------------------------------------------------------
ALTER TABLE public."FeeDiscounts" 
    ADD COLUMN IF NOT EXISTS "OrganizationId" UUID NULL;

-- Backfill OrganizationId from parent StudentFees if existing rows exist
UPDATE public."FeeDiscounts" fd
SET "OrganizationId" = sf."OrganizationId"
FROM public."StudentFees" sf
WHERE fd."StudentFeeId" = sf."Id" AND fd."OrganizationId" IS NULL;

-- Set NOT NULL once backfilled
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM public."FeeDiscounts" WHERE "OrganizationId" IS NULL) THEN
        ALTER TABLE public."FeeDiscounts" ALTER COLUMN "OrganizationId" SET NOT NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_FeeDiscounts_Organizations_OrganizationId'
    ) THEN
        ALTER TABLE public."FeeDiscounts"
            ADD CONSTRAINT "FK_FeeDiscounts_Organizations_OrganizationId" 
            FOREIGN KEY ("OrganizationId") 
            REFERENCES public."Organizations"("Id") 
            ON UPDATE CASCADE 
            ON DELETE CASCADE;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS "IX_FeeDiscounts_OrganizationId" 
    ON public."FeeDiscounts" ("OrganizationId");


-- ----------------------------------------------------------------------------
-- 4. FeeInstallments: Add OrganizationId and PaidAmount
-- ----------------------------------------------------------------------------
ALTER TABLE public."FeeInstallments" 
    ADD COLUMN IF NOT EXISTS "OrganizationId" UUID NULL,
    ADD COLUMN IF NOT EXISTS "PaidAmount" NUMERIC(12, 2) NOT NULL DEFAULT 0.00;

-- Backfill OrganizationId from parent StudentFees
UPDATE public."FeeInstallments" fi
SET "OrganizationId" = sf."OrganizationId"
FROM public."StudentFees" sf
WHERE fi."StudentFeeId" = sf."Id" AND fi."OrganizationId" IS NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM public."FeeInstallments" WHERE "OrganizationId" IS NULL) THEN
        ALTER TABLE public."FeeInstallments" ALTER COLUMN "OrganizationId" SET NOT NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_FeeInstallments_Organizations_OrganizationId'
    ) THEN
        ALTER TABLE public."FeeInstallments"
            ADD CONSTRAINT "FK_FeeInstallments_Organizations_OrganizationId" 
            FOREIGN KEY ("OrganizationId") 
            REFERENCES public."Organizations"("Id") 
            ON UPDATE CASCADE 
            ON DELETE CASCADE;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'CK_FeeInstallments_PaidAmount'
    ) THEN
        ALTER TABLE public."FeeInstallments"
            ADD CONSTRAINT "CK_FeeInstallments_PaidAmount" 
            CHECK ("PaidAmount" >= 0 AND "PaidAmount" <= "Amount");
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_OrganizationId" 
    ON public."FeeInstallments" ("OrganizationId");


-- ----------------------------------------------------------------------------
-- 5. Payments: Add ReceiptNumber and BranchId
-- ----------------------------------------------------------------------------
ALTER TABLE public."Payments" 
    ADD COLUMN IF NOT EXISTS "ReceiptNumber" VARCHAR(50) NULL,
    ADD COLUMN IF NOT EXISTS "BranchId" UUID NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'UQ_Payments_OrganizationId_ReceiptNumber'
    ) THEN
        ALTER TABLE public."Payments"
            ADD CONSTRAINT "UQ_Payments_OrganizationId_ReceiptNumber" 
            UNIQUE ("OrganizationId", "ReceiptNumber");
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Payments_Branches_BranchId'
    ) THEN
        ALTER TABLE public."Payments"
            ADD CONSTRAINT "FK_Payments_Branches_BranchId" 
            FOREIGN KEY ("BranchId") 
            REFERENCES public."Branches"("Id") 
            ON UPDATE CASCADE 
            ON DELETE SET NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS "IX_Payments_ReceiptNumber" 
    ON public."Payments" ("ReceiptNumber");

CREATE INDEX IF NOT EXISTS "IX_Payments_BranchId" 
    ON public."Payments" ("BranchId");

COMMIT;
