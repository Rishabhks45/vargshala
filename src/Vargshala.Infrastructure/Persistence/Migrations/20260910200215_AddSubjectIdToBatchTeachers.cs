using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vargshala.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectIdToBatchTeachers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Ensure user fields exist
            migrationBuilder.Sql(@"
                ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""PasswordResetToken"" character varying(500);
                ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""PasswordResetTokenExpiresAt"" timestamp with time zone;
                ALTER TABLE ""Users"" ADD COLUMN IF NOT EXISTS ""ProfilePictureUrl"" text;
            ");

            // 2. Add SubjectId to BatchTeachers, backfill from Batches, set NOT NULL, and configure constraints
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'BatchTeachers' AND column_name = 'SubjectId'
                    ) THEN
                        ALTER TABLE ""BatchTeachers"" ADD COLUMN ""SubjectId"" uuid;
                        
                        -- Backfill existing rows from Batches
                        UPDATE ""BatchTeachers"" bt 
                        SET ""SubjectId"" = b.""SubjectId"" 
                        FROM ""Batches"" b 
                        WHERE bt.""BatchId"" = b.""Id"";

                        -- Enforce NOT NULL
                        ALTER TABLE ""BatchTeachers"" ALTER COLUMN ""SubjectId"" SET NOT NULL;
                    END IF;
                END $$;

                -- Drop old unique constraint and index
                ALTER TABLE ""BatchTeachers"" DROP CONSTRAINT IF EXISTS ""UQ_BatchTeachers_BatchId_TeacherId"";
                DROP INDEX IF EXISTS ""UQ_BatchTeachers_BatchId_TeacherId"";

                -- Create new unique index on (BatchId, TeacherId, SubjectId)
                DROP INDEX IF EXISTS ""UQ_BatchTeachers_BatchId_TeacherId_SubjectId"";
                CREATE UNIQUE INDEX ""UQ_BatchTeachers_BatchId_TeacherId_SubjectId"" 
                    ON ""BatchTeachers"" (""BatchId"", ""TeacherId"", ""SubjectId"");

                -- Create index on SubjectId
                DROP INDEX IF EXISTS ""IX_BatchTeachers_SubjectId"";
                CREATE INDEX ""IX_BatchTeachers_SubjectId"" ON ""BatchTeachers"" (""SubjectId"");

                -- Add Foreign Key to Subjects
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'FK_BatchTeachers_Subjects_SubjectId'
                    ) THEN
                        ALTER TABLE ""BatchTeachers"" 
                            ADD CONSTRAINT ""FK_BatchTeachers_Subjects_SubjectId"" 
                            FOREIGN KEY (""SubjectId"") REFERENCES ""Subjects"" (""Id"") ON DELETE RESTRICT;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ""BatchTeachers"" DROP CONSTRAINT IF EXISTS ""FK_BatchTeachers_Subjects_SubjectId"";
                DROP INDEX IF EXISTS ""IX_BatchTeachers_SubjectId"";
                DROP INDEX IF EXISTS ""UQ_BatchTeachers_BatchId_TeacherId_SubjectId"";
                
                -- Restore old unique index
                CREATE UNIQUE INDEX IF NOT EXISTS ""UQ_BatchTeachers_BatchId_TeacherId"" 
                    ON ""BatchTeachers"" (""BatchId"", ""TeacherId"");

                ALTER TABLE ""BatchTeachers"" DROP COLUMN IF EXISTS ""SubjectId"";
            ");
        }
    }
}
