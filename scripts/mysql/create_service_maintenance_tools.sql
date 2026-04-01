CREATE TABLE IF NOT EXISTS service_maintenance_tools (
    id INT NOT NULL AUTO_INCREMENT,
    maintenance_record_id INT NOT NULL,
    description VARCHAR(255) NOT NULL,
    created_date DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    updated_date DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    PRIMARY KEY (id),
    INDEX idx_service_maintenance_tools_maintenance_record_id (maintenance_record_id),
    CONSTRAINT fk_service_maintenance_tools_maintenance_record
        FOREIGN KEY (maintenance_record_id) REFERENCES maintenance_record (id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
