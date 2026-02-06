-- Для выбора сотрудником предпочтительной к выдаче номенклатуры
ALTER TABLE protection_tools_nomenclature
DROP PRIMARY KEY,
    ADD COLUMN id INT UNSIGNED NOT NULL AUTO_INCREMENT FIRST,
    ADD PRIMARY KEY (id),
    ADD UNIQUE INDEX `unique_protection_tools_nomenclature` (protection_tools_id, nomenclature_id);
-- Добавление архивирования дежурной нормы
ALTER TABLE duty_norms ADD COLUMN archival TINYINT(1) NOT NULL DEFAULT 0 AFTER dateto;
