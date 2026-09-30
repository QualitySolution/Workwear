-- Альтернативное наименование номенклатуры нормы для отображения на лицевой карточке сотрудника.
ALTER TABLE protection_tools
    ADD COLUMN `official_name` VARCHAR(800) NULL DEFAULT NULL COMMENT 'Наименование, отображаемое на лицевой карточке сотрудника, если заполнено'
        AFTER name;

-- Разрешаем приём на обслуживание спецодежды, числящейся не на сотруднике.
ALTER TABLE clothing_service_claim
	MODIFY employee_id int UNSIGNED NULL;

-- Срок ранней выдачи разделён по типам выдачи. Коллективным выдачам переносим прежнее общее значение.
INSERT IGNORE INTO base_parameters (name, str_value)
SELECT 'ColDayAheadOfSheduleCollective', str_value FROM base_parameters WHERE name = 'ColDayAheadOfShedule';
