-- 1. Create MessageReactions Table
CREATE TABLE IF NOT EXISTS "MessageReactions" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "OrganizationId" UUID NOT NULL,
    "MessageId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "Emoji" VARCHAR(32) NOT NULL,
    "ReactedAt" TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_MessageReactions_Organizations_OrganizationId" 
        FOREIGN KEY ("OrganizationId") REFERENCES "Organizations" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MessageReactions_Messages_MessageId" 
        FOREIGN KEY ("MessageId") REFERENCES "Messages" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_MessageReactions_Users_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "UQ_MessageReactions_MessageId_UserId" 
        UNIQUE ("MessageId", "UserId")
);

-- 2. Lookup & Tenant Indexes
CREATE INDEX IF NOT EXISTS "IX_MessageReactions_OrganizationId" ON "MessageReactions" ("OrganizationId");
CREATE INDEX IF NOT EXISTS "IX_MessageReactions_MessageId" ON "MessageReactions" ("MessageId");
CREATE INDEX IF NOT EXISTS "IX_MessageReactions_UserId" ON "MessageReactions" ("UserId");
