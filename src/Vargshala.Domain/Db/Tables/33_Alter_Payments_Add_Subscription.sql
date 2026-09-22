-- =============================================================================
-- VARGSHALA DATABASE SCHEMA - 33: ALTER PAYMENTS FOR SAAS SUBSCRIPTIONS
-- =============================================================================

-- 1. Make StudentId nullable (since Subscription payments belong to Organization, not Student)
ALTER TABLE public."Payments" 
ALTER COLUMN "StudentId" DROP NOT NULL;

-- 2. Add PaymentType column ('StudentFee' or 'Subscription')
ALTER TABLE public."Payments"
ADD COLUMN IF NOT EXISTS "PaymentType" varchar(30) NOT NULL DEFAULT 'StudentFee';

-- 3. Add OrganizationSubscriptionId FK column
ALTER TABLE public."Payments"
ADD COLUMN IF NOT EXISTS "OrganizationSubscriptionId" uuid;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_Payments_OrganizationSubscriptions'
    ) THEN
        ALTER TABLE public."Payments"
        ADD CONSTRAINT "FK_Payments_OrganizationSubscriptions"
            FOREIGN KEY ("OrganizationSubscriptionId")
            REFERENCES public."OrganizationSubscriptions" ("Id")
            ON DELETE RESTRICT;
    END IF;
END $$;

-- 4. Create Index on OrganizationSubscriptionId
CREATE INDEX IF NOT EXISTS "IX_Payments_OrganizationSubscriptionId"
ON public."Payments" ("OrganizationSubscriptionId");

-- 5. Create Index on PaymentType
CREATE INDEX IF NOT EXISTS "IX_Payments_PaymentType"
ON public."Payments" ("PaymentType");
