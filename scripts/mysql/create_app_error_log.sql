CREATE TABLE IF NOT EXISTS app_error_log (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    occurred_at DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    repository VARCHAR(150) NOT NULL,
    method VARCHAR(150) NOT NULL,
    error_type VARCHAR(255) NOT NULL,
    message TEXT NOT NULL,
    stack_trace LONGTEXT NULL,
    inner_exception LONGTEXT NULL,
    PRIMARY KEY (id),
    INDEX idx_app_error_log_occurred_at (occurred_at),
    INDEX idx_app_error_log_repository_method (repository, method)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
