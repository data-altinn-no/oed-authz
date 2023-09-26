-- 1. Change the SERIAL field to BIGSERIAL in roleassignments.
ALTER TABLE oedauthz.roleassignments ALTER COLUMN "id" TYPE BIGINT;
ALTER SEQUENCE oedauthz.roleassignments_id_seq AS BIGINT;

-- 2. Change the SERIAL field to BIGSERIAL in roleassignments_log.
ALTER TABLE oedauthz.roleassignments_log ALTER COLUMN "id" TYPE BIGINT;
ALTER SEQUENCE oedauthz.roleassignments_log_id_seq AS BIGINT;
