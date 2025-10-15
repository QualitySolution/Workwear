-- Добавление поля для архивации номенклатуры нормы
ALTER TABLE `protection_tools` 
	ADD COLUMN `archival` BOOL NOT NULL DEFAULT FALSE;

-- Черновик документа выдачи
ALTER TABLE `stock_expense`
	ADD `issue_date` DATE NULL AFTER `date`;

UPDATE `stock_expense` SET `issue_date` = `date` WHERE `issue_date` IS NULL;

-- Записи на посещение
CREATE TABLE `visits`
(
	`id`              INT UNSIGNED AUTO_INCREMENT,
	`create_date`     DATETIME              NOT NULL,
	`visit_date`      DATETIME              NOT NULL,
	`employee_id`     INT UNSIGNED          NOT NULL,
	`employee_create` BOOLEAN DEFAULT TRUE  NOT NULL,
	`done`            BOOLEAN DEFAULT FALSE NOT NULL,
	`cancelled`       BOOLEAN DEFAULT FALSE NOT NULL,
	`comment`         TEXT                  NULL,
	CONSTRAINT `visits_pk`
		PRIMARY KEY (`id`),
	CONSTRAINT `visits_employees_id_fk`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE INDEX `visits_create_date_index`
	ON `visits` (`create_date`);
CREATE INDEX `visits_visit_date_index`
	ON `visits` (`visit_date`);


CREATE TABLE `visits_documents`
(
	`id`         INT UNSIGNED AUTO_INCREMENT
        PRIMARY KEY,
	`visit_id`   INT UNSIGNED NOT NULL,
	`expence_id` INT UNSIGNED NULL,
	`writeof_id` INT UNSIGNED NULL,
	`return_id`  INT UNSIGNED NULL,
	CONSTRAINT `visits_documents_stock_expense_id_fk`
		FOREIGN KEY (`expence_id`) REFERENCES `stock_expense` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `visits_documents_stock_return_id_fk`
		FOREIGN KEY (`return_id`) REFERENCES `stock_return` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `visits_documents_stock_write_off_organization_id_fk`
		FOREIGN KEY (`writeof_id`) REFERENCES `stock_write_off` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `visits_documents_visits_id_fk`
		FOREIGN KEY (`visit_id`) REFERENCES `visits` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE
);

-- Учёт дней недели
CREATE TABLE `work_days`
(
	`id`          INT UNSIGNED AUTO_INCREMENT,
	`date` 		DATE 	NOT NULL,
	`is_work_day` BOOLEAN DEFAULT TRUE NOT NULL,
	`comment` 	TEXT	NULL,
	CONSTRAINT `work_days_pk`
		PRIMARY KEY (`id`)
);

-- Добавление параметра для отключения строки нормы
ALTER TABLE `norms_item`
	ADD COLUMN `is_disabled` BOOLEAN DEFAULT FALSE NOT NULL;

-- В прошлом релизе по ошибке выпустили в релиз разную структуру базы для новой и обновлений.
-- Приводим к единой структуре.

ALTER TABLE `shipment`
	MODIFY `start_period` DATE NULL,
	MODIFY `end_period` DATE NULL;

-- Оказываемые услуги
CREATE TABLE `clothing_service_services`
(
	`id`   	INT UNSIGNED AUTO_INCREMENT,
	`name` 	VARCHAR(60)       NOT NULL,
	`cost` 	DECIMAL DEFAULT 0 NOT NULL,
	`code`    VARCHAR(13)       NULL,
	`comment` TEXT       		  NULL,
	CONSTRAINT `clothing_service_services_pk`
		PRIMARY KEY (`id`)
)
	AUTO_INCREMENT = 101;

CREATE TABLE `clothing_service_services_nomenclature`
(
	`id`   			INT UNSIGNED AUTO_INCREMENT,
	`nomenclature_id` INT UNSIGNED NOT NULL,
	`service_id`     	INT UNSIGNED NOT NULL,
	CONSTRAINT `clothing_service_services_pk`
		PRIMARY KEY (`id`),
	CONSTRAINT `fk_services_nomenclature_nomenclature_id`
		FOREIGN KEY (`nomenclature_id`) REFERENCES `nomenclature` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `fk_services_nomenclature_service_id`
		FOREIGN KEY (`service_id`) REFERENCES `clothing_service_services` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE TABLE `clothing_service_services_claim`
(
	`id`         INT UNSIGNED AUTO_INCREMENT,
	`service_id` INT UNSIGNED NULL,
	`claim_id`   INT UNSIGNED,
	CONSTRAINT `clothing_service_services_claim_pk`
		PRIMARY KEY (`id`),
	CONSTRAINT `clothing_service_services_claim_service_id_claim_id_uindex`
		UNIQUE (`service_id`, `claim_id`),
	CONSTRAINT `clothing_service_services_claim_clothing_service_claim_id_fk`
		FOREIGN KEY (`claim_id`) REFERENCES `clothing_service_claim` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `clothing_service_services_claim_clothing_service_services_id_fk`
		FOREIGN KEY (`service_id`) REFERENCES `clothing_service_services` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE
);
