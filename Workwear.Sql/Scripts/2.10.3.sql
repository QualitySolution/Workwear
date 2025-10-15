-- Создание новой функции, которая корректно считает количество, необходимое к выдаче
DELIMITER $$
CREATE FUNCTION `quantity_issue`(
	`amount` INT UNSIGNED,
	`norm_period` INT UNSIGNED,
	`next_issue` DATE,
	`begin_date` DATE,
	`end_date` DATE,
	`begin_Issue_Period` DATE,
	`end_Issue_Period` DATE)
	RETURNS INT(10) UNSIGNED
    NO SQL
    DETERMINISTIC
    COMMENT 'Функция рассчитывает количество, необходимое к выдаче.'
BEGIN
    DECLARE issue_count INT;
    DECLARE next_issue_new DATE;
    DECLARE begin_Issue_Period_New DATE;
    DECLARE end_Issue_Period_New DATE;
    DECLARE start_Of_Year DATE;
    DECLARE end_Of_Year DATE;

    IF norm_period <= 0 THEN RETURN 0; END IF;
    IF next_issue IS NULL THEN RETURN 0; END IF;

    SET issue_count = 0;
    SET next_issue_new = CONCAT('2000', '-', MONTH(next_issue), '-', DAY(next_issue));
    SET begin_Issue_Period_New = CONCAT('2000', '-', MONTH(begin_Issue_Period), '-', DAY(begin_Issue_Period));
    SET end_Issue_Period_New = CONCAT('2000', '-', MONTH(end_Issue_Period), '-', DAY(end_Issue_Period));
    SET start_Of_Year = CONCAT('2000', '-', 1, '-',  1);
    SET end_Of_Year = CONCAT('2000', '-', 12 , '-', 31);

    WHILE next_issue <= end_date DO
            IF begin_Issue_Period IS NOT NULL THEN
                IF (next_issue_new  BETWEEN begin_Issue_Period_New AND end_Issue_Period_New)
                    OR ((next_issue_new BETWEEN begin_Issue_Period_New AND end_Of_Year OR next_issue_new BETWEEN start_Of_Year AND end_Issue_Period_New)
                        AND (MONTH(begin_Issue_Period) > MONTH(end_Issue_Period))) THEN
                    IF next_issue >= begin_date THEN
                        SET issue_count = issue_count + amount;
					END IF;
                    SET next_issue = DATE_ADD(next_issue, INTERVAL norm_period MONTH);
				END IF;
                IF (next_issue_new BETWEEN (DATE_ADD(end_Issue_Period_New, INTERVAL 1 DAY)) AND (DATE_ADD(begin_Issue_Period_New, INTERVAL -1 DAY)))
                    OR ((next_issue_new < begin_Issue_Period_New OR next_issue_new > end_Issue_Period_New) AND (MONTH(begin_Issue_Period) <= MONTH(end_Issue_Period))) THEN
                    SET next_issue = CONCAT(YEAR(next_issue), '-', MONTH(begin_Issue_Period), '-', DAY(begin_Issue_Period));
                    IF (next_issue_new > end_Issue_Period_New) AND (MONTH(begin_Issue_Period) <= MONTH(end_Issue_Period)) THEN
                        SET next_issue = DATE_ADD(next_issue, INTERVAL 1 YEAR);
					END IF;
				END IF;
			ELSE
                IF next_issue >= begin_date THEN
                    SET issue_count = issue_count + amount;
				END IF;
                SET next_issue = DATE_ADD(next_issue, INTERVAL norm_period MONTH);
			END IF;
            SET next_issue_new = CONCAT('2000', '-', MONTH(next_issue), '-', DAY(next_issue));
	END WHILE;
	RETURN issue_count;
END$$

DELIMITER ;

-- Исключение выходных дней
DELIMITER $$
CREATE FUNCTION `count_working_days` (`start_date` DATE, `end_date` DATE)
	RETURNS INT
	DETERMINISTIC
	COMMENT 'Функция подсчитывает количество дней нахождения спецодежды на каждом этапе, исключая выходные дни'
BEGIN
RETURN (WITH RECURSIVE date_range AS
						   (SELECT start_date as sd
							UNION ALL
							SELECT DATE_ADD(sd, INTERVAL 1 Day)
							FROM date_range
							WHERE DATE_ADD(sd, INTERVAL 1 Day) < end_date
						   )
		SELECT COUNT(*)
		FROM date_range
		WHERE WEEKDAY(sd) NOT IN (5, 6) AND start_date < end_date
);
END $$;
DELIMITER ;

-- Добавление дополнительной информации в номенклатуру
ALTER TABLE `nomenclature`
	ADD COLUMN `additional_info` TEXT NULL DEFAULT NULL AFTER `number`;

-- ------------------------------------------------------
-- Запрет смены размера для потребности в личном кабинете
-- ------------------------------------------------------
ALTER TABLE `protection_tools`
	ADD `size_change` INT NULL  COMMENT 'null - не ограничиваем, 0 - не даём всегда, остальное - число дней до выдачи, когда нужно ограничивать редактирования типа размера этого объекта' 
		AFTER `dispenser`;

-- Новое расписание работы склада
CREATE TABLE `days_schedule` (
	   `id`   	     	INT UNSIGNED AUTO_INCREMENT,
	   `date` 		 	DATE NULL 			COMMENT 'Если указана дата, то расписание действует только на эту дату',
	   `day_of_week` 	INT UNSIGNED NULL 	COMMENT 'Если указано, то расписание действует на этот день недели (1-Пн, 2-Вт, ..., 7-Вс)',
	   `start` 			TIME NULL 			COMMENT 'Время начала рабочего дня, если null, то день нерабочий',
	   `end` 		    TIME NULL 			COMMENT 'Время окончания рабочего дня, если null, то день нерабочий',
	   `visit_interval` INT UNSIGNED NULL 	COMMENT 'Интервал между записями на приём в минутах, если null, то день нерабочий',
	   `comment` 	 	TEXT NULL,
	   CONSTRAINT `days_schedule`
		   PRIMARY KEY (`id`)
);

-- Добавление поля для уведомлений о просроченных вещах
ALTER TABLE `postomat_document_items`
	ADD COLUMN `notification_sent` BOOLEAN NOT NULL DEFAULT FALSE;

-- Создаем пропущенные индексы для снижения нагрузки на ЦП сервиса постаматов
ALTER TABLE `clothing_service_states` ADD INDEX(`operation_time`);

-- ---------------------------------------------
-- Каталог, выбор пользователем предпочтительных номенклатур
-- ---------------------------------------------
ALTER TABLE `nomenclature`
	ADD `catalog_id` CHAR(24) NULL AFTER `archival`;

ALTER TABLE `protection_tools_nomenclature`
	ADD `can_choose` BOOLEAN DEFAULT FALSE NOT NULL;

CREATE TABLE `employees_selected_nomenclatures`
(
	`id`                  INT UNSIGNED AUTO_INCREMENT,
	`employee_id`         INT UNSIGNED NOT NULL,
	`protection_tools_id` INT UNSIGNED NOT NULL,
	`nomenclature_id`     INT UNSIGNED NOT NULL,
	CONSTRAINT `employees_selected_nomenclatures_pk`
		PRIMARY KEY (`id`),
	CONSTRAINT `employees_selected_nomenclatures_employees_id_fk`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `employees_selected_nomenclatures_nomenclature_id_fk`
		FOREIGN KEY (`nomenclature_id`) REFERENCES `nomenclature` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `employees_selected_nomenclatures_protection_tools_id_fk`
		FOREIGN KEY (`protection_tools_id`) REFERENCES `protection_tools` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE
)
	COMMENT 'Номенклатуры выбранные пользователем, как предпочтительные к выдаче';

-- Добавление операции выдачи по дежурной норме в ведомость
ALTER TABLE `issuance_sheet_items` 
	ADD COLUMN `duty_norm_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `issued_operation_id`;

ALTER TABLE `issuance_sheet_items`
	ADD CONSTRAINT `fk_issuance_sheet_items_duty_norm_issue_operation_id`
		FOREIGN KEY (`duty_norm_issue_operation_id`) REFERENCES `operation_issued_by_duty_norm`(`id`)
			ON UPDATE CASCADE 
			ON DELETE NO ACTION;

CREATE INDEX `fk_issuance_sheet_items_duty_norm_issue_operation_idx` 
	ON `issuance_sheet_items`(`duty_norm_issue_operation_id` ASC);

-- Добавление операций выдачи по дежурной норме в операции со штрихкодами
ALTER TABLE `operation_barcodes`
	ADD COLUMN `duty_norm_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `employee_issue_operation_id`;

ALTER TABLE `operation_barcodes`
	ADD CONSTRAINT `fk_operation_barcodes_duty_norm_issue_operation_id`
		FOREIGN KEY (`duty_norm_issue_operation_id`) REFERENCES `operation_issued_by_duty_norm`(`id`)
			ON UPDATE CASCADE
			ON DELETE NO ACTION;

CREATE INDEX `fk_operation_barcodes_duty_norm_issue_operation_id_idx`
	ON `operation_barcodes`(`duty_norm_issue_operation_id` ASC);

-- Подсветка в журнале сотрудников по подразделению
ALTER TABLE `subdivisions`
	ADD `employees_color` VARCHAR(7) NULL;
