-- =============================================================================
-- VARGSHALA SEED DATA - 34: SUBSCRIPTION PLANS (DEFAULT SAAS PACKAGES)
-- =============================================================================

INSERT INTO public."SubscriptionPlans" 
(
    "Id", 
    "Name", 
    "Description", 
    "Price", 
    "BillingCycle", 
    "MaxStudents", 
    "MaxTeachers", 
    "MaxBranches", 
    "IsActive", 
    "CreatedAt", 
    "IsDeleted"
)
VALUES
(
    'a1111111-1111-1111-1111-111111111111',
    'Starter',
    'Perfect for solo tutors and small coaching centers starting out.',
    499.00,
    'Monthly',
    100,
    10,
    1,
    true,
    CURRENT_TIMESTAMP,
    false
),
(
    'a2222222-2222-2222-2222-222222222222',
    'Standard',
    'Our most popular plan for growing coaching institutes with 2 branches.',
    999.00,
    'Monthly',
    250,
    25,
    2,
    true,
    CURRENT_TIMESTAMP,
    false
),
(
    'a3333333-3333-3333-3333-333333333333',
    'Pro Institute',
    'Comprehensive management for multi-branch institutes and test prep academies.',
    2499.00,
    'Monthly',
    1000,
    100,
    5,
    true,
    CURRENT_TIMESTAMP,
    false
),
(
    'a4444444-4444-4444-4444-444444444444',
    'Enterprise',
    'Unlimited students, teachers, and branches with custom branding and priority support.',
    5999.00,
    'Monthly',
    NULL,
    NULL,
    NULL,
    true,
    CURRENT_TIMESTAMP,
    false
)
ON CONFLICT ("Id") DO UPDATE 
SET 
    "Price" = EXCLUDED."Price",
    "MaxStudents" = EXCLUDED."MaxStudents",
    "MaxTeachers" = EXCLUDED."MaxTeachers",
    "MaxBranches" = EXCLUDED."MaxBranches",
    "UpdatedAt" = CURRENT_TIMESTAMP;
