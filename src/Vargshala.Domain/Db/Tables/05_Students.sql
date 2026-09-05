-- Table: public.Students

-- DROP TABLE IF EXISTS public."Students";

CREATE TABLE IF NOT EXISTS public."Students"
(
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Gender" character varying(20) COLLATE pg_catalog."default",
    "DateOfBirth" date,
    "BloodGroup" character varying(10) COLLATE pg_catalog."default",
    "Nationality" character varying(50) COLLATE pg_catalog."default",
    "StudentCode" character varying(50) COLLATE pg_catalog."default",
    "EnrollmentDate" date,
    "ClassName" character varying(100) COLLATE pg_catalog."default",
    "Section" character varying(50) COLLATE pg_catalog."default",
    "RollNumber" character varying(50) COLLATE pg_catalog."default",
    "FatherName" character varying(150) COLLATE pg_catalog."default",
    "FatherMobile" character varying(20) COLLATE pg_catalog."default",
    "FatherAlternateMobile" character varying(20) COLLATE pg_catalog."default",
    "MotherName" character varying(150) COLLATE pg_catalog."default",
    "Address" text COLLATE pg_catalog."default",
    "City" character varying(100) COLLATE pg_catalog."default",
    "State" character varying(100) COLLATE pg_catalog."default",
    "PostalCode" character varying(20) COLLATE pg_catalog."default",
    "Country" character varying(100) COLLATE pg_catalog."default",
    "EmergencyContactName" character varying(150) COLLATE pg_catalog."default",
    "EmergencyContactMobile" character varying(20) COLLATE pg_catalog."default",
    "EmergencyContactRelation" character varying(50) COLLATE pg_catalog."default",
    "AadharNumber" character varying(20) COLLATE pg_catalog."default",
    "PreviousInstitute" character varying(200) COLLATE pg_catalog."default",
    "MedicalNotes" text COLLATE pg_catalog."default",
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT false,
    "DeletedBy" uuid,
    "DeletedAt" timestamp with time zone,
    CONSTRAINT "Students_pkey" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_Students_UserId" UNIQUE ("UserId"),
    CONSTRAINT "FK_Students_Users" FOREIGN KEY ("UserId")
        REFERENCES public."Users" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."Students"
    OWNER to postgres;

-- Indexes
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Students_StudentCode"
    ON public."Students" USING btree
    ("StudentCode" COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default
    WHERE "StudentCode" IS NOT NULL AND "IsDeleted" = false;

CREATE INDEX IF NOT EXISTS "IX_Students_ClassName_Section"
    ON public."Students" USING btree
    ("ClassName" COLLATE pg_catalog."default" ASC NULLS LAST, "Section" COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
