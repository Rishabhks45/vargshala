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

-- 2. Insert SuperAdmin User (Rishabh Sharma - Platform Level, No Organization)
-- Password: Admin@12345 (BCrypt Hash: $2a$11$0wGZqPcf9260Y70/eF1dCe1mUjLszn34Xo26q2K7P.CjD1/uV4v1S)
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
    '$2a$11$0wGZqPcf9260Y70/eF1dCe1mUjLszn34Xo26q2K7P.CjD1/uV4v1S',
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

-- 3. Insert OrganizationAdmin User (Rishabh Sharma - Vargshala Institute)
-- Password: Admin@12345
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
    '$2a$11$0wGZqPcf9260Y70/eF1dCe1mUjLszn34Xo26q2K7P.CjD1/uV4v1S',
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

COMMIT;
