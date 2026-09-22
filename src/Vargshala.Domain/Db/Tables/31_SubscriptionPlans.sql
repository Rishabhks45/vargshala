-- =============================================================================
-- VARGSHALA DATABASE SCHEMA - 31: SUBSCRIPTION PLANS (SAAS PACKAGES)
-- =============================================================================

CREATE TABLE IF NOT EXISTS public."SubscriptionPlans"
(
    "Id" uuid NOT NULL,

    "Name" varchar(100) NOT NULL,

    "Description" text,

    "Price" numeric(12,2) NOT NULL,

    "BillingCycle" varchar(20) NOT NULL,

    "MaxStudents" integer,

    "MaxTeachers" integer,

    "MaxBranches" integer,

    "IsActive" boolean NOT NULL DEFAULT true,

    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,

    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,

    "IsDeleted" boolean NOT NULL DEFAULT false,

    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone,

    CONSTRAINT "PK_SubscriptionPlans"
        PRIMARY KEY ("Id"),

    CONSTRAINT "CK_SubscriptionPlans_Price"
        CHECK ("Price" >= 0),

    CONSTRAINT "CK_SubscriptionPlans_MaxStudents"
        CHECK ("MaxStudents" IS NULL OR "MaxStudents" > 0),

    CONSTRAINT "CK_SubscriptionPlans_MaxTeachers"
        CHECK ("MaxTeachers" IS NULL OR "MaxTeachers" > 0),

    CONSTRAINT "CK_SubscriptionPlans_MaxBranches"
        CHECK ("MaxBranches" IS NULL OR "MaxBranches" > 0)
);

CREATE INDEX IF NOT EXISTS "IX_SubscriptionPlans_IsActive"
ON public."SubscriptionPlans" ("IsActive")
WHERE "IsDeleted" = false;
