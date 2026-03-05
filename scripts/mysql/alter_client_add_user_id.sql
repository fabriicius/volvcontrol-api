ALTER TABLE client
    ADD COLUMN user_id BIGINT UNSIGNED NULL AFTER id;

UPDATE client
SET user_id = 2
WHERE user_id IS NULL;

ALTER TABLE client
    ADD INDEX idx_client_user_id (user_id);

ALTER TABLE client
    ADD CONSTRAINT fk_client_user
    FOREIGN KEY (user_id) REFERENCES users (id);
