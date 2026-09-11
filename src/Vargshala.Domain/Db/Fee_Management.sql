-- ============================================================================
-- VARGSHALA FEE MANAGEMENT MODULE DDL
-- Tables: FeeStructures, StudentFees, FeeDiscounts, FeeInstallments, Payments, PaymentAllocations
-- ============================================================================

CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================================================================
-- 1. FEE STRUCTURES (Fee Template per Branch / Academic Session)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."FeeStructures" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL REFERENCES public."Organizations"("Id") ON DELETE CASCADE,
    "BranchId" UUID NOT NULL REFERENCES public."Branches"("Id") ON DELETE CASCADE,
    "ClassId" UUID NULL REFERENCES public."Classes"("Id") ON DELETE SET NULL,
    "Name" VARCHAR(150) NOT NULL,
    "Description" TEXT,
    "TotalAmount" NUMERIC(12, 2) NOT NULL DEFAULT 0.00,
    "AcademicSession" VARCHAR(50) NOT NULL, -- e.g. '2026-2027'
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    
    -- Audit & Soft Delete
    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "CK_FeeStructures_TotalAmount" CHECK ("TotalAmount" >= 0)
);

CREATE INDEX IF NOT EXISTS "IX_FeeStructures_OrganizationId" ON public."FeeStructures" ("OrganizationId");
CREATE INDEX IF NOT EXISTS "IX_FeeStructures_BranchId" ON public."FeeStructures" ("BranchId");
CREATE INDEX IF NOT EXISTS "IX_FeeStructures_ClassId" ON public."FeeStructures" ("ClassId");
CREATE INDEX IF NOT EXISTS "IX_FeeStructures_Org_Branch" ON public."FeeStructures" ("OrganizationId", "BranchId");
CREATE INDEX IF NOT EXISTS "IX_FeeStructures_Session_IsActive" ON public."FeeStructures" ("AcademicSession", "IsActive");


-- ============================================================================
-- 2. STUDENT FEES (Assigned Fee Structure to a Student)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."StudentFees" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL REFERENCES public."Organizations"("Id") ON DELETE CASCADE,
    "StudentId" UUID NOT NULL REFERENCES public."Students"("Id") ON DELETE CASCADE,
    "FeeStructureId" UUID NOT NULL REFERENCES public."FeeStructures"("Id") ON DELETE RESTRICT,
    "OriginalAmount" NUMERIC(12, 2) NOT NULL,
    "DiscountAmount" NUMERIC(12, 2) NOT NULL DEFAULT 0.00,
    "FinalAmount" NUMERIC(12, 2) NOT NULL,
    "PaidAmount" NUMERIC(12, 2) NOT NULL DEFAULT 0.00,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Pending', -- 'Pending', 'PartiallyPaid', 'Paid', 'Overdue'
    "AssignedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Audit & Soft Delete
    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "CK_StudentFees_Amounts" CHECK ("FinalAmount" = ("OriginalAmount" - "DiscountAmount")),
    CONSTRAINT "CK_StudentFees_PaidAmount" CHECK ("PaidAmount" >= 0 AND "PaidAmount" <= "FinalAmount"),
    CONSTRAINT "CK_StudentFees_Status" CHECK ("Status" IN ('Pending', 'PartiallyPaid', 'Paid', 'Overdue', 'Cancelled'))
);

CREATE INDEX IF NOT EXISTS "IX_StudentFees_OrganizationId" ON public."StudentFees" ("OrganizationId");
CREATE INDEX IF NOT EXISTS "IX_StudentFees_StudentId" ON public."StudentFees" ("StudentId");
CREATE INDEX IF NOT EXISTS "IX_StudentFees_FeeStructureId" ON public."StudentFees" ("FeeStructureId");
CREATE INDEX IF NOT EXISTS "IX_StudentFees_Org_Status" ON public."StudentFees" ("OrganizationId", "Status");


-- ============================================================================
-- 3. FEE DISCOUNTS (Discounts / Scholarships Applied to Student Fee)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."FeeDiscounts" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL REFERENCES public."Organizations"("Id") ON DELETE CASCADE,
    "StudentFeeId" UUID NOT NULL REFERENCES public."StudentFees"("Id") ON DELETE CASCADE,
    "DiscountType" VARCHAR(50) NOT NULL, -- 'Percentage', 'FixedAmount', 'Scholarship', 'Sibling'
    "Value" NUMERIC(12, 2) NOT NULL,      -- e.g. 10 (%) or 5000 (Fixed Amount)
    "Amount" NUMERIC(12, 2) NOT NULL,     -- Actual discounted rupee amount
    "Reason" VARCHAR(255),
    "ApprovedBy" UUID REFERENCES public."Users"("Id") ON DELETE SET NULL,
    "ApprovedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Audit
    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "CK_FeeDiscounts_Amount" CHECK ("Amount" >= 0),
    CONSTRAINT "CK_FeeDiscounts_Value" CHECK ("Value" >= 0)
);

CREATE INDEX IF NOT EXISTS "IX_FeeDiscounts_OrganizationId" ON public."FeeDiscounts" ("OrganizationId");
CREATE INDEX IF NOT EXISTS "IX_FeeDiscounts_StudentFeeId" ON public."FeeDiscounts" ("StudentFeeId");
CREATE INDEX IF NOT EXISTS "IX_FeeDiscounts_ApprovedBy" ON public."FeeDiscounts" ("ApprovedBy");


-- ============================================================================
-- 4. FEE INSTALLMENTS (Installment Breakdown per Student Fee)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."FeeInstallments" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL REFERENCES public."Organizations"("Id") ON DELETE CASCADE,
    "StudentFeeId" UUID NOT NULL REFERENCES public."StudentFees"("Id") ON DELETE CASCADE,
    "InstallmentNumber" INT NOT NULL,
    "Amount" NUMERIC(12, 2) NOT NULL,
    "PaidAmount" NUMERIC(12, 2) NOT NULL DEFAULT 0.00,
    "DueDate" DATE NOT NULL,
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Pending', -- 'Pending', 'PartiallyPaid', 'Paid', 'Overdue'

    -- Audit
    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,

    CONSTRAINT "UQ_FeeInstallments_StudentFeeId_InstallmentNumber" UNIQUE ("StudentFeeId", "InstallmentNumber"),
    CONSTRAINT "CK_FeeInstallments_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "CK_FeeInstallments_PaidAmount" CHECK ("PaidAmount" >= 0 AND "PaidAmount" <= "Amount"),
    CONSTRAINT "CK_FeeInstallments_Status" CHECK ("Status" IN ('Pending', 'PartiallyPaid', 'Paid', 'Overdue', 'Cancelled'))
);

CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_OrganizationId" ON public."FeeInstallments" ("OrganizationId");
CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_StudentFeeId" ON public."FeeInstallments" ("StudentFeeId");
CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_DueDate" ON public."FeeInstallments" ("DueDate");
CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_Status" ON public."FeeInstallments" ("Status");
CREATE INDEX IF NOT EXISTS "IX_FeeInstallments_StudentFee_DueDate" ON public."FeeInstallments" ("StudentFeeId", "DueDate");


-- ============================================================================
-- 5. PAYMENTS (Receipts / Inward Student Payments)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."Payments" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL REFERENCES public."Organizations"("Id") ON DELETE CASCADE,
    "BranchId" UUID NULL REFERENCES public."Branches"("Id") ON DELETE SET NULL,
    "StudentId" UUID NOT NULL REFERENCES public."Students"("Id") ON DELETE CASCADE,
    "ReceiptNumber" VARCHAR(50) NULL,
    "Amount" NUMERIC(12, 2) NOT NULL,
    "PaymentDate" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "PaymentMethod" VARCHAR(50) NOT NULL, -- 'Cash', 'UPI', 'NetBanking', 'Card', 'Cheque', 'DemandDraft'
    "TransactionReference" VARCHAR(100),  -- UTR / Bank Reference
    "Status" VARCHAR(30) NOT NULL DEFAULT 'Completed', -- 'Completed', 'Pending', 'Failed', 'Refunded'
    "Remarks" TEXT,

    -- Audit & Soft Delete
    "CreatedBy" UUID REFERENCES public."Users"("Id") ON DELETE SET NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" UUID,
    "UpdatedAt" TIMESTAMPTZ,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedBy" UUID,
    "DeletedAt" TIMESTAMPTZ,

    CONSTRAINT "UQ_Payments_OrganizationId_ReceiptNumber" UNIQUE ("OrganizationId", "ReceiptNumber"),
    CONSTRAINT "CK_Payments_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "CK_Payments_Status" CHECK ("Status" IN ('Completed', 'Pending', 'Failed', 'Refunded'))
);

CREATE INDEX IF NOT EXISTS "IX_Payments_OrganizationId" ON public."Payments" ("OrganizationId");
CREATE INDEX IF NOT EXISTS "IX_Payments_BranchId" ON public."Payments" ("BranchId");
CREATE INDEX IF NOT EXISTS "IX_Payments_StudentId" ON public."Payments" ("StudentId");
CREATE INDEX IF NOT EXISTS "IX_Payments_ReceiptNumber" ON public."Payments" ("ReceiptNumber");
CREATE INDEX IF NOT EXISTS "IX_Payments_PaymentDate" ON public."Payments" ("PaymentDate");
CREATE INDEX IF NOT EXISTS "IX_Payments_TransactionReference" ON public."Payments" ("TransactionReference");
CREATE INDEX IF NOT EXISTS "IX_Payments_Org_Date" ON public."Payments" ("OrganizationId", "PaymentDate");


-- ============================================================================
-- 6. PAYMENT ALLOCATIONS (Mapping Payment to Specific Installments)
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."PaymentAllocations" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "PaymentId" UUID NOT NULL REFERENCES public."Payments"("Id") ON DELETE CASCADE,
    "FeeInstallmentId" UUID NOT NULL REFERENCES public."FeeInstallments"("Id") ON DELETE RESTRICT,
    "AllocatedAmount" NUMERIC(12, 2) NOT NULL,

    -- Audit
    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "UQ_PaymentAllocations_PaymentId_FeeInstallmentId" UNIQUE ("PaymentId", "FeeInstallmentId"),
    CONSTRAINT "CK_PaymentAllocations_Amount" CHECK ("AllocatedAmount" > 0)
);

CREATE INDEX IF NOT EXISTS "IX_PaymentAllocations_PaymentId" ON public."PaymentAllocations" ("PaymentId");
CREATE INDEX IF NOT EXISTS "IX_PaymentAllocations_FeeInstallmentId" ON public."PaymentAllocations" ("FeeInstallmentId");
