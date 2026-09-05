-- ============================================================================
-- Vargshala - Coupons & Discounts Module
-- Database: PostgreSQL
-- Enums Reference:
--   Category (CampaignCategory):
--     1 = LaunchOffer ("Launch Offer")
--     2 = Promotional ("Promotional")
--     3 = Seasonal ("Seasonal / Festive")
--     4 = VipPromo ("VIP / Corporate")
--     5 = Retention ("Retention / Winback")
--     6 = General ("General")
--
--   DiscountType:
--     1 = Percentage ("Percentage (%)")
--     2 = FlatAmount ("Flat Amount (₹)")
--
--   ApplicablePlan:
--     1 = AllPlans ("All Plans")
--     2 = Standard ("Standard Plan")
--     3 = ProInstitute ("Pro Institute")
--     4 = Enterprise ("Enterprise")
-- ============================================================================

START TRANSACTION;

-- ============================================================================
-- 1. Coupons Table
-- ============================================================================
CREATE TABLE IF NOT EXISTS public."Coupons"
(
    -- Primary Key & Multi-Tenancy
    "Id"                    UUID                     NOT NULL,
    "OrganizationId"        UUID                     NULL,     -- NULL for Platform/SaaS Subscription coupons, or Org Id for Institute-level student discounts
    
    -- Coupon & Campaign Details
    "Code"                  VARCHAR(50)              NOT NULL, -- E.g. 'WELCOME50', 'FESTIVE25', 'PROANNUAL'
    "Category"              INTEGER                  NOT NULL DEFAULT 2, -- CampaignCategory Enum (1=LaunchOffer, 2=Promotional, 3=Seasonal, etc.)
    "Description"           VARCHAR(500)             NULL,
    
    -- Discount Calculation & Conditions
    "DiscountType"          INTEGER                  NOT NULL DEFAULT 1, -- DiscountType Enum (1=Percentage %, 2=Flat Amount ₹)
    "DiscountValue"         NUMERIC(18, 2)           NOT NULL, -- Discount value (e.g. 50.00 for 50%, or 1000.00 for ₹1,000)
    "MinOrderAmount"        NUMERIC(18, 2)           NULL,     -- Minimum order/subscription cart amount required
    "MaxDiscountAmount"     NUMERIC(18, 2)           NULL,     -- Maximum discount cap (useful for percentage discounts)
    
    -- Scope & Quota
    "ApplicablePlan"        INTEGER                  NOT NULL DEFAULT 1, -- ApplicablePlan Enum (1=AllPlans, 2=Standard, 3=ProInstitute, 4=Enterprise)
    "UsedCount"             INTEGER                  NOT NULL DEFAULT 0, -- Total times redeemed
    "MaxUses"               INTEGER                  NOT NULL DEFAULT 100, -- Maximum allowed redemptions
    
    -- Validity
    "ExpiryDate"            TIMESTAMP WITH TIME ZONE NOT NULL,
    
    -- BaseEntity Status & Audit Trail
    "IsActive"              BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"             UUID                     NULL,
    "CreatedAt"             TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"             UUID                     NULL,
    "UpdatedAt"             TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"             BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"             UUID                     NULL,
    "DeletedAt"             TIMESTAMP WITH TIME ZONE NULL,

    CONSTRAINT "PK_Coupons" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Coupons_Organizations_OrganizationId" 
        FOREIGN KEY ("OrganizationId") 
        REFERENCES public."Organizations" ("Id") 
        ON DELETE RESTRICT
);

-- ============================================================================
-- 2. Indexes for Coupons
-- ============================================================================
-- Unique constraint on Coupon Code for active/non-deleted records
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Coupons_Code_NonDeleted"
    ON public."Coupons" ("Code")
    WHERE "IsDeleted" = false;

-- Filter index for fast tenant query and status lookups
CREATE INDEX IF NOT EXISTS "IX_Coupons_Org_Active"
    ON public."Coupons" ("OrganizationId", "IsActive")
    WHERE "IsDeleted" = false;

-- Expiry date index for validity checks
CREATE INDEX IF NOT EXISTS "IX_Coupons_ExpiryDate"
    ON public."Coupons" ("ExpiryDate");

-- Category and Plan filters index
CREATE INDEX IF NOT EXISTS "IX_Coupons_Category_Plan"
    ON public."Coupons" ("Category", "ApplicablePlan")
    WHERE "IsDeleted" = false;

-- ============================================================================
-- 3. Standard Seed Coupons & Promotional Campaigns
-- ============================================================================
INSERT INTO public."Coupons" (
    "Id", "OrganizationId", "Code", "Category", "Description", 
    "DiscountType", "DiscountValue", "MinOrderAmount", "MaxDiscountAmount", 
    "ApplicablePlan", "UsedCount", "MaxUses", "ExpiryDate", "IsActive", "CreatedAt", "IsDeleted"
) VALUES 
(
    'c1111111-1111-1111-1111-111111111111',
    NULL,
    'WELCOME50',
    1, -- LaunchOffer
    'Flat 50% discount for first-time coaching institute registrations',
    1, -- Percentage
    50.00,
    NULL,
    1500.00,
    1, -- AllPlans
    84,
    100,
    '2026-12-31 23:59:59+00',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'c2222222-2222-2222-2222-222222222222',
    NULL,
    'PROANNUAL',
    4, -- VipPromo
    'Flat ₹1,000 off on Pro Institute annual subscription',
    2, -- FlatAmount
    1000.00,
    2499.00,
    NULL,
    3, -- ProInstitute
    36,
    50,
    '2026-11-15 23:59:59+00',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'c3333333-3333-3333-3333-333333333333',
    NULL,
    'STARTUP20',
    2, -- Promotional
    '20% off for newly established coaching institutes (<50 students)',
    1, -- Percentage
    20.00,
    NULL,
    500.00,
    2, -- Standard
    28,
    50,
    '2026-10-30 23:59:59+00',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'c4444444-4444-4444-4444-444444444444',
    NULL,
    'ENTERPRISEVIP',
    4, -- VipPromo
    'Special VIP discount for multi-branch enterprise networks',
    2, -- FlatAmount
    2000.00,
    5999.00,
    NULL,
    4, -- Enterprise
    12,
    20,
    '2026-12-31 23:59:59+00',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'c5555555-5555-5555-5555-555555555555',
    NULL,
    'FESTIVE25',
    3, -- Seasonal
    'Special festive discount on any annual billing tier',
    1, -- Percentage
    25.00,
    1999.00,
    1000.00,
    1, -- AllPlans
    19,
    75,
    '2026-09-20 23:59:59+00',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'c6666666-6666-6666-6666-666666666666',
    NULL,
    'EARLYBIRD',
    1, -- LaunchOffer
    'Early beta tester launch incentive campaign',
    1, -- Percentage
    30.00,
    NULL,
    750.00,
    1, -- AllPlans
    8,
    50,
    '2026-09-12 23:59:59+00',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;
