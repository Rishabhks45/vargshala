-- Table: public.FeeDiscounts

-- DROP TABLE IF EXISTS public."FeeDiscounts";

CREATE TABLE IF NOT EXISTS public."FeeDiscounts"
(
    "Id" UUID NOT NULL DEFAULT gen_random_uuid(),
    "StudentFeeId" UUID NOT NULL,

    "DiscountType" VARCHAR(50) NOT NULL,
    "Value" NUMERIC(12, 2) NOT NULL,
    "Amount" NUMERIC(12, 2) NOT NULL,
    "Reason" VARCHAR(255),
    "ApprovedBy" UUID,
    "ApprovedAt" TIMESTAMPTZ DEFAULT NOW(),

    "CreatedBy" UUID,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT "PK_FeeDiscounts"
        PRIMARY KEY ("Id"),

    CONSTRAINT "FK_FeeDiscounts_StudentFees_StudentFeeId"
        FOREIGN KEY ("StudentFeeId")
        REFERENCES public."StudentFees" ("Id")
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT "FK_FeeDiscounts_Users_ApprovedBy"
        FOREIGN KEY ("ApprovedBy")
        REFERENCES public."Users" ("Id")
        ON UPDATE CASCADE
        ON DELETE SET NULL,

    CONSTRAINT "CK_FeeDiscounts_Amount"
        CHECK ("Amount" >= 0),

    CONSTRAINT "CK_FeeDiscounts_Value"
        CHECK ("Value" >= 0)
);

CREATE INDEX IF NOT EXISTS "IX_FeeDiscounts_StudentFeeId"
    ON public."FeeDiscounts" ("StudentFeeId");

CREATE INDEX IF NOT EXISTS "IX_FeeDiscounts_ApprovedBy"
    ON public."FeeDiscounts" ("ApprovedBy");
