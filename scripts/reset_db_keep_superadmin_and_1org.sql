-- ==============================================================================
-- VARGSHALA SAAS - DATABASE RESET SCRIPT
-- ==============================================================================
-- Purpose:
--   1. Keeps exactly 1 SuperAdmin user login (Role = 1001).
--   2. Keeps exactly 1 Organization + its 1 OrgAdmin user login (Role = 1) + 1 Main Branch.
--   3. Completely WIPES all operational, academic, fee, and communication tables (0 rows).
--   4. Deletes all other organizations, branches, and users.
--
-- Target Database: PostgreSQL (Supabase / Self-hosted)
-- ==============================================================================

BEGIN;

DO $$
DECLARE
    v_superadmin_id UUID;
    v_superadmin_email TEXT;
    
    v_org_id UUID;
    v_org_name TEXT;
    
    v_orgadmin_id UUID;
    v_orgadmin_email TEXT;
    
    v_branch_id UUID;
    v_branch_name TEXT;
BEGIN
    -- -------------------------------------------------------------------------
    -- STEP 1: Identify the SuperAdmin to preserve (Role = 1001)
    -- -------------------------------------------------------------------------
    SELECT "Id", "Email" INTO v_superadmin_id, v_superadmin_email
    FROM "Users"
    WHERE "Role" = 1001 AND "IsDeleted" = false
    ORDER BY "CreatedAt" ASC
    LIMIT 1;

    -- Fallback: If no non-deleted SuperAdmin exists, check any SuperAdmin
    IF v_superadmin_id IS NULL THEN
        SELECT "Id", "Email" INTO v_superadmin_id, v_superadmin_email
        FROM "Users"
        WHERE "Role" = 1001
        ORDER BY "CreatedAt" ASC
        LIMIT 1;
    END IF;

    IF v_superadmin_id IS NULL THEN
        RAISE EXCEPTION 'ERROR: No SuperAdmin user found with Role = 1001. Please create or verify your SuperAdmin before running this script.';
    END IF;

    -- -------------------------------------------------------------------------
    -- STEP 2: Identify the 1 Organization to preserve
    -- -------------------------------------------------------------------------
    SELECT "Id", "Name" INTO v_org_id, v_org_name
    FROM "Organizations"
    WHERE "IsDeleted" = false
    ORDER BY "CreatedAt" ASC
    LIMIT 1;

    -- Fallback: If no non-deleted Organization exists, check any Organization
    IF v_org_id IS NULL THEN
        SELECT "Id", "Name" INTO v_org_id, v_org_name
        FROM "Organizations"
        ORDER BY "CreatedAt" ASC
        LIMIT 1;
    END IF;

    IF v_org_id IS NULL THEN
        RAISE EXCEPTION 'ERROR: No Organization found. Please register at least 1 Organization before running this script.';
    END IF;

    -- -------------------------------------------------------------------------
    -- STEP 3: Identify the OrgAdmin (Role = 1) for this Organization
    -- -------------------------------------------------------------------------
    SELECT "Id", "Email" INTO v_orgadmin_id, v_orgadmin_email
    FROM "Users"
    WHERE "OrganizationId" = v_org_id AND "Role" = 1 AND "IsDeleted" = false
    ORDER BY "CreatedAt" ASC
    LIMIT 1;

    IF v_orgadmin_id IS NULL THEN
        SELECT "Id", "Email" INTO v_orgadmin_id, v_orgadmin_email
        FROM "Users"
        WHERE "OrganizationId" = v_org_id AND "Role" = 1
        ORDER BY "CreatedAt" ASC
        LIMIT 1;
    END IF;

    IF v_orgadmin_id IS NULL THEN
        RAISE EXCEPTION 'ERROR: No OrgAdmin user (Role = 1) found for Organization "%" (ID: %).', v_org_name, v_org_id;
    END IF;

    -- -------------------------------------------------------------------------
    -- STEP 4: Identify the 1 Main Branch for this Organization
    -- -------------------------------------------------------------------------
    SELECT "Id", "Name" INTO v_branch_id, v_branch_name
    FROM "Branches"
    WHERE "OrganizationId" = v_org_id AND "IsMainBranch" = true AND "IsDeleted" = false
    ORDER BY "CreatedAt" ASC
    LIMIT 1;

    IF v_branch_id IS NULL THEN
        SELECT "Id", "Name" INTO v_branch_id, v_branch_name
        FROM "Branches"
        WHERE "OrganizationId" = v_org_id
        ORDER BY "CreatedAt" ASC
        LIMIT 1;
    END IF;

    IF v_branch_id IS NULL THEN
        RAISE EXCEPTION 'ERROR: No Branch found for Organization "%" (ID: %).', v_org_name, v_org_id;
    END IF;

    -- Log what will be preserved
    RAISE NOTICE '=========================================================';
    RAISE NOTICE 'PRESERVING THE FOLLOWING IDENTITIES:';
    RAISE NOTICE '  * SuperAdmin  : % (ID: %)', v_superadmin_email, v_superadmin_id;
    RAISE NOTICE '  * Organization: % (ID: %)', v_org_name, v_org_id;
    RAISE NOTICE '  * OrgAdmin    : % (ID: %)', v_orgadmin_email, v_orgadmin_id;
    RAISE NOTICE '  * Main Branch : % (ID: %)', v_branch_name, v_branch_id;
    RAISE NOTICE '=========================================================';

    -- -------------------------------------------------------------------------
    -- STEP 5: TRUNCATE All Transactional, Operational & Child Tables
    -- -------------------------------------------------------------------------
    -- Messaging & Communication Tables
    TRUNCATE TABLE 
        "AnnouncementReplyPermissions",
        "MessageReactions",
        "MessageReads",
        "MessageAttachments",
        "Messages",
        "ConversationAdmins",
        "ConversationParticipants",
        "Conversations"
    CASCADE;

    -- Fees & Payment Tables
    TRUNCATE TABLE 
        "PaymentAllocations",
        "Payments",
        "FeeInstallments",
        "FeeDiscounts",
        "StudentFees",
        "FeeStructures"
    CASCADE;

    -- Academic & Scheduling Tables
    TRUNCATE TABLE 
        "Attendances",
        "ClassSessions",
        "BatchSchedules",
        "BatchStudents",
        "BatchTeachers",
        "Batches",
        "Classes",
        "Subjects"
    CASCADE;

    -- Students & Teachers Profiles
    TRUNCATE TABLE 
        "Students",
        "Teachers"
    CASCADE;

    -- Promotional Coupons
    TRUNCATE TABLE 
        "Coupons"
    CASCADE;

    -- -------------------------------------------------------------------------
    -- STEP 6: Clean UserBranchAccess (Keep only preserved OrgAdmin <-> Branch)
    -- -------------------------------------------------------------------------
    DELETE FROM "UserBranchAccess"
    WHERE NOT ("UserId" = v_orgadmin_id AND "BranchId" = v_branch_id);

    -- Ensure the single mapping exists and is active
    IF NOT EXISTS (
        SELECT 1 FROM "UserBranchAccess" 
        WHERE "UserId" = v_orgadmin_id AND "BranchId" = v_branch_id
    ) THEN
        INSERT INTO "UserBranchAccess" ("Id", "UserId", "BranchId", "IsActive", "CreatedAt")
        VALUES (gen_random_uuid(), v_orgadmin_id, v_branch_id, true, NOW());
    ELSE
        UPDATE "UserBranchAccess" 
        SET "IsActive" = true
        WHERE "UserId" = v_orgadmin_id AND "BranchId" = v_branch_id;
    END IF;

    -- -------------------------------------------------------------------------
    -- STEP 7: Clean Branches (Keep only preserved Main Branch)
    -- -------------------------------------------------------------------------
    DELETE FROM "Branches"
    WHERE "Id" != v_branch_id;

    UPDATE "Branches"
    SET "IsActive" = true, "IsDeleted" = false, "IsMainBranch" = true
    WHERE "Id" = v_branch_id;

    -- -------------------------------------------------------------------------
    -- STEP 8: Clean Users (Keep only SuperAdmin and 1 OrgAdmin)
    -- -------------------------------------------------------------------------
    DELETE FROM "Users"
    WHERE "Id" NOT IN (v_superadmin_id, v_orgadmin_id);

    UPDATE "Users"
    SET "IsActive" = true, "IsDeleted" = false
    WHERE "Id" IN (v_superadmin_id, v_orgadmin_id);

    -- -------------------------------------------------------------------------
    -- STEP 9: Clean Organizations (Keep only preserved Organization)
    -- -------------------------------------------------------------------------
    DELETE FROM "Organizations"
    WHERE "Id" != v_org_id;

    UPDATE "Organizations"
    SET "IsActive" = true, "IsDeleted" = false
    WHERE "Id" = v_org_id;

    -- -------------------------------------------------------------------------
    -- STEP 10: Clean EmailTemplates (Delete templates of other organizations)
    -- -------------------------------------------------------------------------
    DELETE FROM "EmailTemplates"
    WHERE "OrganizationId" IS NOT NULL AND "OrganizationId" != v_org_id;

    RAISE NOTICE 'SUCCESS: Database cleared! Only SuperAdmin and 1 Organization login remain.';
END $$;

COMMIT;

-- -----------------------------------------------------------------------------
-- VERIFICATION: Count rows across all tables
-- -----------------------------------------------------------------------------
SELECT 'Organizations' AS table_name, COUNT(*) AS remaining_count FROM "Organizations"
UNION ALL SELECT 'Users (SuperAdmin + OrgAdmin)', COUNT(*) FROM "Users"
UNION ALL SELECT 'Branches (Main Branch)', COUNT(*) FROM "Branches"
UNION ALL SELECT 'UserBranchAccess', COUNT(*) FROM "UserBranchAccess"
UNION ALL SELECT 'Students', COUNT(*) FROM "Students"
UNION ALL SELECT 'Teachers', COUNT(*) FROM "Teachers"
UNION ALL SELECT 'Classes', COUNT(*) FROM "Classes"
UNION ALL SELECT 'Batches', COUNT(*) FROM "Batches"
UNION ALL SELECT 'Subjects', COUNT(*) FROM "Subjects"
UNION ALL SELECT 'ClassSessions', COUNT(*) FROM "ClassSessions"
UNION ALL SELECT 'Attendances', COUNT(*) FROM "Attendances"
UNION ALL SELECT 'FeeStructures', COUNT(*) FROM "FeeStructures"
UNION ALL SELECT 'StudentFees', COUNT(*) FROM "StudentFees"
UNION ALL SELECT 'Payments', COUNT(*) FROM "Payments"
UNION ALL SELECT 'Conversations', COUNT(*) FROM "Conversations"
UNION ALL SELECT 'Messages', COUNT(*) FROM "Messages"
UNION ALL SELECT 'Coupons', COUNT(*) FROM "Coupons";
