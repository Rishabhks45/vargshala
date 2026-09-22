-- =============================================================================
-- VARGSHALA SEED DATA - 35: 7-DAY FREE TRIAL + 3 PAID SUBSCRIPTION PLANS
-- =============================================================================
-- Har statement independent hai, aap pura script ek sath ya 
-- line-by-line run kar sakte hain.
-- =============================================================================

-- 1. 7-Day Free Trial Plan (Price: 0)
INSERT INTO public."SubscriptionPlans" 
    ("Id", "Name", "Description", "Price", "BillingCycle", "MaxStudents", "MaxTeachers", "MaxBranches", "IsActive", "CreatedAt", "IsDeleted")
VALUES 
    ('a0000000-0000-0000-0000-000000000000', '7-Day Free Trial', 'Full platform access for 7 days to evaluate student management, test series, and fees.', 0.00, 'Trial', 50, 5, 1, true, CURRENT_TIMESTAMP, false)
ON CONFLICT ("Id") DO UPDATE 
SET 
    "Name" = EXCLUDED."Name",
    "Description" = EXCLUDED."Description",
    "Price" = EXCLUDED."Price",
    "BillingCycle" = EXCLUDED."BillingCycle",
    "MaxStudents" = EXCLUDED."MaxStudents",
    "MaxTeachers" = EXCLUDED."MaxTeachers",
    "MaxBranches" = EXCLUDED."MaxBranches",
    "IsActive" = EXCLUDED."IsActive",
    "UpdatedAt" = CURRENT_TIMESTAMP;

-- 2. Starter Plan (Monthly ₹499)
INSERT INTO public."SubscriptionPlans" 
    ("Id", "Name", "Description", "Price", "BillingCycle", "MaxStudents", "MaxTeachers", "MaxBranches", "IsActive", "CreatedAt", "IsDeleted")
VALUES 
    ('a1111111-1111-1111-1111-111111111111', 'Starter', 'Perfect for solo tutors and growing coaching centers up to 100 students.', 499.00, 'Monthly', 100, 10, 1, true, CURRENT_TIMESTAMP, false)
ON CONFLICT ("Id") DO UPDATE 
SET 
    "Name" = EXCLUDED."Name",
    "Description" = EXCLUDED."Description",
    "Price" = EXCLUDED."Price",
    "BillingCycle" = EXCLUDED."BillingCycle",
    "MaxStudents" = EXCLUDED."MaxStudents",
    "MaxTeachers" = EXCLUDED."MaxTeachers",
    "MaxBranches" = EXCLUDED."MaxBranches",
    "IsActive" = EXCLUDED."IsActive",
    "UpdatedAt" = CURRENT_TIMESTAMP;

-- 3. Standard Plan (Monthly ₹999 - Most Popular)
INSERT INTO public."SubscriptionPlans" 
    ("Id", "Name", "Description", "Price", "BillingCycle", "MaxStudents", "MaxTeachers", "MaxBranches", "IsActive", "CreatedAt", "IsDeleted")
VALUES 
    ('a2222222-2222-2222-2222-222222222222', 'Standard', 'Our most popular package for established institutes with up to 2 branches and 250 students.', 999.00, 'Monthly', 250, 25, 2, true, CURRENT_TIMESTAMP, false)
ON CONFLICT ("Id") DO UPDATE 
SET 
    "Name" = EXCLUDED."Name",
    "Description" = EXCLUDED."Description",
    "Price" = EXCLUDED."Price",
    "BillingCycle" = EXCLUDED."BillingCycle",
    "MaxStudents" = EXCLUDED."MaxStudents",
    "MaxTeachers" = EXCLUDED."MaxTeachers",
    "MaxBranches" = EXCLUDED."MaxBranches",
    "IsActive" = EXCLUDED."IsActive",
    "UpdatedAt" = CURRENT_TIMESTAMP;

-- 4. Enterprise Plan (Monthly ₹2499 - Unlimited)
INSERT INTO public."SubscriptionPlans" 
    ("Id", "Name", "Description", "Price", "BillingCycle", "MaxStudents", "MaxTeachers", "MaxBranches", "IsActive", "CreatedAt", "IsDeleted")
VALUES 
    ('a3333333-3333-3333-3333-333333333333', 'Enterprise', 'Unlimited students, teachers, and branches with dedicated support and custom branding.', 2499.00, 'Monthly', NULL, NULL, NULL, true, CURRENT_TIMESTAMP, false)
ON CONFLICT ("Id") DO UPDATE 
SET 
    "Name" = EXCLUDED."Name",
    "Description" = EXCLUDED."Description",
    "Price" = EXCLUDED."Price",
    "BillingCycle" = EXCLUDED."BillingCycle",
    "MaxStudents" = EXCLUDED."MaxStudents",
    "MaxTeachers" = EXCLUDED."MaxTeachers",
    "MaxBranches" = EXCLUDED."MaxBranches",
    "IsActive" = EXCLUDED."IsActive",
    "UpdatedAt" = CURRENT_TIMESTAMP;
