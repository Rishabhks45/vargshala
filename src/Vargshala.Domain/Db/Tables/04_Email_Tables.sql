-- ============================================================================
-- Vargshala - Email Module (Templates)
-- Database: PostgreSQL
-- Flow: Category -> TargetRole -> Template (Code & Name) -> Subject -> Placeholders -> Body
-- ============================================================================

START TRANSACTION;

-- ============================================================================
-- 1. EmailTemplates Table
-- ============================================================================
-- "Category" (EmailTemplateCategory Enum):
--   1 = Onboarding
--   2 = Auth & Security
--   3 = Billing & Invoicing
--   4 = System Notices
--
-- "TargetRole" (UserRole Enum):
--   NULL = All Roles / General
--   1    = OrganizationAdmin (Institute Admin)
--   2    = Teacher
--   3    = Student
--   1001 = SuperAdmin
--   1002 = BackOffice
--
-- "Code" (EmailTemplateName Code):
--   E.g. 'WELCOME_ONBOARD', 'FORGOT_PASSWORD', 'PASSWORD_RESET', 'FEE_RECEIPT'
--
-- "Name" (EmailTemplateName Display Label):
--   E.g. 'Welcome & Onboarding', 'Forgot Password', 'Fee Payment Receipt'
-- ============================================================================

CREATE TABLE IF NOT EXISTS public."EmailTemplates"
(
    "Id"                    UUID                     NOT NULL,
    "OrganizationId"        UUID                     NULL,     -- NULL for Platform/Global Templates, or specific Org Id
    
    -- Step 1: Category
    "Category"              INTEGER                  NOT NULL DEFAULT 1, -- 1=Onboarding, 2=Auth & Security, 3=Billing, 4=System
    
    -- Step 2: Target Role (Audience)
    "TargetRole"            INTEGER                  NULL,     -- NULL=All Roles, 1001=SuperAdmin, 1=OrgAdmin, 2=Teacher, 3=Student
    
    -- Step 3: Template (Code & Display Name)
    "Code"                  VARCHAR(50)              NOT NULL, -- 'WELCOME_ONBOARD', 'FORGOT_PASSWORD', 'PASSWORD_RESET'
    "Name"                  VARCHAR(150)             NOT NULL, -- 'Welcome & Onboarding', 'Forgot Password', 'Password Reset'
    
    -- Step 4: Subject Line
    "Subject"               VARCHAR(250)             NOT NULL,
    
    -- Step 5: Available Placeholders (Tokens)
    "AvailablePlaceholders" VARCHAR(1000)            NULL,     -- '{{InstituteName}},{{RecipientName}},{{LoginUrl}}'
    
    -- Step 6: Body Content & Notes
    "BodyHtml"              TEXT                     NOT NULL,
    "Description"           VARCHAR(500)             NULL,
    
    -- Status & Audit Trail
    "IsActive"              BOOLEAN                  NOT NULL DEFAULT TRUE,
    "CreatedBy"             UUID                     NULL,
    "CreatedAt"             TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy"             UUID                     NULL,
    "UpdatedAt"             TIMESTAMP WITH TIME ZONE NULL,
    "IsDeleted"             BOOLEAN                  NOT NULL DEFAULT FALSE,
    "DeletedBy"             UUID                     NULL,
    "DeletedAt"             TIMESTAMP WITH TIME ZONE NULL,

    CONSTRAINT "PK_EmailTemplates" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EmailTemplates_Organizations_OrganizationId" 
        FOREIGN KEY ("OrganizationId") 
        REFERENCES public."Organizations" ("Id") 
        ON DELETE RESTRICT
);

-- ============================================================================
-- 2. Indexes for EmailTemplates
-- ============================================================================
CREATE UNIQUE INDEX IF NOT EXISTS "IX_EmailTemplates_Code_OrganizationId"
    ON public."EmailTemplates" ("Code", "OrganizationId")
    WHERE "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_EmailTemplates_Category"
    ON public."EmailTemplates" ("Category")
    WHERE "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_EmailTemplates_TargetRole"
    ON public."EmailTemplates" ("TargetRole")
    WHERE "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_EmailTemplates_OrganizationId"
    ON public."EmailTemplates" ("OrganizationId");


-- ============================================================================
-- 3. Standard Seed Templates
-- ============================================================================
INSERT INTO public."EmailTemplates" (
    "Id", "OrganizationId", "Category", "TargetRole", "Code", "Name", "Subject", "BodyHtml", "Description", "AvailablePlaceholders", "IsActive", "CreatedAt", "IsDeleted"
) VALUES 
(
    'a1111111-1111-1111-1111-111111111111',
    NULL,
    1, -- Category: Onboarding
    NULL, -- TargetRole: All Roles
    'WELCOME_ONBOARD',
    'Welcome & Onboarding',
    'Welcome to {{InstituteName}}!',
    '<div style="font-family:sans-serif;padding:24px;"><h2>Welcome {{RecipientName}}!</h2><p>Your account at {{InstituteName}} is ready. <a href="{{LoginUrl}}">Click here to Login</a></p></div>',
    'Triggered automatically when a new student or teacher account is created',
    '{{InstituteName}},{{RecipientName}},{{LoginUrl}},{{SupportEmail}}',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'a2222222-2222-2222-2222-222222222222',
    NULL,
    2, -- Category: Auth & Security
    NULL, -- TargetRole: All Roles
    'FORGOT_PASSWORD',
    'Forgot Password',
    'OTP for Password Reset - {{InstituteName}}',
    '<div style="font-family:sans-serif;padding:24px;"><h2>Password Recovery</h2><p>Use OTP <strong>{{OtpCode}}</strong> to reset your password. Valid for 10 minutes.</p></div>',
    'Sent when user requests password recovery via OTP',
    '{{InstituteName}},{{RecipientName}},{{OtpCode}}',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'a3333333-3333-3333-3333-333333333333',
    NULL,
    2, -- Category: Auth & Security
    NULL, -- TargetRole: All Roles
    'PASSWORD_RESET',
    'Password Reset Link',
    'Reset your password for {{InstituteName}}',
    '<div style="font-family:sans-serif;padding:24px;"><h2>Password Reset</h2><p>Click the link below to set a new password:</p><p><a href="{{ResetUrl}}">Reset Password</a></p></div>',
    'Sent when user requests a password reset link',
    '{{InstituteName}},{{RecipientName}},{{ResetUrl}}',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'a4444444-4444-4444-4444-444444444444',
    NULL,
    2, -- Category: Auth & Security
    NULL, -- TargetRole: All Roles
    'VERIFICATION_OTP',
    'Verification OTP',
    'Your Verification Code - {{InstituteName}}',
    '<div style="font-family:sans-serif;padding:24px;"><h2>Verify Your Account</h2><p>Your verification code is <strong>{{OtpCode}}</strong>. Valid for 10 minutes.</p></div>',
    'Sent to verify user email address during onboarding',
    '{{InstituteName}},{{RecipientName}},{{OtpCode}}',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'a5555555-5555-5555-5555-555555555555',
    NULL,
    1, -- Category: Onboarding
    3, -- TargetRole: Student
    'ADMISSION_CONFIRMATION',
    'Admission Confirmation',
    'Admission Confirmed - {{BatchName}} | {{InstituteName}}',
    '<div style="font-family:sans-serif;padding:24px;"><h2>Congratulations {{RecipientName}}!</h2><p>Your admission for {{BatchName}} has been confirmed. Roll No: {{RollNumber}}.</p></div>',
    'Sent to student & parents when admission is processed',
    '{{InstituteName}},{{RecipientName}},{{BatchName}},{{RollNumber}}',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'a6666666-6666-6666-6666-666666666666',
    NULL,
    3, -- Category: Billing & Invoicing
    3, -- TargetRole: Student
    'FEE_RECEIPT',
    'Fee Payment Receipt',
    'Payment Receipt #{{InvoiceNumber}} - {{InstituteName}}',
    '<div style="font-family:sans-serif;padding:24px;"><h2>Fee Payment Successful</h2><p>Dear {{RecipientName}}, we received payment of {{AmountPaid}} for invoice {{InvoiceNumber}}.</p></div>',
    'Sent to parents/students upon fee payment confirmation',
    '{{InstituteName}},{{RecipientName}},{{InvoiceNumber}},{{AmountPaid}}',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'a7777777-7777-7777-7777-777777777777',
    NULL,
    3, -- Category: Billing & Invoicing
    3, -- TargetRole: Student
    'FEE_DUE_REMINDER',
    'Fee Due Reminder',
    'Fee Payment Reminder - {{InstituteName}}',
    '<div style="font-family:sans-serif;padding:24px;"><h2>Payment Reminder</h2><p>Dear {{RecipientName}}, an installment of {{AmountDue}} is due on {{DueDate}}.</p></div>',
    'Sent before or on the due date for upcoming fee installments',
    '{{InstituteName}},{{RecipientName}},{{AmountDue}},{{DueDate}}',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'a8888888-8888-8888-8888-888888888888',
    NULL,
    4, -- Category: System Notices
    3, -- TargetRole: Student
    'ATTENDANCE_ALERT',
    'Attendance Alert',
    'Attendance Alert: {{RecipientName}} is Absent today',
    '<div style="font-family:sans-serif;padding:24px;"><h2>Attendance Notice</h2><p>Dear Parent, {{RecipientName}} was marked absent for {{BatchName}} on {{Date}}.</p></div>',
    'Triggered when student is marked absent in daily attendance',
    '{{InstituteName}},{{RecipientName}},{{BatchName}},{{Date}}',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'a9999999-9999-9999-9999-999999999999',
    NULL,
    4, -- Category: System Notices
    3, -- TargetRole: Student
    'EXAM_NOTICE',
    'Exam & Quiz Notice',
    'Upcoming Exam Schedule - {{InstituteName}}',
    '<div style="font-family:sans-serif;padding:24px;"><h2>Exam Notice</h2><p>Exam {{ExamTitle}} for batch {{BatchName}} is scheduled on {{ExamDate}}.</p></div>',
    'Sent when a test or exam schedule is published',
    '{{InstituteName}},{{RecipientName}},{{ExamTitle}},{{BatchName}},{{ExamDate}}',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
),
(
    'baaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    NULL,
    4, -- Category: System Notices
    NULL, -- TargetRole: All Roles
    'GENERAL_ANNOUNCEMENT',
    'General Announcement',
    'Announcement from {{InstituteName}}: {{AnnouncementTitle}}',
    '<div style="font-family:sans-serif;padding:24px;"><h2>{{AnnouncementTitle}}</h2><p>{{AnnouncementBody}}</p></div>',
    'Broadcast circular or notice sent to teachers and students',
    '{{InstituteName}},{{RecipientName}},{{AnnouncementTitle}},{{AnnouncementBody}}',
    TRUE,
    CURRENT_TIMESTAMP,
    FALSE
)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;
