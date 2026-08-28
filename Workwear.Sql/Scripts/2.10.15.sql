-- Альтернативное наименование номенклатуры нормы для отображения на лицевой карточке сотрудника.
ALTER TABLE protection_tools
    ADD COLUMN alternative_name VARCHAR(800) NULL DEFAULT NULL
        AFTER name;
