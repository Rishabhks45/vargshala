--
-- PostgreSQL database dump
--

-- \restrict 3WVzluOg1FpS3DphFDaqpWLCxQB2cIXMue0l2kdotJt1iDq5OFhd1CPBeTCpvDX  (commented out for pgAdmin/GUI SQL editor compatibility)

-- Dumped from database version 18.6 (2078fcb)
-- Dumped by pg_dump version 18.3

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public."Users" DROP CONSTRAINT IF EXISTS "FK_Users_Organizations_OrganizationId";
ALTER TABLE IF EXISTS ONLY public."UserBranchAccess" DROP CONSTRAINT IF EXISTS "FK_UserBranchAccess_Users_UserId";
ALTER TABLE IF EXISTS ONLY public."UserBranchAccess" DROP CONSTRAINT IF EXISTS "FK_UserBranchAccess_Branches_BranchId";
ALTER TABLE IF EXISTS ONLY public."Teachers" DROP CONSTRAINT IF EXISTS "FK_Teachers_Users";
ALTER TABLE IF EXISTS ONLY public."Subjects" DROP CONSTRAINT IF EXISTS "FK_Subjects_Organizations_OrganizationId";
ALTER TABLE IF EXISTS ONLY public."Students" DROP CONSTRAINT IF EXISTS "FK_Students_Users";
ALTER TABLE IF EXISTS ONLY public."EmailTemplates" DROP CONSTRAINT IF EXISTS "FK_EmailTemplates_Organizations_OrganizationId";
ALTER TABLE IF EXISTS ONLY public."Coupons" DROP CONSTRAINT IF EXISTS "FK_Coupons_Organizations";
ALTER TABLE IF EXISTS ONLY public."Classes" DROP CONSTRAINT IF EXISTS "FK_Classes_Branches_BranchId";
ALTER TABLE IF EXISTS ONLY public."ClassSessions" DROP CONSTRAINT IF EXISTS "FK_ClassSessions_Teachers_TeacherId";
ALTER TABLE IF EXISTS ONLY public."ClassSessions" DROP CONSTRAINT IF EXISTS "FK_ClassSessions_Batches_BatchId";
ALTER TABLE IF EXISTS ONLY public."ClassSessions" DROP CONSTRAINT IF EXISTS "FK_ClassSessions_BatchSchedules_BatchScheduleId";
ALTER TABLE IF EXISTS ONLY public."Branches" DROP CONSTRAINT IF EXISTS "FK_Branches_Organizations_OrganizationId";
ALTER TABLE IF EXISTS ONLY public."Batches" DROP CONSTRAINT IF EXISTS "FK_Batches_Subjects_SubjectId";
ALTER TABLE IF EXISTS ONLY public."Batches" DROP CONSTRAINT IF EXISTS "FK_Batches_Classes_ClassId";
ALTER TABLE IF EXISTS ONLY public."BatchTeachers" DROP CONSTRAINT IF EXISTS "FK_BatchTeachers_Teachers_TeacherId";
ALTER TABLE IF EXISTS ONLY public."BatchTeachers" DROP CONSTRAINT IF EXISTS "FK_BatchTeachers_Batches_BatchId";
ALTER TABLE IF EXISTS ONLY public."BatchStudents" DROP CONSTRAINT IF EXISTS "FK_BatchStudents_Students_StudentId";
ALTER TABLE IF EXISTS ONLY public."BatchStudents" DROP CONSTRAINT IF EXISTS "FK_BatchStudents_Batches_BatchId";
ALTER TABLE IF EXISTS ONLY public."BatchSchedules" DROP CONSTRAINT IF EXISTS "FK_BatchSchedules_Batches_BatchId";
ALTER TABLE IF EXISTS ONLY public."Attendances" DROP CONSTRAINT IF EXISTS "FK_Attendances_Students_StudentId";
ALTER TABLE IF EXISTS ONLY public."Attendances" DROP CONSTRAINT IF EXISTS "FK_Attendances_ClassSessions_ClassSessionId";
DROP INDEX IF EXISTS public."IX_Users_OrganizationId";
DROP INDEX IF EXISTS public."IX_Users_Email_OrganizationId";
DROP INDEX IF EXISTS public."IX_UserBranchAccess_UserId";
DROP INDEX IF EXISTS public."IX_UserBranchAccess_BranchId";
DROP INDEX IF EXISTS public."IX_Teachers_EmployeeCode";
DROP INDEX IF EXISTS public."IX_Teachers_Designation";
DROP INDEX IF EXISTS public."IX_Teachers_Department";
DROP INDEX IF EXISTS public."IX_Subjects_OrganizationId";
DROP INDEX IF EXISTS public."IX_Students_StudentCode";
DROP INDEX IF EXISTS public."IX_Students_ClassName_Section";
DROP INDEX IF EXISTS public."IX_Organizations_Code";
DROP INDEX IF EXISTS public."IX_EmailTemplates_TargetRole";
DROP INDEX IF EXISTS public."IX_EmailTemplates_OrganizationId";
DROP INDEX IF EXISTS public."IX_EmailTemplates_Org_Code";
DROP INDEX IF EXISTS public."IX_EmailTemplates_Code_OrganizationId";
DROP INDEX IF EXISTS public."IX_EmailTemplates_Category";
DROP INDEX IF EXISTS public."IX_Coupons_Organization_Active";
DROP INDEX IF EXISTS public."IX_Coupons_Org_Active";
DROP INDEX IF EXISTS public."IX_Coupons_ExpiryDate";
DROP INDEX IF EXISTS public."IX_Coupons_Code_NonDeleted";
DROP INDEX IF EXISTS public."IX_Coupons_Code";
DROP INDEX IF EXISTS public."IX_Classes_BranchId";
DROP INDEX IF EXISTS public."IX_ClassSessions_TeacherId";
DROP INDEX IF EXISTS public."IX_ClassSessions_SessionDate";
DROP INDEX IF EXISTS public."IX_ClassSessions_BatchScheduleId";
DROP INDEX IF EXISTS public."IX_ClassSessions_BatchId_SessionDate";
DROP INDEX IF EXISTS public."IX_ClassSessions_BatchId";
DROP INDEX IF EXISTS public."IX_Branches_OrganizationId";
DROP INDEX IF EXISTS public."IX_Branches_City";
DROP INDEX IF EXISTS public."IX_Batches_SubjectId";
DROP INDEX IF EXISTS public."IX_Batches_ClassId";
DROP INDEX IF EXISTS public."IX_BatchTeachers_TeacherId";
DROP INDEX IF EXISTS public."IX_BatchTeachers_BatchId";
DROP INDEX IF EXISTS public."IX_BatchStudents_StudentId";
DROP INDEX IF EXISTS public."IX_BatchStudents_BatchId";
DROP INDEX IF EXISTS public."IX_BatchSchedules_DayOfWeek";
DROP INDEX IF EXISTS public."IX_BatchSchedules_BatchId";
DROP INDEX IF EXISTS public."IX_Attendances_StudentId";
DROP INDEX IF EXISTS public."IX_Attendances_ClassSessionId";
ALTER TABLE IF EXISTS ONLY public."UserBranchAccess" DROP CONSTRAINT IF EXISTS "UQ_UserBranchAccess_UserId_BranchId";
ALTER TABLE IF EXISTS ONLY public."Teachers" DROP CONSTRAINT IF EXISTS "UQ_Teachers_UserId";
ALTER TABLE IF EXISTS ONLY public."Subjects" DROP CONSTRAINT IF EXISTS "UQ_Subjects_OrganizationId_Code";
ALTER TABLE IF EXISTS ONLY public."Students" DROP CONSTRAINT IF EXISTS "UQ_Students_UserId";
ALTER TABLE IF EXISTS ONLY public."Classes" DROP CONSTRAINT IF EXISTS "UQ_Classes_BranchId_Code";
ALTER TABLE IF EXISTS ONLY public."Branches" DROP CONSTRAINT IF EXISTS "UQ_Branches_OrganizationId_Code";
ALTER TABLE IF EXISTS ONLY public."Batches" DROP CONSTRAINT IF EXISTS "UQ_Batches_ClassId_Code";
ALTER TABLE IF EXISTS ONLY public."BatchTeachers" DROP CONSTRAINT IF EXISTS "UQ_BatchTeachers_BatchId_TeacherId";
ALTER TABLE IF EXISTS ONLY public."BatchStudents" DROP CONSTRAINT IF EXISTS "UQ_BatchStudents_BatchId_StudentId";
ALTER TABLE IF EXISTS ONLY public."Attendances" DROP CONSTRAINT IF EXISTS "UQ_Attendances_ClassSessionId_StudentId";
ALTER TABLE IF EXISTS ONLY public."Teachers" DROP CONSTRAINT IF EXISTS "Teachers_pkey";
ALTER TABLE IF EXISTS ONLY public."Students" DROP CONSTRAINT IF EXISTS "Students_pkey";
ALTER TABLE IF EXISTS ONLY public."__EFMigrationsHistory" DROP CONSTRAINT IF EXISTS "PK___EFMigrationsHistory";
ALTER TABLE IF EXISTS ONLY public."Users" DROP CONSTRAINT IF EXISTS "PK_Users";
ALTER TABLE IF EXISTS ONLY public."UserBranchAccess" DROP CONSTRAINT IF EXISTS "PK_UserBranchAccess";
ALTER TABLE IF EXISTS ONLY public."Subjects" DROP CONSTRAINT IF EXISTS "PK_Subjects";
ALTER TABLE IF EXISTS ONLY public."Organizations" DROP CONSTRAINT IF EXISTS "PK_Organizations";
ALTER TABLE IF EXISTS ONLY public."EmailTemplates" DROP CONSTRAINT IF EXISTS "PK_EmailTemplates";
ALTER TABLE IF EXISTS ONLY public."Coupons" DROP CONSTRAINT IF EXISTS "PK_Coupons";
ALTER TABLE IF EXISTS ONLY public."Classes" DROP CONSTRAINT IF EXISTS "PK_Classes";
ALTER TABLE IF EXISTS ONLY public."ClassSessions" DROP CONSTRAINT IF EXISTS "PK_ClassSessions";
ALTER TABLE IF EXISTS ONLY public."Branches" DROP CONSTRAINT IF EXISTS "PK_Branches";
ALTER TABLE IF EXISTS ONLY public."Batches" DROP CONSTRAINT IF EXISTS "PK_Batches";
ALTER TABLE IF EXISTS ONLY public."BatchTeachers" DROP CONSTRAINT IF EXISTS "PK_BatchTeachers";
ALTER TABLE IF EXISTS ONLY public."BatchStudents" DROP CONSTRAINT IF EXISTS "PK_BatchStudents";
ALTER TABLE IF EXISTS ONLY public."BatchSchedules" DROP CONSTRAINT IF EXISTS "PK_BatchSchedules";
ALTER TABLE IF EXISTS ONLY public."Attendances" DROP CONSTRAINT IF EXISTS "PK_Attendances";
DROP TABLE IF EXISTS public."__EFMigrationsHistory";
DROP TABLE IF EXISTS public."Users";
DROP TABLE IF EXISTS public."UserBranchAccess";
DROP TABLE IF EXISTS public."Teachers";
DROP TABLE IF EXISTS public."Subjects";
DROP TABLE IF EXISTS public."Students";
DROP TABLE IF EXISTS public."Organizations";
DROP TABLE IF EXISTS public."EmailTemplates";
DROP TABLE IF EXISTS public."Coupons";
DROP TABLE IF EXISTS public."Classes";
DROP TABLE IF EXISTS public."ClassSessions";
DROP TABLE IF EXISTS public."Branches";
DROP TABLE IF EXISTS public."Batches";
DROP TABLE IF EXISTS public."BatchTeachers";
DROP TABLE IF EXISTS public."BatchStudents";
DROP TABLE IF EXISTS public."BatchSchedules";
DROP TABLE IF EXISTS public."Attendances";
SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: Attendances; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Attendances" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ClassSessionId" uuid NOT NULL,
    "StudentId" uuid NOT NULL,
    "Status" character varying(20) DEFAULT 'Present'::character varying NOT NULL,
    "MarkedAt" timestamp with time zone,
    "Remarks" text,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone,
    CONSTRAINT "CK_Attendances_Status" CHECK ((("Status")::text = ANY ((ARRAY['Present'::character varying, 'Absent'::character varying, 'Late'::character varying, 'Excused'::character varying])::text[])))
);


--
-- Name: BatchSchedules; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."BatchSchedules" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "BatchId" uuid NOT NULL,
    "DayOfWeek" integer NOT NULL,
    "StartTime" time without time zone NOT NULL,
    "EndTime" time without time zone NOT NULL,
    "RoomOrLocation" character varying(150),
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone
);


--
-- Name: BatchStudents; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."BatchStudents" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "BatchId" uuid NOT NULL,
    "StudentId" uuid NOT NULL,
    "IsPrimary" boolean DEFAULT true NOT NULL,
    "EnrollmentType" character varying(50) DEFAULT 'Regular'::character varying NOT NULL,
    "JoinedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "LeftAt" timestamp with time zone,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: BatchTeachers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."BatchTeachers" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "BatchId" uuid NOT NULL,
    "TeacherId" uuid NOT NULL,
    "AssignedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "RemovedAt" timestamp with time zone,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: Batches; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Batches" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "ClassId" uuid NOT NULL,
    "SubjectId" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "Code" character varying(50) NOT NULL,
    "StartTime" time without time zone,
    "EndTime" time without time zone,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone
);


--
-- Name: Branches; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Branches" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "OrganizationId" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Code" character varying(50) NOT NULL,
    "LogoUrl" text,
    "Email" character varying(150),
    "Mobile" character varying(20),
    "AlternateMobile" character varying(20),
    "Address" character varying(500),
    "City" character varying(100),
    "State" character varying(100),
    "Pincode" character varying(10),
    "Country" character varying(100),
    "IsMainBranch" boolean DEFAULT false NOT NULL,
    "UseBranchName" boolean DEFAULT true NOT NULL,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone
);


--
-- Name: ClassSessions; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."ClassSessions" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "BatchId" uuid NOT NULL,
    "BatchScheduleId" uuid,
    "TeacherId" uuid,
    "SessionDate" date NOT NULL,
    "StartTime" time without time zone NOT NULL,
    "EndTime" time without time zone NOT NULL,
    "Topic" character varying(250),
    "Notes" text,
    "Status" character varying(30) DEFAULT 'Scheduled'::character varying NOT NULL,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone
);


--
-- Name: Classes; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Classes" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "BranchId" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "Code" character varying(50) NOT NULL,
    "Description" text,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone
);


--
-- Name: Coupons; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Coupons" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "OrganizationId" uuid,
    "Code" character varying(50) NOT NULL,
    "Category" integer DEFAULT 2 NOT NULL,
    "Description" character varying(500),
    "DiscountType" integer DEFAULT 1 NOT NULL,
    "DiscountValue" numeric(18,2) NOT NULL,
    "MinOrderAmount" numeric(18,2),
    "MaxDiscountAmount" numeric(18,2),
    "ApplicablePlan" integer DEFAULT 1 NOT NULL,
    "UsedCount" integer DEFAULT 0 NOT NULL,
    "MaxUses" integer DEFAULT 100 NOT NULL,
    "ExpiryDate" timestamp with time zone NOT NULL,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone
);


--
-- Name: EmailTemplates; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."EmailTemplates" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "OrganizationId" uuid,
    "Category" integer DEFAULT 1 NOT NULL,
    "TargetRole" integer,
    "Code" character varying(50) NOT NULL,
    "Name" character varying(150) NOT NULL,
    "Subject" character varying(250) NOT NULL,
    "AvailablePlaceholders" character varying(1000),
    "BodyHtml" text NOT NULL,
    "Description" character varying(500),
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone
);


--
-- Name: Organizations; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Organizations" (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Code" character varying(50) NOT NULL,
    "LogoUrl" text,
    "Email" character varying(150),
    "Mobile" character varying(20),
    "Address" character varying(500),
    "City" character varying(100),
    "State" character varying(100),
    "Pincode" character varying(10),
    "AcademicSession" character varying(20),
    "IsActive" boolean NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone
);


--
-- Name: Students; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Students" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "UserId" uuid NOT NULL,
    "DateOfBirth" date,
    "Gender" character varying(20),
    "BloodGroup" character varying(10),
    "Nationality" character varying(50),
    "StudentCode" character varying(50),
    "AdmissionDate" date,
    "ClassName" character varying(100),
    "Section" character varying(50),
    "RollNumber" character varying(50),
    "FatherName" character varying(150),
    "FatherMobile" character varying(20),
    "FatherAlternateMobile" character varying(20),
    "MotherName" character varying(150),
    "Address" text,
    "City" character varying(100),
    "State" character varying(100),
    "PostalCode" character varying(20),
    "Country" character varying(100),
    "EmergencyContactName" character varying(150),
    "EmergencyContactMobile" character varying(20),
    "EmergencyContactRelation" character varying(50),
    "AadharNumber" character varying(20),
    "PreviousInstitute" character varying(200),
    "MedicalNotes" text,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone
);


--
-- Name: Subjects; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Subjects" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "OrganizationId" uuid NOT NULL,
    "Name" character varying(150) NOT NULL,
    "Code" character varying(50) NOT NULL,
    "Description" text,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone
);


--
-- Name: Teachers; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Teachers" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "UserId" uuid NOT NULL,
    "EmployeeCode" character varying(50),
    "JoiningDate" date,
    "Department" character varying(100),
    "Designation" integer,
    "HighestQualification" integer,
    "Specialization" character varying(150),
    "TeachingExperienceYears" numeric(5,2),
    "Address" text,
    "City" character varying(100),
    "State" character varying(100),
    "PostalCode" character varying(20),
    "Country" character varying(100),
    "AadharNumber" character varying(20),
    "PreviousInstitute" character varying(200),
    "Bio" text,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone
);


--
-- Name: UserBranchAccess; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."UserBranchAccess" (
    "Id" uuid DEFAULT gen_random_uuid() NOT NULL,
    "UserId" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "IsActive" boolean DEFAULT true NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone DEFAULT now() NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone
);


--
-- Name: Users; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."Users" (
    "Id" uuid NOT NULL,
    "OrganizationId" uuid,
    "FirstName" character varying(100) NOT NULL,
    "LastName" character varying(100) NOT NULL,
    "Email" character varying(150),
    "Mobile" character varying(20),
    "PasswordHash" character varying(500) NOT NULL,
    "Role" integer NOT NULL,
    "EmailVerified" boolean NOT NULL,
    "MobileVerified" boolean NOT NULL,
    "LastLoginAt" timestamp with time zone,
    "RefreshToken" character varying(500),
    "RefreshTokenExpiryTime" timestamp with time zone,
    "IsActive" boolean NOT NULL,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone,
    "ProfilePictureUrl" character varying,
    "PasswordResetToken" character varying(500),
    "PasswordResetTokenExpiresAt" timestamp with time zone
);


--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


--
-- Data for Name: Attendances; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('346362bc-5628-445d-b96c-bcf9a2cba2dd', '01a08a74-7067-7cf3-b74b-fed9658ff793', '0f0106f3-6179-40e2-a67a-359899e47f77', 'Present', '2026-09-10 15:30:02.556014+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:26:59.358728+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:02.556014+00', false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('48f95576-b850-4438-970d-287874fc0a8e', '01a08a74-7067-7cf3-b74b-fed9658ff793', '448ab6d1-c5ea-43f2-8582-6f0352a58431', 'Present', '2026-09-10 15:30:02.556014+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:26:59.358728+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:02.556014+00', false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('67715ae2-b13a-45de-833b-9aa96939cf8a', '01a08a74-7067-7cf3-b74b-fed9658ff793', 'd1fa6c6d-82c9-4374-a56f-12d35e80fe87', 'Present', '2026-09-10 15:30:02.556014+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:26:59.358728+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:02.556014+00', false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('69ebd4d3-f6fe-43fe-bb94-a8c8a72133e1', '01a08a74-7067-7cf3-b74b-fed9658ff793', 'd2591ec4-f01b-4b92-9581-995ac1a37468', 'Present', '2026-09-10 15:30:02.556014+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:26:59.358728+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:02.556014+00', false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('6e318ca4-a7cc-4535-9f6e-0b09626d9e46', '01a08a74-7067-7cf3-b74b-fed9658ff793', '850bb4bc-c64d-4077-81ff-6a82ce1de9c8', 'Present', '2026-09-10 15:30:02.556014+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:26:59.358728+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:02.556014+00', false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('a7012caf-055e-4768-b3f0-4da5b350b929', '01a08a74-7067-7cf3-b74b-fed9658ff793', 'ed50949e-b0c1-4acb-a2ff-6d5ccdf46790', 'Absent', '2026-09-10 15:30:02.556014+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:26:59.358728+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:02.556014+00', false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('a9020947-8c9d-4d00-b057-430371ce1418', '01a08a74-7067-7cf3-b74b-fed9658ff793', 'b51dee7e-7215-4585-84e1-24c56f53c81c', 'Absent', '2026-09-10 15:30:02.556014+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:26:59.358728+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:02.556014+00', false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('aa281937-7be8-498b-ad9c-b9097e0b06f0', '01a08a74-7067-7cf3-b74b-fed9658ff793', '0ff5b3a5-a6fe-4daf-8cef-b0af1f09b437', 'Present', '2026-09-10 15:30:02.556014+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:26:59.358728+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:02.556014+00', false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('b368bc3e-344b-4561-bd86-3d857c7ba73c', '01a08a74-7067-7cf3-b74b-fed9658ff793', '0499efe6-9fcf-4192-be8f-6e963adbcdb9', 'Present', '2026-09-10 15:30:02.556014+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:26:59.358728+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:02.556014+00', false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('f43ee19d-ec02-4443-bcf3-390476e0c17e', '01a08a74-7067-7cf3-b74b-fed9658ff793', 'b253703a-04b3-4098-bc0e-43def74c37d7', 'Present', '2026-09-10 15:30:02.556014+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:26:59.358728+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:02.556014+00', false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('e8fe1d1d-0a31-42f6-b34f-040ebe81257d', '01a08a74-7067-7cf3-b74b-fed9658ff793', 'bc578d5e-cb13-4d5d-a1c5-1365cb0398f1', 'Absent', '2026-09-10 15:30:02.556014+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:02.556014+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('07330809-4289-4c14-a0aa-49c8401b1ddf', '01a08aa1-ee09-7cca-9a26-cbaa57a03357', '0f0106f3-6179-40e2-a67a-359899e47f77', 'Present', '2026-09-10 15:30:53.652133+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:53.652133+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('3d36fb6c-7b85-4679-9e38-549c5fcbf038', '01a08aa1-ee09-7cca-9a26-cbaa57a03357', 'd2591ec4-f01b-4b92-9581-995ac1a37468', 'Present', '2026-09-10 15:30:53.652133+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:53.652133+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('6ce6b601-aa4a-482c-bcc7-3344290c60fa', '01a08aa1-ee09-7cca-9a26-cbaa57a03357', 'ed50949e-b0c1-4acb-a2ff-6d5ccdf46790', 'Present', '2026-09-10 15:30:53.652133+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:53.652133+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('9b1f092a-d27a-4472-a9ea-896613115a11', '01a08aa1-ee09-7cca-9a26-cbaa57a03357', '0ff5b3a5-a6fe-4daf-8cef-b0af1f09b437', 'Present', '2026-09-10 15:30:53.652133+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:53.652133+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('9bc94fc3-b646-4926-b5e1-db1ac24c3447', '01a08aa1-ee09-7cca-9a26-cbaa57a03357', '850bb4bc-c64d-4077-81ff-6a82ce1de9c8', 'Present', '2026-09-10 15:30:53.652133+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:53.652133+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('a901dadd-df79-4d5a-9447-1ceb5973936f', '01a08aa1-ee09-7cca-9a26-cbaa57a03357', '448ab6d1-c5ea-43f2-8582-6f0352a58431', 'Present', '2026-09-10 15:30:53.652133+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:53.652133+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('b1d103ec-8fda-47c6-9686-d39a6a4b6d0f', '01a08aa1-ee09-7cca-9a26-cbaa57a03357', 'd1fa6c6d-82c9-4374-a56f-12d35e80fe87', 'Present', '2026-09-10 15:30:53.652133+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:53.652133+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('b7ec3fe9-a9bb-4973-8dc3-d11a0b21ea2b', '01a08aa1-ee09-7cca-9a26-cbaa57a03357', 'bc578d5e-cb13-4d5d-a1c5-1365cb0398f1', 'Present', '2026-09-10 15:30:53.652133+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:53.652133+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('c9275a61-00a5-4ea8-b441-389df486844b', '01a08aa1-ee09-7cca-9a26-cbaa57a03357', 'b253703a-04b3-4098-bc0e-43def74c37d7', 'Present', '2026-09-10 15:30:53.652133+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:53.652133+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('d0d2f7dc-56f9-45ea-9f9f-e4ee80a77743', '01a08aa1-ee09-7cca-9a26-cbaa57a03357', '0499efe6-9fcf-4192-be8f-6e963adbcdb9', 'Present', '2026-09-10 15:30:53.652133+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:53.652133+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Attendances" ("Id", "ClassSessionId", "StudentId", "Status", "MarkedAt", "Remarks", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('d89f3048-ba54-43a0-8e6e-08a8f525b4dc', '01a08aa1-ee09-7cca-9a26-cbaa57a03357', 'b51dee7e-7215-4585-84e1-24c56f53c81c', 'Present', '2026-09-10 15:30:53.652133+00', NULL, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:30:53.652133+00', NULL, NULL, false, NULL, NULL);


--
-- Data for Name: BatchSchedules; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: BatchStudents; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('d6f2b666-4b63-42cb-8f0d-7e2d819d7be7', 'c4506cd4-3166-45e7-9391-e3c44199dd81', '448ab6d1-c5ea-43f2-8582-6f0352a58431', true, 'Regular', '2026-09-10 14:36:18.510301+00', NULL, true, NULL, '2026-09-10 14:36:18.510301+00', NULL, NULL);
INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('dcb83701-dd51-4801-b22e-ddda12667e60', '2401b858-d1b5-4e08-b80e-4fb3a32b6b80', '850bb4bc-c64d-4077-81ff-6a82ce1de9c8', true, 'Regular', '2026-04-09 00:00:00+00', '2026-09-10 15:04:35.523515+00', false, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:04:24.436+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:04:35.523991+00');
INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('2b76d4b9-ff83-44e9-bead-d6119634c03f', 'c4506cd4-3166-45e7-9391-e3c44199dd81', '850bb4bc-c64d-4077-81ff-6a82ce1de9c8', true, 'Regular', '2026-04-09 00:00:00+00', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:04:53.32846+00', NULL, NULL);
INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('c11319f1-d291-448d-a624-ac40ecf0fd1d', 'c4506cd4-3166-45e7-9391-e3c44199dd81', '0ff5b3a5-a6fe-4daf-8cef-b0af1f09b437', true, 'Regular', '2026-04-02 00:00:00+00', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:05:15.277425+00', NULL, NULL);
INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('4369b6e2-fb52-491c-bcc4-8f75f08f2fee', 'c4506cd4-3166-45e7-9391-e3c44199dd81', 'b51dee7e-7215-4585-84e1-24c56f53c81c', true, 'Regular', '2026-04-03 00:00:00+00', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:05:34.816507+00', NULL, NULL);
INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('d21e0e42-d3e7-4812-8345-96e6dfbbe3c9', 'c4506cd4-3166-45e7-9391-e3c44199dd81', 'b253703a-04b3-4098-bc0e-43def74c37d7', true, 'Regular', '2026-04-04 00:00:00+00', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:05:55.903342+00', NULL, NULL);
INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('644b8142-f9ae-4673-9b30-bac1014c1f8e', 'c4506cd4-3166-45e7-9391-e3c44199dd81', 'd1fa6c6d-82c9-4374-a56f-12d35e80fe87', true, 'Regular', '2026-04-05 00:00:00+00', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:06:14.032505+00', NULL, NULL);
INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('de8c108b-70d1-41f0-8c64-b2220bb454e0', 'c4506cd4-3166-45e7-9391-e3c44199dd81', '0499efe6-9fcf-4192-be8f-6e963adbcdb9', true, 'Regular', '2026-04-10 00:00:00+00', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:06:33.708933+00', NULL, NULL);
INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('f42ce0a0-4ed3-4a8b-bb11-c3b4698e8d1e', 'c4506cd4-3166-45e7-9391-e3c44199dd81', '0f0106f3-6179-40e2-a67a-359899e47f77', true, 'Regular', '2026-04-08 00:00:00+00', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:07:32.904144+00', NULL, NULL);
INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('5d429441-e55a-467f-b1cd-b7eecedcdebe', 'c4506cd4-3166-45e7-9391-e3c44199dd81', 'd2591ec4-f01b-4b92-9581-995ac1a37468', true, 'Regular', '2026-04-07 00:00:00+00', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:07:49.713817+00', NULL, NULL);
INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('7e44cea1-9d3e-4808-96a8-f526de7e6b36', 'c4506cd4-3166-45e7-9391-e3c44199dd81', 'ed50949e-b0c1-4acb-a2ff-6d5ccdf46790', true, 'Regular', '2026-09-05 00:00:00+00', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:08:17.142432+00', NULL, NULL);
INSERT INTO public."BatchStudents" ("Id", "BatchId", "StudentId", "IsPrimary", "EnrollmentType", "JoinedAt", "LeftAt", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('f5296d75-7cdb-43c1-8118-085910de893f', 'c4506cd4-3166-45e7-9391-e3c44199dd81', 'bc578d5e-cb13-4d5d-a1c5-1365cb0398f1', true, 'Regular', '2026-04-06 00:00:00+00', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:29:05.118072+00', NULL, NULL);


--
-- Data for Name: BatchTeachers; Type: TABLE DATA; Schema: public; Owner: -
--



--
-- Data for Name: Batches; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Batches" ("Id", "ClassId", "SubjectId", "Name", "Code", "StartTime", "EndTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('c4506cd4-3166-45e7-9391-e3c44199dd81', 'b972e0f9-3987-4664-9ab7-aaff3a24ae95', 'd20eeaba-c950-4168-a72f-b1291fd03b74', 'Physics', 'PHYSICS_1', '06:20:00', '07:20:00', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 07:51:06.803676+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 14:27:21.067825+00', false, NULL, NULL);
INSERT INTO public."Batches" ("Id", "ClassId", "SubjectId", "Name", "Code", "StartTime", "EndTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('2401b858-d1b5-4e08-b80e-4fb3a32b6b80', 'b972e0f9-3987-4664-9ab7-aaff3a24ae95', '058ca8c3-6079-4d5f-8f92-6b8c421bcb1b', 'Math', 'MATH-45', '20:20:00', '21:20:00', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:04:03.925187+00', NULL, NULL, false, NULL, NULL);


--
-- Data for Name: Branches; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Branches" ("Id", "OrganizationId", "Name", "Code", "LogoUrl", "Email", "Mobile", "AlternateMobile", "Address", "City", "State", "Pincode", "Country", "IsMainBranch", "UseBranchName", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('9a74dd2e-8182-48c2-8370-e71159e4469e', '9d66519f-a058-4ada-80c5-fe836aeba6dd', 'Main Branch', 'MAIN', 'https://images.template.net/547758/Avatar-Profile-Picture-Template-edit-online.webp', 'brightacademy2026@yopmail.com', '8789352863', NULL, 'Main Road', 'Chapra', 'Bihar', '841208', 'India', true, true, true, NULL, '2026-09-05 19:56:22.505979+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Branches" ("Id", "OrganizationId", "Name", "Code", "LogoUrl", "Email", "Mobile", "AlternateMobile", "Address", "City", "State", "Pincode", "Country", "IsMainBranch", "UseBranchName", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('c750a02d-d09c-4324-80bd-182ee0d6c8aa', '9d66519f-a058-4ada-80c5-fe836aeba6dd', 'North Campus', 'BA-NC02', NULL, 'northcampus@brightacademy.com', '9876543230', NULL, 'College Road, North Campus', 'Chapra', 'Bihar', '841208', 'India', false, true, true, NULL, '2026-09-05 20:18:56.917549+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Branches" ("Id", "OrganizationId", "Name", "Code", "LogoUrl", "Email", "Mobile", "AlternateMobile", "Address", "City", "State", "Pincode", "Country", "IsMainBranch", "UseBranchName", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('2152c664-eefe-4f4c-baea-b8e430bc3b2f', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Main Branch', 'MAIN', NULL, 'bcc@yopmail.com', '8789352863', NULL, 'Village-Rith, Police Statiion-Ekma', 'Chapra', 'Bihar', '841208', 'India', true, true, true, NULL, '2026-09-05 19:56:22.46218+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 20:34:09.209269+00', false, NULL, NULL);
INSERT INTO public."Branches" ("Id", "OrganizationId", "Name", "Code", "LogoUrl", "Email", "Mobile", "AlternateMobile", "Address", "City", "State", "Pincode", "Country", "IsMainBranch", "UseBranchName", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('8adb1543-6094-45bd-8f1b-3b4ab1732919', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Town Hall Campus', 'BCC-TH02', NULL, 'townhall@bcc.com', '9876543220', NULL, 'Station Road, Near Town Hall', 'Chapra', 'Bihar', '841301', 'India', false, true, true, NULL, '2026-09-05 20:17:15.219592+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 20:59:41.765929+00', false, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 20:28:04.323906+00');


--
-- Data for Name: ClassSessions; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a74-7067-7cf3-b74b-fed9658ff793', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-10', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:34:41.903194+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a74-7068-7487-bd14-c427193c6e3a', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-14', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:34:41.903201+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a74-7068-7e71-a719-addebf04a424', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-11', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:34:41.9032+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a74-7069-7974-8ada-83793af4b915', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-16', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:34:41.903202+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a74-7069-7a22-8a61-0f9a5ce2b0e5', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-17', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:34:41.903202+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a74-7069-7b0b-9c55-d498d72ef8c0', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-15', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:34:41.903201+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a74-706a-7471-91a6-d395e31993ea', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-18', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:34:41.903203+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a74-706b-78f2-b109-a9e9b96e26c2', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-23', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:34:41.903206+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a74-706b-7929-8ace-bc499ca2e612', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-21', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:34:41.903203+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a74-706b-7b8a-bc20-f7bcb80b7483', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-22', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:34:41.903204+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a74-706c-78ff-b9fa-5750a757f20f', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-24', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:34:41.903213+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08aa1-ee09-7cca-9a26-cbaa57a03357', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-10', '08:00:00', '09:00:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 09:24:23.607061+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."ClassSessions" ("Id", "BatchId", "BatchScheduleId", "TeacherId", "SessionDate", "StartTime", "EndTime", "Topic", "Notes", "Status", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('01a08a86-a68b-70d0-839d-5d4595847ed8', 'c4506cd4-3166-45e7-9391-e3c44199dd81', NULL, 'e47f2cd9-2d37-4876-a940-ec83544de738', '2026-09-24', '06:20:00', '07:20:00', 'Physics Class', NULL, 'Scheduled', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:54:35.404298+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 09:24:53.701749+00', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 09:24:53.698507+00');


--
-- Data for Name: Classes; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Classes" ("Id", "BranchId", "Name", "Code", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('b972e0f9-3987-4664-9ab7-aaff3a24ae95', '2152c664-eefe-4f4c-baea-b8e430bc3b2f', 'Class 12th', 'CLASS-12TH', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 07:49:37.56946+00', NULL, NULL, false, NULL, NULL);


--
-- Data for Name: Coupons; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Coupons" ("Id", "OrganizationId", "Code", "Category", "Description", "DiscountType", "DiscountValue", "MinOrderAmount", "MaxDiscountAmount", "ApplicablePlan", "UsedCount", "MaxUses", "ExpiryDate", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('0f44753b-f22e-46f2-a23f-e881d2e7db98', NULL, 'PROANNUAL', 4, 'Flat â‚¹1,000 off on Pro Institute annual subscription', 2, 1000.00, 2499.00, NULL, 3, 36, 50, '2026-11-15 23:59:59+00', true, NULL, '2026-09-05 10:01:33.400749+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Coupons" ("Id", "OrganizationId", "Code", "Category", "Description", "DiscountType", "DiscountValue", "MinOrderAmount", "MaxDiscountAmount", "ApplicablePlan", "UsedCount", "MaxUses", "ExpiryDate", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('26db1788-ab54-4d0f-9733-72066d8eab99', NULL, 'ENTERPRISEVIP', 4, 'Special VIP discount for multi-branch enterprise networks', 2, 2000.00, 5999.00, NULL, 4, 12, 20, '2026-12-31 23:59:59+00', true, NULL, '2026-09-05 10:01:33.400749+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Coupons" ("Id", "OrganizationId", "Code", "Category", "Description", "DiscountType", "DiscountValue", "MinOrderAmount", "MaxDiscountAmount", "ApplicablePlan", "UsedCount", "MaxUses", "ExpiryDate", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('b0e38247-74b0-4b30-9029-3091250f256d', NULL, 'FESTIVE25', 3, 'Special festive discount on any annual billing tier', 1, 25.00, 1999.00, 1000.00, 1, 19, 75, '2026-09-20 23:59:59+00', true, NULL, '2026-09-05 10:01:33.400749+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Coupons" ("Id", "OrganizationId", "Code", "Category", "Description", "DiscountType", "DiscountValue", "MinOrderAmount", "MaxDiscountAmount", "ApplicablePlan", "UsedCount", "MaxUses", "ExpiryDate", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('539cb35a-b8a6-4113-8961-6fe4a0b5e417', NULL, 'WELCOME50', 1, 'Flat 50% discount for first-time coaching institute registrations', 1, 50.00, NULL, 1500.00, 1, 84, 100, '2026-12-31 23:59:59+00', true, NULL, '2026-09-05 10:01:33.400749+00', NULL, '2026-09-05 10:21:33.083733+00', false, NULL, NULL);
INSERT INTO public."Coupons" ("Id", "OrganizationId", "Code", "Category", "Description", "DiscountType", "DiscountValue", "MinOrderAmount", "MaxDiscountAmount", "ApplicablePlan", "UsedCount", "MaxUses", "ExpiryDate", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('e8d786b0-5fb5-4fdb-92c2-8cce76299d4e', NULL, 'STARTUP20', 2, '20% off for newly established coaching institutes (<50 students)', 1, 20.00, NULL, 500.00, 2, 28, 50, '2026-10-30 23:59:59+00', true, NULL, '2026-09-05 10:01:33.400749+00', NULL, '2026-09-05 10:21:43.139146+00', false, NULL, NULL);
INSERT INTO public."Coupons" ("Id", "OrganizationId", "Code", "Category", "Description", "DiscountType", "DiscountValue", "MinOrderAmount", "MaxDiscountAmount", "ApplicablePlan", "UsedCount", "MaxUses", "ExpiryDate", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('abb5d7fe-e1f0-4a62-8990-f3fd6ef71e32', NULL, 'RIMISHBH', 2, '', 2, 2099.00, 5000.00, 100000.00, 1, 0, 84, '2026-10-06 10:07:58.369949+00', true, NULL, '2026-09-06 10:08:43.815355+00', NULL, NULL, false, NULL, NULL);


--
-- Data for Name: EmailTemplates; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."EmailTemplates" ("Id", "OrganizationId", "Category", "TargetRole", "Code", "Name", "Subject", "AvailablePlaceholders", "BodyHtml", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('5f4e439c-5bc6-416f-ac11-1734598591f8', NULL, 1, NULL, 'WELCOME_ONBOARD', 'Welcome & Onboarding', 'Welcome to {{InstituteName}}!', '{{InstituteName}},{{RecipientName}},{{LoginUrl}},{{SupportEmail}}', '<div style="font-family:sans-serif;padding:24px;"><h2>Welcome {{RecipientName}}!</h2><p>Your account at {{InstituteName}} is ready. <a href="{{LoginUrl}}">Click here to Login</a></p></div>', 'Triggered automatically when a new student or teacher account is created', true, NULL, '2026-09-04 22:01:24.566496+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."EmailTemplates" ("Id", "OrganizationId", "Category", "TargetRole", "Code", "Name", "Subject", "AvailablePlaceholders", "BodyHtml", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('336ebe4c-c505-4fa3-96e9-be22831d2b72', NULL, 2, NULL, 'VERIFICATION_OTP', 'Verification OTP', 'Your Verification Code - {{InstituteName}}', '{{InstituteName}},{{RecipientName}},{{OtpCode}}', '<div style="font-family:sans-serif;padding:24px;"><h2>Verify Your Account</h2><p>Your verification code is <strong>{{OtpCode}}</strong>. Valid for 10 minutes.</p></div>', 'Sent to verify user email address during onboarding', true, NULL, '2026-09-04 22:01:24.566496+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."EmailTemplates" ("Id", "OrganizationId", "Category", "TargetRole", "Code", "Name", "Subject", "AvailablePlaceholders", "BodyHtml", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('129e0c5a-2177-455f-9b1c-e40c098b5fbd', NULL, 3, 3, 'FEE_RECEIPT', 'Fee Payment Receipt', 'Payment Receipt #{{InvoiceNumber}} - {{InstituteName}}', '{{InstituteName}},{{RecipientName}},{{InvoiceNumber}},{{AmountPaid}}', '<div style="font-family:sans-serif;padding:24px;"><h2>Fee Payment Successful</h2><p>Dear {{RecipientName}}, we received payment of {{AmountPaid}} for invoice {{InvoiceNumber}}.</p></div>', 'Sent to parents/students upon fee payment confirmation', true, NULL, '2026-09-04 22:01:24.566496+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."EmailTemplates" ("Id", "OrganizationId", "Category", "TargetRole", "Code", "Name", "Subject", "AvailablePlaceholders", "BodyHtml", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('af05f4de-190d-44bb-a66c-4192646718ea', NULL, 3, 3, 'FEE_DUE_REMINDER', 'Fee Due Reminder', 'Fee Payment Reminder - {{InstituteName}}', '{{InstituteName}},{{RecipientName}},{{AmountDue}},{{DueDate}}', '<div style="font-family:sans-serif;padding:24px;"><h2>Payment Reminder</h2><p>Dear {{RecipientName}}, an installment of {{AmountDue}} is due on {{DueDate}}.</p></div>', 'Sent before or on the due date for upcoming fee installments', true, NULL, '2026-09-04 22:01:24.566496+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."EmailTemplates" ("Id", "OrganizationId", "Category", "TargetRole", "Code", "Name", "Subject", "AvailablePlaceholders", "BodyHtml", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('45c09b97-183a-4aa5-928e-7cf168fe5bf5', NULL, 1, 3, 'ADMISSION_CONFIRMATION', 'Admission Confirmation', 'Admission Confirmed - {{BatchName}} | {{InstituteName}}', '{{InstituteName}},{{RecipientName}},{{BatchName}},{{RollNumber}}', '<div style="font-family:sans-serif;padding:24px;"><h2>Congratulations {{RecipientName}}!</h2><p>Your admission for {{BatchName}} has been confirmed. Roll No: {{RollNumber}}.</p></div>', 'Sent to student & parents when admission is processed', true, NULL, '2026-09-04 22:01:24.566496+00', NULL, '2026-09-04 22:13:07.356162+00', false, NULL, NULL);
INSERT INTO public."EmailTemplates" ("Id", "OrganizationId", "Category", "TargetRole", "Code", "Name", "Subject", "AvailablePlaceholders", "BodyHtml", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('1c34523e-97e8-4cd1-b925-820eb32540b6', NULL, 4, 3, 'ATTENDANCE_ALERT', 'Attendance Alert', 'Attendance Alert: {{RecipientName}} is Absent today', '{{InstituteName}},{{RecipientName}},{{BatchName}},{{Date}}', '<div style="font-family:sans-serif;padding:24px;"><h2>Attendance Notice</h2><p>Dear Parent, {{RecipientName}} was marked absent for {{BatchName}} on {{Date}}.</p></div>', 'Triggered when student is marked absent in daily attendance', true, NULL, '2026-09-04 22:01:24.566496+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."EmailTemplates" ("Id", "OrganizationId", "Category", "TargetRole", "Code", "Name", "Subject", "AvailablePlaceholders", "BodyHtml", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('1cac1c3f-2d59-4f90-a63e-7b7023abf9f6', NULL, 4, 3, 'EXAM_NOTICE', 'Exam & Quiz Notice', 'Upcoming Exam Schedule - {{InstituteName}}', '{{InstituteName}},{{RecipientName}},{{ExamTitle}},{{BatchName}},{{ExamDate}}', '<div style="font-family:sans-serif;padding:24px;"><h2>Exam Notice</h2><p>Exam {{ExamTitle}} for batch {{BatchName}} is scheduled on {{ExamDate}}.</p></div>', 'Sent when a test or exam schedule is published', true, NULL, '2026-09-04 22:01:24.566496+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."EmailTemplates" ("Id", "OrganizationId", "Category", "TargetRole", "Code", "Name", "Subject", "AvailablePlaceholders", "BodyHtml", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('1498d1c2-44bf-44f2-91aa-abfb89840735', NULL, 4, NULL, 'GENERAL_ANNOUNCEMENT', 'General Announcement', 'Announcement from {{InstituteName}}: {{AnnouncementTitle}}', '{{InstituteName}},{{RecipientName}},{{AnnouncementTitle}},{{AnnouncementBody}}', '<div style="font-family:sans-serif;padding:24px;"><h2>{{AnnouncementTitle}}</h2><p>{{AnnouncementBody}}</p></div>', 'Broadcast circular or notice sent to teachers and students', true, NULL, '2026-09-04 22:01:24.566496+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."EmailTemplates" ("Id", "OrganizationId", "Category", "TargetRole", "Code", "Name", "Subject", "AvailablePlaceholders", "BodyHtml", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('90a8fbc1-a0af-4564-8126-e64d7703be1e', NULL, 2, NULL, 'PASSWORD_RESET', 'Password Reset Link', 'Reset your password for {{InstituteName}}', '{{InstituteName}},{{RecipientName}},{{ResetUrl}},{{CurrentYear}},{{OtpCode}}', '<div style="background:#ffffff;border-radius:16px;overflow:hidden;border:1px solid #e5e7eb;">

  <div style="padding:32px 36px 24px;text-align:center;">
    <div style="font-size:28px;font-weight:700;color:#111827;margin-bottom:8px;">
      Password Reset
    </div>
    <div style="font-size:15px;line-height:24px;color:#6b7280;">
      Click the button below to set a new password and regain access to your account.
    </div>
  </div>

  <div style="padding:8px 36px 32px;text-align:center;">

```
<a href="{{ResetUrl}}" style="display:inline-block;background:#111827;color:#ffffff;text-decoration:none;font-size:15px;font-weight:600;border-radius:10px;padding:14px 28px;margin:12px 0 20px;">
  Reset Password
</a>

<p style="font-size:14px;line-height:22px;color:#6b7280;margin:4px 0 0;">
  This link is provided to help you securely update your password.
</p>

<div style="height:1px;background:#e5e7eb;margin:28px 0 20px;"></div>

<p style="font-size:12px;line-height:20px;color:#9ca3af;margin:0;">
  If you didn''t request a password reset, you can safely ignore this email.
</p>
```

  </div>
</div>

<div style="text-align:center;padding:20px 10px;font-size:12px;color:#9ca3af;">
  Â© {{CurrentYear}} Â· All rights reserved
</div>
', 'Sent when user requests a password reset link', true, NULL, '2026-09-04 22:01:24.566496+00', NULL, '2026-09-05 09:32:28.753435+00', false, NULL, NULL);
INSERT INTO public."EmailTemplates" ("Id", "OrganizationId", "Category", "TargetRole", "Code", "Name", "Subject", "AvailablePlaceholders", "BodyHtml", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('74a5bf1b-1124-4086-bef8-09a28c126f9a', NULL, 2, NULL, 'FORGOT_PASSWORD', 'Forgot Password', 'OTP for Password Reset - {{InstituteName}}', '{{InstituteName}},{{RecipientName}},{{ResetUrl}},{{CurrentYear}},{{OtpCode}}', '<div style="margin:0;padding:0;background:#f4f6f8;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
  <div style="max-width:560px;margin:40px auto;padding:0 20px;">

    <div style="background:#ffffff;border-radius:16px;overflow:hidden;border:1px solid #e5e7eb;box-shadow:0 4px 14px rgba(0,0,0,0.04);">
      
      <!-- Header -->
      <div style="padding:32px 36px 20px;text-align:center;">
        <div style="font-size:26px;font-weight:700;color:#111827;margin-bottom:8px;">
          Reset Your Password
        </div>
        <div style="font-size:15px;line-height:24px;color:#6b7280;">
          Hello <strong style="color:#111827;">{{RecipientName}}</strong>, click the button below to securely set a new password for your <strong style="color:#111827;">{{InstituteName}}</strong> account.
        </div>
      </div>

      <!-- Action Button -->
      <div style="padding:10px 36px 32px;text-align:center;">
        
        <div style="margin:18px 0 24px;">
          <a href="{{ResetUrl}}" 
             target="_blank" 
             style="display:inline-block;background:#004D40;color:#ffffff;text-decoration:none;font-size:15px;font-weight:600;border-radius:12px;padding:14px 34px;box-shadow:0 4px 14px rgba(0,77,64,0.25);">
            Reset Password &rarr;
          </a>
        </div>

        <p style="font-size:13px;line-height:20px;color:#6b7280;margin:0 0 16px;">
          â±ï¸ This link is valid for <strong style="color:#374151;">30 minutes</strong> and can only be used <strong style="color:#374151;">one time</strong>.
        </p>

        <div style="height:1px;background:#e5e7eb;margin:24px 0 20px;"></div>

        <p style="font-size:12px;line-height:18px;color:#9ca3af;margin:0 0 6px;">
          If the button above does not work, copy and paste this link into your browser:
        </p>
        <p style="font-size:11px;line-height:16px;word-break:break-all;color:#009488;margin:0 0 20px;font-family:monospace;">
          {{ResetUrl}}
        </p>

        <p style="font-size:12px;line-height:20px;color:#9ca3af;margin:0;">
          If you didn''t request a password reset, you can safely ignore this email. Your password will remain unchanged.
        </p>

      </div>
    </div>

    <!-- Footer -->
    <div style="text-align:center;padding:20px 10px;font-size:12px;color:#9ca3af;">
      Â© {{CurrentYear}} {{InstituteName}} Â· All rights reserved
    </div>

  </div>
</div>
', 'Sent when user requests password recovery via OTP', true, NULL, '2026-09-04 22:01:24.566496+00', NULL, '2026-09-08 14:24:29.818088+00', false, NULL, NULL);


--
-- Data for Name: Organizations; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Organizations" ("Id", "Name", "Code", "LogoUrl", "Email", "Mobile", "Address", "City", "State", "Pincode", "AcademicSession", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Brillent Coaching Center', 'BCC', 'https://images.template.net/547758/Avatar-Profile-Picture-Template-edit-online.webp', 'bcc@yopmail.com', '8789352863', 'Village-Rith, Police Statiion-Ekma', 'Chapra', 'Bihar', '841208', '2026-2027', true, NULL, '2026-09-01 19:49:38.873993+00', NULL, '2026-09-05 12:05:21.592174+00', false, NULL, NULL);
INSERT INTO public."Organizations" ("Id", "Name", "Code", "LogoUrl", "Email", "Mobile", "Address", "City", "State", "Pincode", "AcademicSession", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('9d66519f-a058-4ada-80c5-fe836aeba6dd', 'Bright Academy', 'BA', 'https://images.template.net/547758/Avatar-Profile-Picture-Template-edit-online.webp', 'brightacademy2026@yopmail.com', '8789352863', 'Main Road', 'Chapra', 'Bihar', '841208', '2026-2027', true, NULL, '2026-09-05 16:54:42.20483+00', NULL, '2026-09-05 17:03:36.582221+00', false, NULL, NULL);


--
-- Data for Name: Students; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Students" ("Id", "UserId", "DateOfBirth", "Gender", "BloodGroup", "Nationality", "StudentCode", "AdmissionDate", "ClassName", "Section", "RollNumber", "FatherName", "FatherMobile", "FatherAlternateMobile", "MotherName", "Address", "City", "State", "PostalCode", "Country", "EmergencyContactName", "EmergencyContactMobile", "EmergencyContactRelation", "AadharNumber", "PreviousInstitute", "MedicalNotes", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('448ab6d1-c5ea-43f2-8582-6f0352a58431', 'ade837c0-b0f2-498b-8b17-4425178c8a11', '2008-04-12', 'Male', 'O+', 'Indian', 'STU-2600001', '2026-04-01', 'Class 10', 'A', '2600001', 'Ramesh Sharma', '8252404935', '8789352863', 'Sunita Sharma', 'Chapra', 'Chapra', 'Bihar', '841208', 'India', 'Ramesh Sharma', '8252404935', 'Father', '123456789012', 'ABC Institute', NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 16:16:47.138627+00', false, NULL, NULL);
INSERT INTO public."Students" ("Id", "UserId", "DateOfBirth", "Gender", "BloodGroup", "Nationality", "StudentCode", "AdmissionDate", "ClassName", "Section", "RollNumber", "FatherName", "FatherMobile", "FatherAlternateMobile", "MotherName", "Address", "City", "State", "PostalCode", "Country", "EmergencyContactName", "EmergencyContactMobile", "EmergencyContactRelation", "AadharNumber", "PreviousInstitute", "MedicalNotes", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('d2591ec4-f01b-4b92-9581-995ac1a37468', '096e35d0-330a-4e94-bed5-95a4c73208f0', '2009-05-27', 'Male', 'A-', 'Indian', 'STU-26007', '2026-04-07', 'Class 12th', 'A', '2300551', 'Ravi Kumar', '8899776655', '8899700001', 'Rekha Kumari', 'Sector 4', 'Bokaro', 'Jharkhand', '827004', 'India', 'Ravi Kumar', '8899776655', 'Father', '123456789018', 'Scholars Institute', NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:07:49.285479+00', false, NULL, NULL);
INSERT INTO public."Students" ("Id", "UserId", "DateOfBirth", "Gender", "BloodGroup", "Nationality", "StudentCode", "AdmissionDate", "ClassName", "Section", "RollNumber", "FatherName", "FatherMobile", "FatherAlternateMobile", "MotherName", "Address", "City", "State", "PostalCode", "Country", "EmergencyContactName", "EmergencyContactMobile", "EmergencyContactRelation", "AadharNumber", "PreviousInstitute", "MedicalNotes", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('ed50949e-b0c1-4acb-a2ff-6d5ccdf46790', '682aeb24-6c38-4b7d-a906-f00e96a1188e', '2000-01-03', 'Male', 'O+', 'Indian', 'VI-2300545', '2026-09-05', 'Class 12th', 'A', '2300545', 'RISHABH SHARMA', '8252404935', '8789352863', 'Rimjhim Rishabh', 'Village-Rith, Police Statiion-Ekma', 'Chapra', 'Bihar', '841208', 'India', 'RISHABH SHARMA', '6074600003', 'Father', '963978415244', 'A.N School', 'sea food alergy', true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 15:02:56.181824+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:08:16.645239+00', false, NULL, NULL);
INSERT INTO public."Students" ("Id", "UserId", "DateOfBirth", "Gender", "BloodGroup", "Nationality", "StudentCode", "AdmissionDate", "ClassName", "Section", "RollNumber", "FatherName", "FatherMobile", "FatherAlternateMobile", "MotherName", "Address", "City", "State", "PostalCode", "Country", "EmergencyContactName", "EmergencyContactMobile", "EmergencyContactRelation", "AadharNumber", "PreviousInstitute", "MedicalNotes", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('bc578d5e-cb13-4d5d-a1c5-1365cb0398f1', 'cf9b6f45-4d6f-478b-89f1-95badaf85703', '2010-01-14', 'Female', 'B-', 'Indian', 'STU-26006', '2026-04-06', 'Class 12th', 'B', '2300550', 'Mahesh Verma', '9012345678', '9012300001', 'Anita Verma', 'Ranchi Road', 'Ranchi', 'Jharkhand', '834001', 'India', 'Anita Verma', '9012345678', 'Mother', '123456789017', 'Modern Institute', NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:29:03.975721+00', false, NULL, NULL);
INSERT INTO public."Students" ("Id", "UserId", "DateOfBirth", "Gender", "BloodGroup", "Nationality", "StudentCode", "AdmissionDate", "ClassName", "Section", "RollNumber", "FatherName", "FatherMobile", "FatherAlternateMobile", "MotherName", "Address", "City", "State", "PostalCode", "Country", "EmergencyContactName", "EmergencyContactMobile", "EmergencyContactRelation", "AadharNumber", "PreviousInstitute", "MedicalNotes", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('850bb4bc-c64d-4077-81ff-6a82ce1de9c8', '6e4c29c0-b801-4912-a23f-fcb7672e8a46', '2010-06-16', 'Male', 'B+', 'Indian', 'STU-26009', '2026-04-09', 'Class 12th', 'A', '2300553', 'Ramesh Yadav', '9665544332', '9665500001', 'Sunita Yadav', 'Arrah Road', 'Arrah', 'Bihar', '802301', 'India', 'Ramesh Yadav', '9665544332', 'Father', '123456789020', 'Future Institute', NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:04:52.730412+00', false, NULL, NULL);
INSERT INTO public."Students" ("Id", "UserId", "DateOfBirth", "Gender", "BloodGroup", "Nationality", "StudentCode", "AdmissionDate", "ClassName", "Section", "RollNumber", "FatherName", "FatherMobile", "FatherAlternateMobile", "MotherName", "Address", "City", "State", "PostalCode", "Country", "EmergencyContactName", "EmergencyContactMobile", "EmergencyContactRelation", "AadharNumber", "PreviousInstitute", "MedicalNotes", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('0ff5b3a5-a6fe-4daf-8cef-b0af1f09b437', '73305b12-9b72-40e9-97f9-692908c6a754', '2009-07-21', 'Female', 'A+', 'Indian', 'STU-26002', '2026-04-02', 'Class 12th', 'A', '2300546', 'Raj Kumar', '9876543210', '9876500001', 'Sunita Kumari', 'Station Road', 'Chapra', 'Bihar', '841301', 'India', 'Raj Kumar', '9876543210', 'Father', '123456789013', 'XYZ Institute', NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:05:14.664375+00', false, NULL, NULL);
INSERT INTO public."Students" ("Id", "UserId", "DateOfBirth", "Gender", "BloodGroup", "Nationality", "StudentCode", "AdmissionDate", "ClassName", "Section", "RollNumber", "FatherName", "FatherMobile", "FatherAlternateMobile", "MotherName", "Address", "City", "State", "PostalCode", "Country", "EmergencyContactName", "EmergencyContactMobile", "EmergencyContactRelation", "AadharNumber", "PreviousInstitute", "MedicalNotes", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('b51dee7e-7215-4585-84e1-24c56f53c81c', '1e37906b-cae5-42a5-b6da-cd3058055be1', '2008-11-05', 'Male', 'B+', 'Indian', 'STU-26003', '2026-04-03', 'Class 12th', 'B', '2300547', 'Mohan Kumar', '9123456780', '9123400001', 'Pooja Devi', 'Main Road', 'Patna', 'Bihar', '800001', 'India', 'Mohan Kumar', '9123456780', 'Father', '123456789014', 'Bright Institute', 'Dust allergy', true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:05:34.24324+00', false, NULL, NULL);
INSERT INTO public."Students" ("Id", "UserId", "DateOfBirth", "Gender", "BloodGroup", "Nationality", "StudentCode", "AdmissionDate", "ClassName", "Section", "RollNumber", "FatherName", "FatherMobile", "FatherAlternateMobile", "MotherName", "Address", "City", "State", "PostalCode", "Country", "EmergencyContactName", "EmergencyContactMobile", "EmergencyContactRelation", "AadharNumber", "PreviousInstitute", "MedicalNotes", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('b253703a-04b3-4098-bc0e-43def74c37d7', '11c9a98f-1329-4acb-90a7-9c05e25c2711', '2009-02-18', 'Female', 'O-', 'Indian', 'STU-26004', '2026-04-04', 'Class 12th', 'A', '2300548', 'Suresh Sharma', '9001122334', '9001100001', 'Kavita Sharma', 'Kankarbagh', 'Patna', 'Bihar', '800020', 'India', 'Suresh Sharma', '9001122334', 'Father', '123456789015', 'Career Institute', NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:05:55.320111+00', false, NULL, NULL);
INSERT INTO public."Students" ("Id", "UserId", "DateOfBirth", "Gender", "BloodGroup", "Nationality", "StudentCode", "AdmissionDate", "ClassName", "Section", "RollNumber", "FatherName", "FatherMobile", "FatherAlternateMobile", "MotherName", "Address", "City", "State", "PostalCode", "Country", "EmergencyContactName", "EmergencyContactMobile", "EmergencyContactRelation", "AadharNumber", "PreviousInstitute", "MedicalNotes", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('d1fa6c6d-82c9-4374-a56f-12d35e80fe87', '39fd3446-382f-48db-a503-997e6372825b', '2008-09-30', 'Male', 'AB+', 'Indian', 'STU-26005', '2026-04-05', 'Class 12th', 'A', '2300549', 'Arun Singh', '9988776655', '9988700001', 'Neha Singh', 'Ashok Nagar', 'New Delhi', 'Delhi', '110018', 'India', 'Arun Singh', '9988776655', 'Father', '123456789016', 'Delhi Institute', NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:06:13.427669+00', false, NULL, NULL);
INSERT INTO public."Students" ("Id", "UserId", "DateOfBirth", "Gender", "BloodGroup", "Nationality", "StudentCode", "AdmissionDate", "ClassName", "Section", "RollNumber", "FatherName", "FatherMobile", "FatherAlternateMobile", "MotherName", "Address", "City", "State", "PostalCode", "Country", "EmergencyContactName", "EmergencyContactMobile", "EmergencyContactRelation", "AadharNumber", "PreviousInstitute", "MedicalNotes", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('0499efe6-9fcf-4192-be8f-6e963adbcdb9', '2b3ee20f-316a-4478-9694-ea5caf10fbbe', '2009-10-22', 'Female', 'A+', 'Indian', 'STU-26010', '2026-04-10', 'Class 12th', 'C', '2300554', 'Vikas Singh', '9554433221', '9554400001', 'Kiran Singh', 'Rajendra Nagar', 'Patna', 'Bihar', '800016', 'India', 'Kiran Singh', '9554433221', 'Mother', '123456789021', 'Excellence Institute', NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:06:33.124728+00', false, NULL, NULL);
INSERT INTO public."Students" ("Id", "UserId", "DateOfBirth", "Gender", "BloodGroup", "Nationality", "StudentCode", "AdmissionDate", "ClassName", "Section", "RollNumber", "FatherName", "FatherMobile", "FatherAlternateMobile", "MotherName", "Address", "City", "State", "PostalCode", "Country", "EmergencyContactName", "EmergencyContactMobile", "EmergencyContactRelation", "AadharNumber", "PreviousInstitute", "MedicalNotes", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('0f0106f3-6179-40e2-a67a-359899e47f77', 'c7238a1a-f040-49a5-bf69-c9d283e08c4d', '2008-12-09', 'Female', 'O+', 'Indian', 'STU-26008', '2026-04-08', 'Class 12th', 'B', '2300552', 'Rajiv Gupta', '8776655443', '8776600001', 'Meena Gupta', 'Boring Road', 'Patna', 'Bihar', '800013', 'India', 'Rajiv Gupta', '8776655443', 'Father', '123456789019', 'Success Institute', NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:07:32.439876+00', false, NULL, NULL);


--
-- Data for Name: Subjects; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Subjects" ("Id", "OrganizationId", "Name", "Code", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('058ca8c3-6079-4d5f-8f92-6b8c421bcb1b', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Math', 'MATH-45', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-09 14:49:12.58089+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Subjects" ("Id", "OrganizationId", "Name", "Code", "Description", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('d20eeaba-c950-4168-a72f-b1291fd03b74', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Physics', 'PHYSICS_1', NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 14:26:14.164511+00', NULL, NULL, false, NULL, NULL);


--
-- Data for Name: Teachers; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Teachers" ("Id", "UserId", "EmployeeCode", "JoiningDate", "Department", "Designation", "HighestQualification", "Specialization", "TeachingExperienceYears", "Address", "City", "State", "PostalCode", "Country", "AadharNumber", "PreviousInstitute", "Bio", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('376d8c4a-856a-4b84-9dee-5ca7f073d1d3', '18bba6e4-c824-4b61-8e89-27acb9bbd686', 'EMP-2600001', '2026-09-05', 'Chemistry', 3, 1, 'Mathematics', 3.00, NULL, NULL, NULL, NULL, 'India', '123456789012', NULL, NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 16:20:52.22955+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 16:22:07.988927+00', false, NULL, NULL);
INSERT INTO public."Teachers" ("Id", "UserId", "EmployeeCode", "JoiningDate", "Department", "Designation", "HighestQualification", "Specialization", "TeachingExperienceYears", "Address", "City", "State", "PostalCode", "Country", "AadharNumber", "PreviousInstitute", "Bio", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('f7fb83f8-d439-47ee-b9fc-78b534d7e177', '7ff47358-7d26-4f80-9402-efb79759abf8', 'EMP-260001', '2026-09-05', 'Chemistry', 3, 4, 'Mathematics', 3.00, 'Chapra, Bihar', 'Chapra', 'Bihar', '841301', 'India', '123456789012', NULL, 'Experienced Mathematics educator and Head of Department.', true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Teachers" ("Id", "UserId", "EmployeeCode", "JoiningDate", "Department", "Designation", "HighestQualification", "Specialization", "TeachingExperienceYears", "Address", "City", "State", "PostalCode", "Country", "AadharNumber", "PreviousInstitute", "Bio", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('e47f2cd9-2d37-4876-a940-ec83544de738', '4a002566-1b94-4491-9bfc-04a65ff95e03', 'EMP-260002', '2026-07-15', 'Physics', 2, 4, 'Physics', 6.00, 'Patna, Bihar', 'Patna', 'Bihar', '800001', 'India', '123456789013', 'ABC Institute', 'Physics teacher with strong academic and mentoring experience.', true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Teachers" ("Id", "UserId", "EmployeeCode", "JoiningDate", "Department", "Designation", "HighestQualification", "Specialization", "TeachingExperienceYears", "Address", "City", "State", "PostalCode", "Country", "AadharNumber", "PreviousInstitute", "Bio", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('18d9a297-8ec7-4e54-a095-2021e0b3b209', 'fcd7e65c-2fe5-48c5-ad4c-c395d183d602', 'EMP-260003', '2026-06-10', 'Mathematics', 2, 4, 'Mathematics', 5.50, 'Gaya, Bihar', 'Gaya', 'Bihar', '823001', 'India', '123456789014', 'Bright Institute', 'Specialized in competitive mathematics preparation.', true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Teachers" ("Id", "UserId", "EmployeeCode", "JoiningDate", "Department", "Designation", "HighestQualification", "Specialization", "TeachingExperienceYears", "Address", "City", "State", "PostalCode", "Country", "AadharNumber", "PreviousInstitute", "Bio", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('f372b9fd-28f3-499a-9c64-4b79fba30f52', '1c31e9d2-0447-4434-9c22-9bc2ca48cc2e', 'EMP-260004', '2026-05-20', 'English', 1, 6, 'English Literature', 4.00, 'Muzaffarpur, Bihar', 'Muzaffarpur', 'Bihar', '842001', 'India', '123456789015', 'Career Institute', 'English language and literature educator.', true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Teachers" ("Id", "UserId", "EmployeeCode", "JoiningDate", "Department", "Designation", "HighestQualification", "Specialization", "TeachingExperienceYears", "Address", "City", "State", "PostalCode", "Country", "AadharNumber", "PreviousInstitute", "Bio", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('b36d50ad-ba42-4646-8ff4-cbc9e4aebe91', '94f5189b-2bb0-4234-a2b0-1bfb0b788a6b', 'EMP-260005', '2026-04-18', 'Biology', 2, 4, 'Biology', 7.00, 'Ranchi, Jharkhand', 'Ranchi', 'Jharkhand', '834001', 'India', '123456789016', 'Modern Institute', 'Biology educator focused on conceptual learning.', true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Teachers" ("Id", "UserId", "EmployeeCode", "JoiningDate", "Department", "Designation", "HighestQualification", "Specialization", "TeachingExperienceYears", "Address", "City", "State", "PostalCode", "Country", "AadharNumber", "PreviousInstitute", "Bio", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('9ea41e09-2eda-4956-a121-e47832d02f04', '034f5e5a-56e3-4f6c-b7db-be220423ff8a', 'EMP-260006', '2026-03-12', 'Chemistry', 1, 4, 'Organic Chemistry', 2.50, 'Bokaro, Jharkhand', 'Bokaro', 'Jharkhand', '827004', 'India', '123456789017', NULL, 'Chemistry teacher specializing in organic chemistry.', true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Teachers" ("Id", "UserId", "EmployeeCode", "JoiningDate", "Department", "Designation", "HighestQualification", "Specialization", "TeachingExperienceYears", "Address", "City", "State", "PostalCode", "Country", "AadharNumber", "PreviousInstitute", "Bio", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('c5949d6f-5942-4a53-9c64-d620b9842fcf', '961d09c2-5f1c-4fde-8a04-01e73ecd0fdd', 'EMP-260008', '2026-01-15', 'Physics', 1, 8, 'Applied Physics', 5.00, 'Delhi', 'New Delhi', 'Delhi', '110018', 'India', '123456789019', 'Success Institute', 'Physics educator with engineering background.', true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Teachers" ("Id", "UserId", "EmployeeCode", "JoiningDate", "Department", "Designation", "HighestQualification", "Specialization", "TeachingExperienceYears", "Address", "City", "State", "PostalCode", "Country", "AadharNumber", "PreviousInstitute", "Bio", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('6d5b00fc-53a1-4f66-adf5-8b78521e78a3', 'cf9e1d6b-3a17-4b02-9753-5b1276605254', 'EMP-260009', '2025-12-10', 'English', 1, 6, 'English', 4.50, 'Arrah, Bihar', 'Arrah', 'Bihar', '802301', 'India', '123456789020', 'Future Institute', 'English communication and grammar specialist.', true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Teachers" ("Id", "UserId", "EmployeeCode", "JoiningDate", "Department", "Designation", "HighestQualification", "Specialization", "TeachingExperienceYears", "Address", "City", "State", "PostalCode", "Country", "AadharNumber", "PreviousInstitute", "Bio", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('8c093a52-b388-47a2-a20b-33d53d2b2195', '99cabede-5604-4131-95f2-4782b8253038', 'EMP-260010', '2025-11-05', 'Biology', 3, 4, 'Zoology', 8.00, 'Patna, Bihar', 'Patna', 'Bihar', '800016', 'India', '123456789021', 'Excellence Institute', 'Experienced Biology educator and academic coordinator.', true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL);
INSERT INTO public."Teachers" ("Id", "UserId", "EmployeeCode", "JoiningDate", "Department", "Designation", "HighestQualification", "Specialization", "TeachingExperienceYears", "Address", "City", "State", "PostalCode", "Country", "AadharNumber", "PreviousInstitute", "Bio", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt") VALUES ('e47a8995-36c5-4007-be3f-2b8a162b1bb0', 'eb98c3d6-37e0-4236-8741-09e66391143c', 'EMP-260007', '2026-02-25', 'Computer Science', 2, 4, 'Applied Mathematics', 3.50, 'Patna, Bihar', 'Patna', 'Bihar', '800020', 'India', '123456789018', 'Scholars Institute', 'Mathematics teacher focused on problem solving.', true, NULL, '2026-09-05 16:30:50.158925+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:40:55.193711+00', false, NULL, NULL);


--
-- Data for Name: UserBranchAccess; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."UserBranchAccess" ("Id", "UserId", "BranchId", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('d98d67e5-6663-4b47-9f22-e56dd427ddab', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2152c664-eefe-4f4c-baea-b8e430bc3b2f', true, NULL, '2026-09-05 19:56:22.477297+00', NULL, NULL);
INSERT INTO public."UserBranchAccess" ("Id", "UserId", "BranchId", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('efb96b7b-0c63-47be-96b6-4234b4bb814a', 'ac530be2-dbaf-40c4-a759-dbda39ed14da', '9a74dd2e-8182-48c2-8370-e71159e4469e', true, NULL, '2026-09-05 19:56:22.507499+00', NULL, NULL);
INSERT INTO public."UserBranchAccess" ("Id", "UserId", "BranchId", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('6c84dfe9-9761-4f71-8168-cc4e1b51ee7c', '457f456f-0634-4d12-a7ab-d8a2797a6d2d', '8adb1543-6094-45bd-8f1b-3b4ab1732919', true, NULL, '2026-09-05 20:17:15.219592+00', NULL, NULL);
INSERT INTO public."UserBranchAccess" ("Id", "UserId", "BranchId", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt") VALUES ('5c7db8f2-75f3-445a-b5a1-bd4c3961ba1d', '9606fbc2-1d55-4aed-881c-062bafc34339', 'c750a02d-d09c-4324-80bd-182ee0d6c8aa', true, NULL, '2026-09-05 20:18:56.917549+00', NULL, NULL);


--
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('73305b12-9b72-40e9-97f9-692908c6a754', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Amit', 'Kumar', 'amit.kumar@student.com', '9876543210', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 3, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:05:14.664379+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('ade837c0-b0f2-498b-8b17-4425178c8a11', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Rishabh', 'Sharma', 'rishabh.sharma@student.com', '8252404935', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 3, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 16:16:47.138727+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('1e37906b-cae5-42a5-b6da-cd3058055be1', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Rajesh', 'Kumar', 'rajesh.kumar@student.com', '9123456780', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 3, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:05:34.243241+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('682aeb24-6c38-4b7d-a906-f00e96a1188e', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Saurav', 'Sharma', 'saurav@gmail.com', '6207460003', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 3, false, false, NULL, NULL, NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 15:02:56.181817+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:08:16.645241+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('457f456f-0634-4d12-a7ab-d8a2797a6d2d', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Vikram', 'Singh', 'vikram.admin@bcc.com', '9876543220', '+9HSuMyIm1NCO6/+SnGYEl1PHmSUNs+aqYmy5GFHkQMjFbmGmjLfNIOTo1Vxykm+wYSW0TQ=', 4, true, true, '2026-09-05 21:00:19.626179+00', '01c6146aa60665ad35cc77e37975a8e9dfc1ef367ef8ea977708f58af87b3619', '2026-09-12 21:00:19.626077+00', true, NULL, '2026-09-05 20:17:15.219592+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 21:02:57.170814+00', false, NULL, NULL, '/uploads/profile/ffda95159c734435903ebdcf73c58f28.jpg', NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('18bba6e4-c824-4b61-8e89-27acb9bbd686', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Sanjeet', 'Yadav', 'sanjeet@yopmail.com', '7225025072', 'kBsdQKpv1XF1fElivJus+jq3Gfjws0tBOAOOZy3yeRskBgKoXVFwak9jXYWS+Z4lGMl1H7K1GQ==', 2, false, false, NULL, NULL, NULL, true, 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 16:20:52.229548+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-05 16:22:07.988928+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('7ff47358-7d26-4f80-9402-efb79759abf8', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Sanjeet', 'Yadav', 'sanjeet.yadav@teacher.com', '7225025072', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 2, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('4a002566-1b94-4491-9bfc-04a65ff95e03', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Amit', 'Sharma', 'amit.sharma@teacher.com', '9876543210', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 2, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('fcd7e65c-2fe5-48c5-ad4c-c395d183d602', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Priya', 'Singh', 'priya.singh@teacher.com', '9123456780', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 2, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('1c31e9d2-0447-4434-9c22-9bc2ca48cc2e', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Rahul', 'Kumar', 'rahul.kumar@teacher.com', '9001122334', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 2, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('94f5189b-2bb0-4234-a2b0-1bfb0b788a6b', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Neha', 'Verma', 'neha.verma@teacher.com', '9988776655', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 2, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('034f5e5a-56e3-4f6c-b7db-be220423ff8a', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Manish', 'Gupta', 'manish.gupta@teacher.com', '9012345678', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 2, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('961d09c2-5f1c-4fde-8a04-01e73ecd0fdd', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Rohit', 'Singh', 'rohit.singh@teacher.com', '8776655443', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 2, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('cf9e1d6b-3a17-4b02-9753-5b1276605254', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Pooja', 'Sharma', 'pooja.sharma@teacher.com', '9665544332', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 2, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('99cabede-5604-4131-95f2-4782b8253038', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Vikas', 'Yadav', 'vikas.yadav@teacher.com', '9554433221', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 2, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 16:30:50.158925+00', NULL, NULL, false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('84aa86d4-5bd2-4a8c-9dec-86457f643de8', NULL, 'Rishabh', 'Sharma', 'rishabh.orgadmin@vargshala.com', '+919876543210', 'm0tcaJvkn/qIu5ojHX5QrhJnQxD1Pc9EiMdPeLsKue+hPcnD8LYwnx83MfmQiSivqY/oc/k=', 1001, true, true, NULL, NULL, NULL, true, NULL, '2026-09-03 16:55:00+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c7', '2026-09-04 20:02:00+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('ca1ae7f9-574d-40cc-be60-4e3296ff26c7', NULL, 'Rishabh Kr', 'Sharma', 'rishabh.sharma@yopmail.com', '+918789352863', 'zdHN+k3Duns7/S7ABrC+jvDX98skrSQbI9b62x5nYywvNbUDzIJZvglHL7qFC/8KDSy54Bk=', 1001, true, true, '2026-09-09 13:17:35.956597+00', 'b6870bbfa76b14bfcbc95290d5115840c5805c72e35451420103e1078785d62a', '2026-09-16 13:17:35.956472+00', true, NULL, '2026-09-01 19:49:38.873884+00', NULL, '2026-09-09 13:17:36.064338+00', false, NULL, NULL, 'data:image/jpeg;base64,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', NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('9606fbc2-1d55-4aed-881c-062bafc34339', '9d66519f-a058-4ada-80c5-fe836aeba6dd', 'Rajesh', 'Verma', 'rishabh.admin@vargshala.com', '9876543230', 'k3FTsHepS2ZWrb956PmBALgGyIG/ggInEHeI4bIeGx+S33rSBvex6iuZMcVu6YoMTIcx2HM=', 4, true, true, '2026-09-09 13:19:26.974384+00', '7681e8d5560d1fdedf9fc32cadfefba3d01c671aac226bd859264dd9b3aee219', '2026-09-16 13:19:26.974383+00', true, NULL, '2026-09-05 20:18:00+00', NULL, '2026-09-09 13:19:27.016026+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('eb98c3d6-37e0-4236-8741-09e66391143c', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Kavita', 'Kumari', 'kavita.kumari@teacher.com', '8899776655', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 2, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 16:30:50.158925+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 08:40:55.193714+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('39fd3446-382f-48db-a503-997e6372825b', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Sanjay', 'Singh', 'sanjay.singh@student.com', '9988776655', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 3, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:06:13.427671+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('2b3ee20f-316a-4478-9694-ea5caf10fbbe', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Pankaj', 'Singh', 'pankaj.singh@student.com', '9554433221', '"2b3ee20f-316a-4478-9694-ea5caf10fbbe"	"0139d0e2-265c-4ce8-9700-d468ed12c1ac"	"Pankaj"	"Singh"	"pankaj.singh@student.com"	"9554433221"	"E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw=="	3	true	true				true		"2026-09-05 20:54:53.097138+05:30"			false			', 3, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:06:33.124729+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('c7238a1a-f040-49a5-bf69-c9d283e08c4d', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Anil', 'Gupta', 'anil.gupta@student.com', '8776655443', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 3, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:07:32.439878+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('096e35d0-330a-4e94-bed5-95a4c73208f0', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Vijay', 'Kumar', 'vijay.kumar@student.com', '8899776655', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 3, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:07:49.285483+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('6e4c29c0-b801-4912-a23f-fcb7672e8a46', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Suresh', 'Yadav', 'suresh.yadav@student.com', '9665544332', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 3, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:04:52.730414+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('11c9a98f-1329-4acb-90a7-9c05e25c2711', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Manoj', 'Sharma', 'manoj.sharma@student.com', '9001122334', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 3, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:05:55.320112+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('cf9b6f45-4d6f-478b-89f1-95badaf85703', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Deepak', 'Verma', 'deepak.verma@student.com', '9012345678', 'E2YZnHIabz3jv6d6x+l6vIKw3KEc2hCXkLPvuFxdrbq+dIwIHbEnvg7lUAIB5XTnDxGvTRsWbw==', 3, true, true, NULL, NULL, NULL, true, NULL, '2026-09-05 15:24:53.097138+00', 'ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '2026-09-10 15:29:03.975729+00', false, NULL, NULL, NULL, NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('ca1ae7f9-574d-40cc-be60-4e3296ff26c4', '0139d0e2-265c-4ce8-9700-d468ed12c1ac', 'Rishabh', 'Sharma', 'rishabh.admin@yopmail.com', '+919876543210', 'm0tcaJvkn/qIu5ojHX5QrhJnQxD1Pc9EiMdPeLsKue+hPcnD8LYwnx83MfmQiSivqY/oc/k=', 1, true, true, '2026-09-10 16:54:50.903322+00', 'b6ce9abba2dfbb6ece0e79d1bdfd22df02c0da5a511881d23112d72fe66cfd97', '2026-09-17 16:54:50.903069+00', true, NULL, '2026-09-01 19:49:38.873995+00', NULL, '2026-09-10 16:54:51.20716+00', false, NULL, NULL, 'data:image/jpeg;base64,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', NULL, NULL);
INSERT INTO public."Users" ("Id", "OrganizationId", "FirstName", "LastName", "Email", "Mobile", "PasswordHash", "Role", "EmailVerified", "MobileVerified", "LastLoginAt", "RefreshToken", "RefreshTokenExpiryTime", "IsActive", "CreatedBy", "CreatedAt", "UpdatedBy", "UpdatedAt", "IsDeleted", "DeletedBy", "DeletedAt", "ProfilePictureUrl", "PasswordResetToken", "PasswordResetTokenExpiresAt") VALUES ('ac530be2-dbaf-40c4-a759-dbda39ed14da', '9d66519f-a058-4ada-80c5-fe836aeba6dd', 'Bright', 'Academy', 'iamrishabhsharma0301@gmail.com', '8789352863', 'k3FTsHepS2ZWrb956PmBALgGyIG/ggInEHeI4bIeGx+S33rSBvex6iuZMcVu6YoMTIcx2HM=', 1, true, true, '2026-09-10 15:22:57.611606+00', '733f6993ffa243ba601540956008696c980e4b0f10a7ab939372928b2590d698', '2026-09-17 15:22:57.611605+00', true, NULL, '2026-09-05 16:54:00+00', NULL, '2026-09-10 15:22:57.612085+00', false, NULL, NULL, '/uploads/profile/d787762860a44f25bbad0b8a66c5d4cc.jpg', NULL, NULL);


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20260901194108_InitialCreate', '10.0.0');


--
-- Name: Attendances PK_Attendances; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Attendances"
    ADD CONSTRAINT "PK_Attendances" PRIMARY KEY ("Id");


--
-- Name: BatchSchedules PK_BatchSchedules; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BatchSchedules"
    ADD CONSTRAINT "PK_BatchSchedules" PRIMARY KEY ("Id");


--
-- Name: BatchStudents PK_BatchStudents; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BatchStudents"
    ADD CONSTRAINT "PK_BatchStudents" PRIMARY KEY ("Id");


--
-- Name: BatchTeachers PK_BatchTeachers; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BatchTeachers"
    ADD CONSTRAINT "PK_BatchTeachers" PRIMARY KEY ("Id");


--
-- Name: Batches PK_Batches; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Batches"
    ADD CONSTRAINT "PK_Batches" PRIMARY KEY ("Id");


--
-- Name: Branches PK_Branches; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Branches"
    ADD CONSTRAINT "PK_Branches" PRIMARY KEY ("Id");


--
-- Name: ClassSessions PK_ClassSessions; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ClassSessions"
    ADD CONSTRAINT "PK_ClassSessions" PRIMARY KEY ("Id");


--
-- Name: Classes PK_Classes; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Classes"
    ADD CONSTRAINT "PK_Classes" PRIMARY KEY ("Id");


--
-- Name: Coupons PK_Coupons; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Coupons"
    ADD CONSTRAINT "PK_Coupons" PRIMARY KEY ("Id");


--
-- Name: EmailTemplates PK_EmailTemplates; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."EmailTemplates"
    ADD CONSTRAINT "PK_EmailTemplates" PRIMARY KEY ("Id");


--
-- Name: Organizations PK_Organizations; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Organizations"
    ADD CONSTRAINT "PK_Organizations" PRIMARY KEY ("Id");


--
-- Name: Subjects PK_Subjects; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Subjects"
    ADD CONSTRAINT "PK_Subjects" PRIMARY KEY ("Id");


--
-- Name: UserBranchAccess PK_UserBranchAccess; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserBranchAccess"
    ADD CONSTRAINT "PK_UserBranchAccess" PRIMARY KEY ("Id");


--
-- Name: Users PK_Users; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: Students Students_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Students"
    ADD CONSTRAINT "Students_pkey" PRIMARY KEY ("Id");


--
-- Name: Teachers Teachers_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Teachers"
    ADD CONSTRAINT "Teachers_pkey" PRIMARY KEY ("Id");


--
-- Name: Attendances UQ_Attendances_ClassSessionId_StudentId; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Attendances"
    ADD CONSTRAINT "UQ_Attendances_ClassSessionId_StudentId" UNIQUE ("ClassSessionId", "StudentId");


--
-- Name: BatchStudents UQ_BatchStudents_BatchId_StudentId; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BatchStudents"
    ADD CONSTRAINT "UQ_BatchStudents_BatchId_StudentId" UNIQUE ("BatchId", "StudentId");


--
-- Name: BatchTeachers UQ_BatchTeachers_BatchId_TeacherId; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BatchTeachers"
    ADD CONSTRAINT "UQ_BatchTeachers_BatchId_TeacherId" UNIQUE ("BatchId", "TeacherId");


--
-- Name: Batches UQ_Batches_ClassId_Code; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Batches"
    ADD CONSTRAINT "UQ_Batches_ClassId_Code" UNIQUE ("ClassId", "Code");


--
-- Name: Branches UQ_Branches_OrganizationId_Code; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Branches"
    ADD CONSTRAINT "UQ_Branches_OrganizationId_Code" UNIQUE ("OrganizationId", "Code");


--
-- Name: Classes UQ_Classes_BranchId_Code; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Classes"
    ADD CONSTRAINT "UQ_Classes_BranchId_Code" UNIQUE ("BranchId", "Code");


--
-- Name: Students UQ_Students_UserId; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Students"
    ADD CONSTRAINT "UQ_Students_UserId" UNIQUE ("UserId");


--
-- Name: Subjects UQ_Subjects_OrganizationId_Code; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Subjects"
    ADD CONSTRAINT "UQ_Subjects_OrganizationId_Code" UNIQUE ("OrganizationId", "Code");


--
-- Name: Teachers UQ_Teachers_UserId; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Teachers"
    ADD CONSTRAINT "UQ_Teachers_UserId" UNIQUE ("UserId");


--
-- Name: UserBranchAccess UQ_UserBranchAccess_UserId_BranchId; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserBranchAccess"
    ADD CONSTRAINT "UQ_UserBranchAccess_UserId_BranchId" UNIQUE ("UserId", "BranchId");


--
-- Name: IX_Attendances_ClassSessionId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Attendances_ClassSessionId" ON public."Attendances" USING btree ("ClassSessionId");


--
-- Name: IX_Attendances_StudentId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Attendances_StudentId" ON public."Attendances" USING btree ("StudentId");


--
-- Name: IX_BatchSchedules_BatchId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_BatchSchedules_BatchId" ON public."BatchSchedules" USING btree ("BatchId");


--
-- Name: IX_BatchSchedules_DayOfWeek; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_BatchSchedules_DayOfWeek" ON public."BatchSchedules" USING btree ("DayOfWeek");


--
-- Name: IX_BatchStudents_BatchId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_BatchStudents_BatchId" ON public."BatchStudents" USING btree ("BatchId");


--
-- Name: IX_BatchStudents_StudentId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_BatchStudents_StudentId" ON public."BatchStudents" USING btree ("StudentId");


--
-- Name: IX_BatchTeachers_BatchId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_BatchTeachers_BatchId" ON public."BatchTeachers" USING btree ("BatchId");


--
-- Name: IX_BatchTeachers_TeacherId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_BatchTeachers_TeacherId" ON public."BatchTeachers" USING btree ("TeacherId");


--
-- Name: IX_Batches_ClassId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Batches_ClassId" ON public."Batches" USING btree ("ClassId");


--
-- Name: IX_Batches_SubjectId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Batches_SubjectId" ON public."Batches" USING btree ("SubjectId");


--
-- Name: IX_Branches_City; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Branches_City" ON public."Branches" USING btree ("City");


--
-- Name: IX_Branches_OrganizationId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Branches_OrganizationId" ON public."Branches" USING btree ("OrganizationId");


--
-- Name: IX_ClassSessions_BatchId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ClassSessions_BatchId" ON public."ClassSessions" USING btree ("BatchId");


--
-- Name: IX_ClassSessions_BatchId_SessionDate; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ClassSessions_BatchId_SessionDate" ON public."ClassSessions" USING btree ("BatchId", "SessionDate");


--
-- Name: IX_ClassSessions_BatchScheduleId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ClassSessions_BatchScheduleId" ON public."ClassSessions" USING btree ("BatchScheduleId");


--
-- Name: IX_ClassSessions_SessionDate; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ClassSessions_SessionDate" ON public."ClassSessions" USING btree ("SessionDate");


--
-- Name: IX_ClassSessions_TeacherId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_ClassSessions_TeacherId" ON public."ClassSessions" USING btree ("TeacherId");


--
-- Name: IX_Classes_BranchId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Classes_BranchId" ON public."Classes" USING btree ("BranchId");


--
-- Name: IX_Coupons_Code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Coupons_Code" ON public."Coupons" USING btree ("Code") WHERE ("IsDeleted" = false);


--
-- Name: IX_Coupons_Code_NonDeleted; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Coupons_Code_NonDeleted" ON public."Coupons" USING btree ("Code") WHERE ("IsDeleted" = false);


--
-- Name: IX_Coupons_ExpiryDate; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Coupons_ExpiryDate" ON public."Coupons" USING btree ("ExpiryDate");


--
-- Name: IX_Coupons_Org_Active; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Coupons_Org_Active" ON public."Coupons" USING btree ("OrganizationId", "IsActive") WHERE ("IsDeleted" = false);


--
-- Name: IX_Coupons_Organization_Active; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Coupons_Organization_Active" ON public."Coupons" USING btree ("OrganizationId", "IsActive") WHERE ("IsDeleted" = false);


--
-- Name: IX_EmailTemplates_Category; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_EmailTemplates_Category" ON public."EmailTemplates" USING btree ("Category") WHERE ("IsDeleted" = false);


--
-- Name: IX_EmailTemplates_Code_OrganizationId; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_EmailTemplates_Code_OrganizationId" ON public."EmailTemplates" USING btree ("Code", "OrganizationId") WHERE ("IsDeleted" = false);


--
-- Name: IX_EmailTemplates_Org_Code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_EmailTemplates_Org_Code" ON public."EmailTemplates" USING btree (COALESCE("OrganizationId", '00000000-0000-0000-0000-000000000000'::uuid), "Code") WHERE ("IsDeleted" = false);


--
-- Name: IX_EmailTemplates_OrganizationId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_EmailTemplates_OrganizationId" ON public."EmailTemplates" USING btree ("OrganizationId");


--
-- Name: IX_EmailTemplates_TargetRole; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_EmailTemplates_TargetRole" ON public."EmailTemplates" USING btree ("TargetRole") WHERE ("IsDeleted" = false);


--
-- Name: IX_Organizations_Code; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Organizations_Code" ON public."Organizations" USING btree ("Code") WHERE ("IsDeleted" = false);


--
-- Name: IX_Students_ClassName_Section; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Students_ClassName_Section" ON public."Students" USING btree ("ClassName", "Section");


--
-- Name: IX_Students_StudentCode; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Students_StudentCode" ON public."Students" USING btree ("StudentCode") WHERE (("StudentCode" IS NOT NULL) AND ("IsDeleted" = false));


--
-- Name: IX_Subjects_OrganizationId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Subjects_OrganizationId" ON public."Subjects" USING btree ("OrganizationId");


--
-- Name: IX_Teachers_Department; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Teachers_Department" ON public."Teachers" USING btree ("Department");


--
-- Name: IX_Teachers_Designation; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Teachers_Designation" ON public."Teachers" USING btree ("Designation") WHERE ("IsDeleted" = false);


--
-- Name: IX_Teachers_EmployeeCode; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Teachers_EmployeeCode" ON public."Teachers" USING btree ("EmployeeCode") WHERE (("EmployeeCode" IS NOT NULL) AND ("IsDeleted" = false));


--
-- Name: IX_UserBranchAccess_BranchId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserBranchAccess_BranchId" ON public."UserBranchAccess" USING btree ("BranchId");


--
-- Name: IX_UserBranchAccess_UserId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_UserBranchAccess_UserId" ON public."UserBranchAccess" USING btree ("UserId");


--
-- Name: IX_Users_Email_OrganizationId; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX "IX_Users_Email_OrganizationId" ON public."Users" USING btree ("Email", "OrganizationId") WHERE (("Email" IS NOT NULL) AND ("IsDeleted" = false));


--
-- Name: IX_Users_OrganizationId; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX "IX_Users_OrganizationId" ON public."Users" USING btree ("OrganizationId");


--
-- Name: Attendances FK_Attendances_ClassSessions_ClassSessionId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Attendances"
    ADD CONSTRAINT "FK_Attendances_ClassSessions_ClassSessionId" FOREIGN KEY ("ClassSessionId") REFERENCES public."ClassSessions"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: Attendances FK_Attendances_Students_StudentId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Attendances"
    ADD CONSTRAINT "FK_Attendances_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES public."Students"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: BatchSchedules FK_BatchSchedules_Batches_BatchId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BatchSchedules"
    ADD CONSTRAINT "FK_BatchSchedules_Batches_BatchId" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: BatchStudents FK_BatchStudents_Batches_BatchId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BatchStudents"
    ADD CONSTRAINT "FK_BatchStudents_Batches_BatchId" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: BatchStudents FK_BatchStudents_Students_StudentId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BatchStudents"
    ADD CONSTRAINT "FK_BatchStudents_Students_StudentId" FOREIGN KEY ("StudentId") REFERENCES public."Students"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: BatchTeachers FK_BatchTeachers_Batches_BatchId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BatchTeachers"
    ADD CONSTRAINT "FK_BatchTeachers_Batches_BatchId" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: BatchTeachers FK_BatchTeachers_Teachers_TeacherId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."BatchTeachers"
    ADD CONSTRAINT "FK_BatchTeachers_Teachers_TeacherId" FOREIGN KEY ("TeacherId") REFERENCES public."Teachers"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: Batches FK_Batches_Classes_ClassId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Batches"
    ADD CONSTRAINT "FK_Batches_Classes_ClassId" FOREIGN KEY ("ClassId") REFERENCES public."Classes"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: Batches FK_Batches_Subjects_SubjectId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Batches"
    ADD CONSTRAINT "FK_Batches_Subjects_SubjectId" FOREIGN KEY ("SubjectId") REFERENCES public."Subjects"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: Branches FK_Branches_Organizations_OrganizationId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Branches"
    ADD CONSTRAINT "FK_Branches_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES public."Organizations"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: ClassSessions FK_ClassSessions_BatchSchedules_BatchScheduleId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ClassSessions"
    ADD CONSTRAINT "FK_ClassSessions_BatchSchedules_BatchScheduleId" FOREIGN KEY ("BatchScheduleId") REFERENCES public."BatchSchedules"("Id") ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: ClassSessions FK_ClassSessions_Batches_BatchId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ClassSessions"
    ADD CONSTRAINT "FK_ClassSessions_Batches_BatchId" FOREIGN KEY ("BatchId") REFERENCES public."Batches"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: ClassSessions FK_ClassSessions_Teachers_TeacherId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."ClassSessions"
    ADD CONSTRAINT "FK_ClassSessions_Teachers_TeacherId" FOREIGN KEY ("TeacherId") REFERENCES public."Teachers"("Id") ON UPDATE CASCADE ON DELETE SET NULL;


--
-- Name: Classes FK_Classes_Branches_BranchId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Classes"
    ADD CONSTRAINT "FK_Classes_Branches_BranchId" FOREIGN KEY ("BranchId") REFERENCES public."Branches"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: Coupons FK_Coupons_Organizations; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Coupons"
    ADD CONSTRAINT "FK_Coupons_Organizations" FOREIGN KEY ("OrganizationId") REFERENCES public."Organizations"("Id") ON DELETE RESTRICT;


--
-- Name: EmailTemplates FK_EmailTemplates_Organizations_OrganizationId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."EmailTemplates"
    ADD CONSTRAINT "FK_EmailTemplates_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES public."Organizations"("Id") ON DELETE RESTRICT;


--
-- Name: Students FK_Students_Users; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Students"
    ADD CONSTRAINT "FK_Students_Users" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: Subjects FK_Subjects_Organizations_OrganizationId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Subjects"
    ADD CONSTRAINT "FK_Subjects_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES public."Organizations"("Id") ON UPDATE CASCADE ON DELETE RESTRICT;


--
-- Name: Teachers FK_Teachers_Users; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Teachers"
    ADD CONSTRAINT "FK_Teachers_Users" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: UserBranchAccess FK_UserBranchAccess_Branches_BranchId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserBranchAccess"
    ADD CONSTRAINT "FK_UserBranchAccess_Branches_BranchId" FOREIGN KEY ("BranchId") REFERENCES public."Branches"("Id") ON DELETE CASCADE;


--
-- Name: UserBranchAccess FK_UserBranchAccess_Users_UserId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."UserBranchAccess"
    ADD CONSTRAINT "FK_UserBranchAccess_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: Users FK_Users_Organizations_OrganizationId; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "FK_Users_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES public."Organizations"("Id") ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

-- \unrestrict 3WVzluOg1FpS3DphFDaqpWLCxQB2cIXMue0l2kdotJt1iDq5OFhd1CPBeTCpvDX  (commented out for pgAdmin/GUI SQL editor compatibility)

