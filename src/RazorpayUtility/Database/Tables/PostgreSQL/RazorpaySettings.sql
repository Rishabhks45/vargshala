-- 2. Aapki keys insert ya update karein (Idempotent Upsert)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM "RazorpaySettings") THEN
        UPDATE "RazorpaySettings"
        SET "KeyId" = 'rzp_test_Tf0t6zwP0gLdPg',
            "KeySecret" = 'XZuIrvy735yPL4zU8Ka9VSKm',
            "WebhookSecret" = 'VargshalaSecret@2026',
            "Currency" = 'INR',
            "CompanyName" = 'Vargshala',
            "ThemeColor" = '#009488',
            "UpdatedAt" = CURRENT_TIMESTAMP;
    ELSE
        INSERT INTO "RazorpaySettings" ("Id", "KeyId", "KeySecret", "WebhookSecret", "Currency", "CompanyName", "ThemeColor", "UpdatedAt")
        VALUES (
            gen_random_uuid(),
            'rzp_test_Tf0t6zwP0gLdPg',
            'XZuIrvy735yPL4zU8Ka9VSKm',
            'VargshalaSecret@2026',
            'INR',
            'Vargshala',
            '#009488',
            CURRENT_TIMESTAMP
        );
    END IF;
END $$;