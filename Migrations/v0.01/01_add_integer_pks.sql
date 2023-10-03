-- 1. Add an `id` column to the `roleassignments` table using BIGSERIAL

ALTER TABLE oedauthz.roleassignments
    ADD COLUMN "id" BIGSERIAL PRIMARY KEY;

-- Rearrange sequence to continue from the last ID (optional but good for consistency)
-- Get the max value from the sequence and set the next value from it
DO $$
    DECLARE
        max_id BIGINT;
    BEGIN
        SELECT COALESCE(MAX("id"), 1) INTO max_id FROM oedauthz.roleassignments;
        PERFORM setval(pg_get_serial_sequence('oedauthz.roleassignments', 'id'), max_id);
    END
$$;


-- 2. Change the `id` column in the `roleassignments_log` table to use BIGSERIAL

-- Firstly, drop the existing primary key constraint
ALTER TABLE oedauthz.roleassignments_log
    DROP CONSTRAINT roleassignments_log_pkey;

-- Alter the `id` column type to BIGSERIAL (in reality, to BIGINT as the SERIAL is just a shorthand)
ALTER TABLE oedauthz.roleassignments_log
    ALTER COLUMN "id" TYPE BIGINT;

-- Then, recreate the primary key constraint
ALTER TABLE oedauthz.roleassignments_log
    ADD PRIMARY KEY ("id");

-- Reassociate the sequence to the column
ALTER SEQUENCE oedauthz.roleassignments_log_id_seq OWNED BY oedauthz.roleassignments_log."id";

-- Set the default value for the column to use the sequence
ALTER TABLE oedauthz.roleassignments_log
    ALTER COLUMN "id" SET DEFAULT nextval('oedauthz.roleassignments_log_id_seq');

-- Again, rearrange sequence for the log table (optional but good for consistency)
DO $$
    DECLARE
        max_log_id BIGINT;
    BEGIN
        SELECT COALESCE(MAX("id"), 1) INTO max_log_id FROM oedauthz.roleassignments_log;
        PERFORM setval(pg_get_serial_sequence('oedauthz.roleassignments_log', 'id'), max_log_id);
    END
$$;
