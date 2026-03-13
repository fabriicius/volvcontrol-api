CREATE TABLE IF NOT EXISTS status_service_request (
    id INT NOT NULL AUTO_INCREMENT,
    description VARCHAR(120) NOT NULL,
    created_date DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_date DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    UNIQUE KEY uk_status_service_request_description (description)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO status_service_request (id, description, created_date, updated_date)
VALUES
    (1, 'Aberto', UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    (2, 'Em andamento', UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    (3, 'Concluido', UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)),
    (4, 'Cancelado', UTC_TIMESTAMP(6), UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE
    description = VALUES(description),
    updated_date = VALUES(updated_date);

SET @old_fk_name = (
    SELECT kcu.CONSTRAINT_NAME
    FROM information_schema.KEY_COLUMN_USAGE kcu
    WHERE kcu.TABLE_SCHEMA = DATABASE()
      AND kcu.TABLE_NAME = 'service_request'
      AND kcu.COLUMN_NAME = 'status_maintenance_record_id'
      AND kcu.REFERENCED_TABLE_NAME IS NOT NULL
    LIMIT 1
);

SET @drop_old_fk_sql = IF(
    @old_fk_name IS NULL,
    'SELECT 1',
    CONCAT('ALTER TABLE service_request DROP FOREIGN KEY ', @old_fk_name)
);
PREPARE stmt_drop_old_fk FROM @drop_old_fk_sql;
EXECUTE stmt_drop_old_fk;
DEALLOCATE PREPARE stmt_drop_old_fk;

ALTER TABLE service_request
    DROP COLUMN IF EXISTS status_maintenance_record_id;

ALTER TABLE service_request
    ADD COLUMN IF NOT EXISTS status_service_request_id INT NOT NULL AFTER user_id;

SET @new_index_exists = (
    SELECT COUNT(1)
    FROM information_schema.STATISTICS s
    WHERE s.TABLE_SCHEMA = DATABASE()
      AND s.TABLE_NAME = 'service_request'
      AND s.INDEX_NAME = 'idx_service_request_status_service_request_id'
);

SET @add_new_index_sql = IF(
    @new_index_exists = 0,
    'ALTER TABLE service_request ADD INDEX idx_service_request_status_service_request_id (status_service_request_id)',
    'SELECT 1'
);
PREPARE stmt_add_new_index FROM @add_new_index_sql;
EXECUTE stmt_add_new_index;
DEALLOCATE PREPARE stmt_add_new_index;

SET @new_fk_exists = (
    SELECT COUNT(1)
    FROM information_schema.TABLE_CONSTRAINTS tc
    WHERE tc.TABLE_SCHEMA = DATABASE()
      AND tc.TABLE_NAME = 'service_request'
      AND tc.CONSTRAINT_TYPE = 'FOREIGN KEY'
      AND tc.CONSTRAINT_NAME = 'fk_service_request_status_service_request'
);

SET @add_new_fk_sql = IF(
    @new_fk_exists = 0,
    'ALTER TABLE service_request ADD CONSTRAINT fk_service_request_status_service_request FOREIGN KEY (status_service_request_id) REFERENCES status_service_request (id)',
    'SELECT 1'
);
PREPARE stmt_add_new_fk FROM @add_new_fk_sql;
EXECUTE stmt_add_new_fk;
DEALLOCATE PREPARE stmt_add_new_fk;

SET @request_number_unique_exists = (
    SELECT COUNT(1)
    FROM information_schema.STATISTICS s
    WHERE s.TABLE_SCHEMA = DATABASE()
      AND s.TABLE_NAME = 'service_request'
      AND s.INDEX_NAME = 'uk_service_request_request_number'
);

SET @add_request_number_unique_sql = IF(
    @request_number_unique_exists = 0,
    'ALTER TABLE service_request ADD CONSTRAINT uk_service_request_request_number UNIQUE (request_number)',
    'SELECT 1'
);
PREPARE stmt_add_request_number_unique FROM @add_request_number_unique_sql;
EXECUTE stmt_add_request_number_unique;
DEALLOCATE PREPARE stmt_add_request_number_unique;
