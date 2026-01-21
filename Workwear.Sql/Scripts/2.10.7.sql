-- Для выбора сотрудником предпочтительной к выдаче номенклатуры
ALTER TABLE protection_tools_nomenclature
DROP PRIMARY KEY,
    ADD COLUMN id INT UNSIGNED NOT NULL AUTO_INCREMENT FIRST,
    ADD PRIMARY KEY (id),
    ADD UNIQUE INDEX uk_protection_tools_nomenclature (protection_tools_id, nomenclature_id);
