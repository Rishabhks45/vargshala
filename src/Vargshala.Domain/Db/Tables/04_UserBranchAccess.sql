-- Table: public.UserBranchAccess

-- DROP TABLE IF EXISTS public."UserBranchAccess";

CREATE TABLE IF NOT EXISTS public."UserBranchAccess"
(
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "BranchId" uuid NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedBy" uuid,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedBy" uuid,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_UserBranchAccess" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_UserBranchAccess_UserId_BranchId" UNIQUE ("UserId", "BranchId"),
    CONSTRAINT "FK_UserBranchAccess_Users_UserId" FOREIGN KEY ("UserId")
        REFERENCES public."Users" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT "FK_UserBranchAccess_Branches_BranchId" FOREIGN KEY ("BranchId")
        REFERENCES public."Branches" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
TABLESPACE pg_default;

ALTER TABLE IF EXISTS public."UserBranchAccess"
    OWNER to postgres;

-- Indexes
CREATE INDEX IF NOT EXISTS "IX_UserBranchAccess_UserId"
    ON public."UserBranchAccess" USING btree
    ("UserId" ASC NULLS LAST)
    TABLESPACE pg_default;

CREATE INDEX IF NOT EXISTS "IX_UserBranchAccess_BranchId"
    ON public."UserBranchAccess" USING btree
    ("BranchId" ASC NULLS LAST)
    TABLESPACE pg_default;
