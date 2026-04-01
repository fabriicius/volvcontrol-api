ALTER TABLE photos_maintenance
    ADD COLUMN photo_moment VARCHAR(10) NOT NULL DEFAULT 'before' AFTER description;

ALTER TABLE photos_maintenance
    ADD INDEX idx_photos_maintenance_photo_moment (photo_moment);
