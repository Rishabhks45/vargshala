-- ============================================================================
-- Seed Data Script (Phase 1 Baseline)
-- Database: PostgreSQL
-- Super Admin: Rishabh Sharma (rishabh.sharma@vargshala.com)
-- Org Admin: Rishabh Sharma (rishabh.admin@vargshala.com) / Vargshala Institute
-- Default Password: Admin@12345
-- ============================================================================

START TRANSACTION;

-- 1. Insert Default Organization (Vargshala Institute)
INSERT INTO "Organizations" (
    "Id", "Name", "Code", "LogoUrl", "Email", "Mobile",
    "Address", "City", "State", "Pincode", "AcademicSession",
    "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt",
    "IsDeleted", "DeletedBy", "DeletedAt"
)
VALUES (
    '22222222-2222-2222-2222-222222222222',
    'Vargshala Institute',
    'VARGSHALA',
    NULL,
    'contact@vargshala.com',
    '+919876543210',
    'Connaught Place',
    'New Delhi',
    'Delhi',
    '110001',
    '2026-2027',
    true,
    NULL,
    CURRENT_TIMESTAMP,
    NULL,
    NULL,
    false,
    NULL,
    NULL
)
ON CONFLICT ("Id") DO NOTHING;

-- 2. Insert SuperAdmin User (Platform Level, No Organization)
INSERT INTO "Users" (
    "Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile",
    "PasswordHash", "Role", "EmailVerified", "MobileVerified",
    "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime",
    "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt",
    "IsDeleted", "DeletedBy", "DeletedAt"
)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    NULL,
    'Rishabh',
    'Sharma',
    'rishabh.sharma@vargshala.com',
    '+919876543210',
    'm0tcaJvkn/qIu5ojHX5QrhJnQxD1Pc9EiMdPeLsKue+hPcnD8LYwnx83MfmQiSivqY/oc/k=',
    1001, -- Role.SuperAdmin
    true,
    true,
    NULL,
    NULL,
    NULL,
    true,
    NULL,
    CURRENT_TIMESTAMP,
    NULL,
    NULL,
    false,
    NULL,
    NULL
)
ON CONFLICT ("Id") DO NOTHING;

-- 3. Insert OrganizationAdmin User (Vargshala Institute)
INSERT INTO "Users" (
    "Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile",
    "PasswordHash", "Role", "EmailVerified", "MobileVerified",
    "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime",
    "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt",
    "IsDeleted", "DeletedBy", "DeletedAt"
)
VALUES (
    '33333333-3333-3333-3333-333333333333',
    '22222222-2222-2222-2222-222222222222',
    'Rishabh',
    'Sharma',
    'rishabh.admin@vargshala.com',
    '+919876543210',
    'm0tcaJvkn/qIu5ojHX5QrhJnQxD1Pc9EiMdPeLsKue+hPcnD8LYwnx83MfmQiSivqY/oc/k=',
    1, -- Role.OrganizationAdmin
    true,
    true,
    NULL,
    NULL,
    NULL,
    true,
    NULL,
    CURRENT_TIMESTAMP,
    NULL,
    NULL,
    false,
    NULL,
    NULL
)
ON CONFLICT ("Id") DO NOTHING;

-- 4. Insert Main Branch for Vargshala Institute
INSERT INTO "Branches" (
    "Id", "OrganizationId", "Name", "Code", "Email", "Mobile",
    "Address", "City", "State", "Pincode", "Country",
    "IsMainBranch", "UseBranchName", "IsActive", "IsDeleted", "CreatedAt"
)
VALUES (
    '44444444-4444-4444-4444-444444444444',
    '22222222-2222-2222-2222-222222222222',
    'Main Branch',
    'MAIN',
    'contact@vargshala.com',
    '+919876543210',
    'Connaught Place',
    'New Delhi',
    'Delhi',
    '110001',
    'India',
    true,
    true,
    true,
    false,
    CURRENT_TIMESTAMP
)
ON CONFLICT ("Id") DO NOTHING;

-- 5. Map OrganizationAdmin User to Main Branch Access
INSERT INTO "UserBranchAccess" (
    "Id", "UserId", "BranchId", "IsActive", "CreatedAt"
)
VALUES (
    '55555555-5555-5555-5555-555555555555',
    '33333333-3333-3333-3333-333333333333',
    '44444444-4444-4444-4444-444444444444',
    true,
    CURRENT_TIMESTAMP
)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;
