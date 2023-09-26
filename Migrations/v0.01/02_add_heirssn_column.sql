-- Add a nullable "heirSsn" field to roleassignments
DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'oedauthz' AND table_name = 'roleassignments' AND column_name = 'heirSsn') THEN
            ALTER TABLE oedauthz.roleassignments ADD COLUMN "heirSsn" character(11) COLLATE pg_catalog."default";
        END IF;
    END $$;

-- Update the constraint to include "heirSsn"
DO $$
    BEGIN

        -- Check if constraint exists
        IF EXISTS (
            SELECT 1
            FROM information_schema.table_constraints
            WHERE table_schema = 'oedauthz'
              AND table_name = 'roleassignments'
              AND constraint_name = 'constraint_roleassignment'
        ) THEN
            -- Drop the constraint
            ALTER TABLE oedauthz.roleassignments DROP CONSTRAINT constraint_roleassignment;
        END IF;

        -- Add a new constraint that includes the heirSsn field
        ALTER TABLE oedauthz.roleassignments ADD CONSTRAINT constraint_roleassignment UNIQUE ("estateSsn", "recipientSsn", "roleCode", "heirSsn");

    END $$;


-- Add a different index to handle heirSsn being null, that is constrain the uniqueness of collective role assigments
DO $$
    BEGIN

        -- Check if the index exists
        IF NOT EXISTS (
            SELECT 1
            FROM pg_indexes
            WHERE schemaname = 'oedauthz'
              AND tablename = 'roleassignments'
              AND indexname = 'idx_unique_collective_roleassignments'
        ) THEN
            -- Create the index
            CREATE UNIQUE INDEX idx_unique_collective_roleassignments
                ON oedauthz.roleassignments ("estateSsn", "recipientSsn", "roleCode")
                WHERE "heirSsn" IS NULL;
        END IF;

    END $$;

-- Add a nullable "heirSsn" field to roleassignments_log
DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'oedauthz' AND table_name = 'roleassignments_log' AND column_name = 'heirSsn') THEN
            ALTER TABLE oedauthz.roleassignments_log ADD COLUMN "heirSsn" character(11) COLLATE pg_catalog."default";
        END IF;
    END $$;

-- Update the function log_roleassignments_changes() to log "heirSsn"
-- Drop dependent triggers only if they exist
DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'roleassignments_insert_log') THEN
            DROP TRIGGER roleassignments_insert_log ON oedauthz.roleassignments;
        END IF;

        IF EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'roleassignments_delete_log') THEN
            DROP TRIGGER roleassignments_delete_log ON oedauthz.roleassignments;
        END IF;
    END $$;

-- Modify the function to log "heirSsn"
-- This part is already idempotent due to "CREATE OR REPLACE"
CREATE OR REPLACE FUNCTION oedauthz.log_roleassignments_changes()
    RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO oedauthz.roleassignments_log ("estateSsn", "recipientSsn", "roleCode", "heirSsn", "action", "timestamp")
        VALUES (NEW."estateSsn", NEW."recipientSsn", NEW."roleCode", NEW."heirSsn", 'GRANT', NOW());
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO oedauthz.roleassignments_log ("estateSsn", "recipientSsn", "roleCode", "heirSsn", "action", "timestamp")
        VALUES (OLD."estateSsn", OLD."recipientSsn", OLD."roleCode", OLD."heirSsn", 'REVOKE', NOW());
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Recreate the triggers
-- This part is already idempotent, because if the triggers exist, they will just be replaced with the same triggers.
CREATE TRIGGER roleassignments_insert_log
    AFTER INSERT ON oedauthz.roleassignments
    FOR EACH ROW
EXECUTE FUNCTION oedauthz.log_roleassignments_changes();

CREATE TRIGGER roleassignments_delete_log
    AFTER DELETE ON oedauthz.roleassignments
    FOR EACH ROW
EXECUTE FUNCTION oedauthz.log_roleassignments_changes();
