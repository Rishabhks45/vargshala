-- 1. Table create karein
CREATE TABLE IF NOT EXISTS "RazorpaySettings" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "KeyId" VARCHAR(255) NOT NULL,
    "KeySecret" VARCHAR(255) NOT NULL,
    "WebhookSecret" VARCHAR(255) NULL,
    "Currency" VARCHAR(10) NOT NULL DEFAULT 'INR',
    "CompanyName" VARCHAR(255) NOT NULL DEFAULT 'Vargshala',
    "ThemeColor" VARCHAR(20) NOT NULL DEFAULT '#009488',
    "SuccessUrl" VARCHAR(500) NULL,
    "CancelUrl" VARCHAR(500) NULL,
    "UpdatedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 2. Aapki keys insert karein
INSERT INTO "RazorpaySettings" ("Id", "KeyId", "KeySecret", "WebhookSecret", "Currency", "CompanyName", "ThemeColor")
VALUES (
    gen_random_uuid(),
    'rzp_test_Tf0t6zwP0gLdPg',
    'XZuIrvy735yPL4zU8Ka9VSKm',
    '',
    'INR',
    'Vargshala',
    '#009488'
);
