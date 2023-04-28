-- Create schema
CREATE SCHEMA IF NOT EXISTS oedauthz;

-- Create the ENUM type for the "action" column
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'roleassignments_action') THEN
        CREATE TYPE oedauthz.roleassignments_action AS ENUM ('GRANT', 'REVOKE');
    END IF;
END
$$;

-- Create the roleassignments_log table
CREATE TABLE IF NOT EXISTS oedauthz.roleassignments_log
(
    "id" SERIAL PRIMARY KEY,
    "estateSsn" character(11) COLLATE pg_catalog."default" NOT NULL,
    "recipientSsn" character(11) COLLATE pg_catalog."default" NOT NULL,
    "roleCode" character varying(60) COLLATE pg_catalog."default" NOT NULL,
    "action" oedauthz.roleassignments_action NOT NULL,
    "timestamp" timestamp with time zone NOT NULL
);

-- Create the function that will be called by the triggers
CREATE OR REPLACE FUNCTION oedauthz.log_roleassignments_changes()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO oedauthz.roleassignments_log ("estateSsn", "recipientSsn", "roleCode", "action", "timestamp")
        VALUES (NEW."estateSsn", NEW."recipientSsn", NEW."roleCode", 'GRANT', NOW());
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO oedauthz.roleassignments_log ("estateSsn", "recipientSsn", "roleCode", "action", "timestamp")
        VALUES (OLD."estateSsn", OLD."recipientSsn", OLD."roleCode", 'REVOKE', NOW());
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Create the triggers
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'roleassignments_insert_log') THEN
        CREATE TRIGGER roleassignments_insert_log
        AFTER INSERT ON oedauthz.roleassignments
        FOR EACH ROW
        EXECUTE FUNCTION oedauthz.log_roleassignments_changes();
    END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'roleassignments_delete_log') THEN
        CREATE TRIGGER roleassignments_delete_log
        AFTER DELETE ON oedauthz.roleassignments
        FOR EACH ROW
        EXECUTE FUNCTION oedauthz.log_roleassignments_changes();
    END IF;
END
$$;


-- Create indices for estateSsn, recipientSsn, and timestamp
CREATE INDEX IF NOT EXISTS idx_roleassignments_log_estateSsn ON oedauthz.roleassignments_log ("estateSsn");
CREATE INDEX IF NOT EXISTS idx_roleassignments_log_recipientSsn ON oedauthz.roleassignments_log ("recipientSsn");
CREATE INDEX IF NOT EXISTS idx_roleassignments_log_timestamp ON oedauthz.roleassignments_log ("timestamp");


-- Create the role assignments table
CREATE TABLE IF NOT EXISTS oedauthz.roleassignments
(
    "estateSsn" character(11) COLLATE pg_catalog."default" NOT NULL,
    "recipientSsn" character(11) COLLATE pg_catalog."default" NOT NULL,
    "roleCode" character varying(60) COLLATE pg_catalog."default" NOT NULL,
    created timestamp with time zone NOT NULL,
    CONSTRAINT constraint_roleassignment UNIQUE ("estateSsn", "recipientSsn", "roleCode")
);

-- Create indices
CREATE INDEX IF NOT EXISTS idx_created ON oedauthz.roleassignments ("created");
CREATE INDEX IF NOT EXISTS idx_estateSsn ON oedauthz.roleassignments ("estateSsn");
CREATE INDEX IF NOT EXISTS "idx_recipientSsn" ON oedauthz.roleassignments ("recipientSsn");
CREATE INDEX IF NOT EXISTS "idx_roleCode" ON oedauthz.roleassignments ("roleCode");

-- Create triggers for log table
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'roleassignments_insert_log') THEN
        CREATE TRIGGER roleassignments_insert_log
        AFTER INSERT ON oedauthz.roleassignments
        FOR EACH ROW
        EXECUTE FUNCTION oedauthz.log_roleassignments_changes();
    END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'roleassignments_delete_log') THEN
        CREATE TRIGGER roleassignments_delete_log
        AFTER DELETE ON oedauthz.roleassignments
        FOR EACH ROW
        EXECUTE FUNCTION oedauthz.log_roleassignments_changes();
    END IF;
END
$$;
