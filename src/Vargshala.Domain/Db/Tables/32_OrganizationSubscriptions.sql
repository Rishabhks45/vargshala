-- =============================================================================
-- VARGSHALA DATABASE SCHEMA - 32: ORGANIZATION SUBSCRIPTIONS
-- =============================================================================

CREATE TABLE IF NOT EXISTS public."OrganizationSubscriptions"
(
    "Id" uuid NOT NULL,

    "OrganizationId" uuid NOT NULL,

    "PlanId" uuid NOT NULL,

    "StartDate" timestamp with time zone NOT NULL,

    "EndDate" timestamp with time zone NOT NULL,

    "Status" varchar(20) NOT NULL DEFAULT 'Trial',

    "AutoRenew" boolean NOT NULL DEFAULT false,

    "IsActive" boolean NOT NULL DEFAULT true,

    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,

    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,

    "IsDeleted" boolean NOT NULL DEFAULT false,

    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone,

    CONSTRAINT "PK_OrganizationSubscriptions"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_OrganizationSubscriptions_Organizations"
        FOREIGN KEY ("OrganizationId")
        REFERENCES public."Organizations" ("Id")
        ON DELETE RESTRICT,

    CONSTRAINT "FK_OrganizationSubscriptions_SubscriptionPlans"
        FOREIGN KEY ("PlanId")
        REFERENCES public."SubscriptionPlans" ("Id")
        ON DELETE RESTRICT,

    CONSTRAINT "CK_OrganizationSubscriptions_Date"
        CHECK ("EndDate" > "StartDate"),

    CONSTRAINT "CK_OrganizationSubscriptions_Status"
        CHECK (
            "Status" IN
            (
                'Trial',
                'Active',
                'Expired',
                'Cancelled',
                'Suspended'
            )
        )
);

CREATE INDEX IF NOT EXISTS "IX_OrganizationSubscriptions_OrganizationId"
ON public."OrganizationSubscriptions" ("OrganizationId");

CREATE INDEX IF NOT EXISTS "IX_OrganizationSubscriptions_PlanId"
ON public."OrganizationSubscriptions" ("PlanId");

CREATE INDEX IF NOT EXISTS "IX_OrganizationSubscriptions_Status"
ON public."OrganizationSubscriptions" ("Status");

CREATE INDEX IF NOT EXISTS "IX_OrganizationSubscriptions_EndDate"
ON public."OrganizationSubscriptions" ("EndDate");
