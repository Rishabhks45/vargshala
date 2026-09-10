-- Table: public.Teachers
-- Designation Enum:
--   1 = Teacher
--   2 = Senior Teacher
--   3 = HOD
--   4 = Assistant Teacher
--   5 = Guest Lecturer
--   6 = Principal
--   7 = Vice Principal
--   8 = Lab Assistant
--   9 = Other
--
-- HighestQualification Enum:
--   1 = B.Ed.
--   2 = M.Ed.
--   3 = B.Sc.
--   4 = M.Sc.
--   5 = B.A.
--   6 = M.A.
--   7 = B.Tech.
--   8 = M.Tech.
--   9 = B.Com.
--   10 = M.Com.
--   11 = BCA
--   12 = MCA
--   13 = MBA
--   14 = Ph.D.
--   15 = Other

-- DROP TABLE IF EXISTS public."Teachers";

CREATE TABLE IF NOT EXISTS public."Teachers"
(
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "EmployeeCode" character varying(50) COLLATE pg_catalog."default",
    "JoiningDate" date,
    "Department" character varying(100) COLLATE pg_catalog."default",
    "Designation" integer,
    "HighestQualification" integer,
    "Specialization" character varying(150) COLLATE pg_catalog."default",
    "TeachingExperienceYears" numeric(5, 2),
    "Address" text COLLATE pg_catalog."default",
    "City" character varying(100) COLLATE pg_catalog."default",
    "State" character varying(100) COLLATE pg_catalog."default",
    "PostalCode" character varying(20) COLLATE pg_catalog."default",
    "Country" character varying(100) COLLATE pg_catalog."default",
    "AadharNumber" character varying(20) COLLATE pg_catalog."default",
    "PreviousInstitute" character varying(200) COLLATE pg_catalog."default",
    "Bio" text COLLATE pg_catalog."default",
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone,
    CONSTRAINT "Teachers_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Teachers_UserId" UNIQUE ("UserId"),
    CONSTRAINT "FK_Teachers_Users" FOREIGN KEY ("UserId")
        REFERENCES public."Users" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Teachers"
    OWNER to postgres;

-- Indexes
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Teachers_EmployeeCode"
    ON public."Teachers" USING btree
    ("EmployeeCode" COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default
    WHERE "EmployeeCode" IS NOT NULL AND "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_Teachers_Department"
    ON public."Teachers" USING btree
    ("Department" COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE INDEX IF NOT EXISTS "IX_Teachers_Designation"
    ON public."Teachers" USING btree
    ("Designation" ASC NULLS LAST)
    TABLESPACE pg_default;
