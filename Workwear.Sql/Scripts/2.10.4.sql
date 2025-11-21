-- Остатки исправление в расхождениях миграций после обновления на 2.9 и по сути переименование внешних ключей
alter table employee_group_items
	add constraint `employee_groups_items_employees_fk`
		foreign key (employee_id) references employees (id)
			on update cascade on delete cascade;

alter table employee_group_items
	add constraint `employee_groups_items_employee_groups_fk`
		foreign key (employee_group_id) references employee_groups (id)
			on update cascade on delete cascade;

-- Исправления расхождения миграций для версии 2.3
ALTER TABLE stock_expense
	MODIFY COLUMN `warehouse_id` INT UNSIGNED NOT NULL DEFAULT 1;

-- Исправление расхождений COLLATION на MySQL
ALTER TABLE `base_parameters`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `causes_write_off`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `clothing_service_services`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `clothing_service_services_claim`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `clothing_service_services_nomenclature`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `days_schedule`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `duty_norm_items`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `duty_norms`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `employee_cards_item`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `employee_group_items`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `employee_groups`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `employees`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `employees_cost_allocation`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `employees_norms`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `employees_selected_nomenclatures`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `employees_sizes`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `employees_vacations`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `issuance_sheet`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `item_types`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `leaders`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `norms`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `norms_item`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `norms_posts`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `operation_issued_by_duty_norm`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `organizations`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `posts`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `protection_tools_category_for_analytics`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `read_news`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `regulations`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `regulations_annex`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `shipment`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `shipment_items`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `stock_expense_duty_norm_items`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `stock_income_items`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `stock_inspection`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `stock_inspection_items`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `stock_inspection_members`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `stock_return`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `stock_return_items`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `stock_transfer`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `stock_transfer_detail`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `stock_write_off`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `stock_write_off_members`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `subdivisions`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `user_settings`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `vacation_type`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `visits_documents`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `warehouse`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `work_days`
	CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

-- Этим колонкам нужно заново установить TEXT, потому что они были в utf8mb3, и по этому при конвертации в utf8mb4 они превращаются в MEDIUMTEXT чтобы сохранить количество символов, но нам это не надо.
ALTER TABLE `norms`
	MODIFY COLUMN `comment` TEXT NULL DEFAULT NULL;

ALTER TABLE `read_news`
	MODIFY COLUMN `items` TEXT NULL DEFAULT NULL;

ALTER TABLE `stock_write_off`
	MODIFY COLUMN `comment` TEXT NULL DEFAULT NULL;

ALTER TABLE `vacation_type`
	MODIFY COLUMN `comment` TEXT NULL DEFAULT NULL;

-- -----------------------------------------------------
-- Пересоздание функций с новыми правами
-- -----------------------------------------------------
DROP FUNCTION count_issue;

DELIMITER $$
CREATE FUNCTION `count_issue`(
    `amount` INT UNSIGNED, 
    `norm_period` INT UNSIGNED, 
    `next_issue` DATE, 
    `begin_date` DATE, 
    `end_date` DATE,
	`begin_Issue_Period` INT,
	`end_Issue_Period` INT) 
    RETURNS int(10) unsigned
    NO SQL
    DETERMINISTIC
    COMMENT 'Функция рассчитывает количество необходимое к выдачи.'
SQL SECURITY INVOKER	
BEGIN
DECLARE issue_count INT;

IF norm_period <= 0 THEN RETURN 0; END IF;
IF next_issue IS NULL THEN RETURN 0; END IF;

SET issue_count = 0;

WHILE next_issue <= end_date DO
    IF next_issue >= begin_date THEN 
    	SET issue_count = issue_count + amount;
    END IF;
  SET next_issue = DATE_ADD(next_issue, INTERVAL norm_period MONTH);
	IF begin_Issue_Period IS NOT NULL THEN
		IF begin_Issue_Period < end_Issue_Period THEN
			IF MONTH(next_issue) BETWEEN begin_Issue_Period AND end_Issue_Period THEN 
             SET next_issue = DATE_ADD(CONCAT(YEAR(next_issue) , '-', end_Issue_Period, '-01'), INTERVAL 1 MONTH);
            END IF; 
        ELSE
			IF MONTH(next_issue) BETWEEN begin_Issue_Period AND 12 THEN
				SET next_issue = DATE_ADD(CONCAT(YEAR(next_issue), '-', end_Issue_Period, '-01'), INTERVAL '1T1' YEAR_MONTH);
			ELSEIF MONTH(next_issue) BETWEEN 1 AND end_Issue_Period THEN
				SET next_issue = DATE_ADD(CONCAT(YEAR(next_issue), '-', end_Issue_Period, '-01'), INTERVAL 1 MONTH);
			END IF;
		END IF;
	END IF;
END WHILE;
RETURN issue_count;
END$$

DELIMITER ;

DROP FUNCTION quantity_issue;

DELIMITER $$
CREATE FUNCTION `quantity_issue`(
	`amount` INT UNSIGNED,
	`norm_period` INT UNSIGNED,
	`next_issue` DATE,
	`begin_date` DATE,
	`end_date` DATE,
	`begin_Issue_Period` DATE,
	`end_Issue_Period` DATE)
	RETURNS int(10) unsigned
    NO SQL
    DETERMINISTIC
    COMMENT 'Функция рассчитывает количество, необходимое к выдаче.'
SQL SECURITY INVOKER
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

DROP FUNCTION count_working_days;

DELIMITER $$
CREATE FUNCTION `count_working_days` (`start_date` DATE, `end_date` DATE)
	RETURNS INT
	DETERMINISTIC
	COMMENT 'Функция подсчитывает количество дней нахождения спецодежды на каждом этапе, исключая выходные дни'
SQL SECURITY INVOKER
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
END$$
DELIMITER ;

-- информация о окнах
CREATE TABLE visit_windows
(
    id   INT UNSIGNED AUTO_INCREMENT
        PRIMARY KEY,
    name CHAR(32) NOT NULL
)
    CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci
    COMMENT 'информация о окнах';

-- изменяем записи посещений на новый формат
ALTER TABLE `visits`
    ADD COLUMN `service_type` ENUM('GiveWear','NewEmployee','Unidentified','Dismiss','GiveReport','WriteOff','ClothingService','Appeal') NOT NULL DEFAULT 'GiveWear' AFTER `employee_id`,
    ADD COLUMN `create_from_lk` TINYINT(1) NOT NULL DEFAULT 1 AFTER `employee_create`,
    ADD COLUMN `status` ENUM('New','Queued','Coming','Serviced','Done','Canceled','Missing') NOT NULL DEFAULT 'New' AFTER `done`,
    ADD COLUMN `ticket_number` CHAR(4) NULL DEFAULT NULL COMMENT 'Талончик в очереди' AFTER `status`,
    ADD COLUMN `window_id` INT(10) UNSIGNED DEFAULT NULL COMMENT 'ID окна обслуживания' AFTER `ticket_number`,
    ADD COLUMN `time_entry` DATETIME DEFAULT NULL COMMENT 'Время постановки в очередь на ПВ' AFTER `window_id`,
    ADD COLUMN `time_start` DATETIME DEFAULT NULL COMMENT 'Начало обслуживания (перво посещение окна)' AFTER `time_entry`,
    ADD COLUMN `time_finish` DATETIME DEFAULT NULL COMMENT 'Завершение визита' AFTER `time_start`,
    CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

-- синхронизируем значениями из employee_create для уже существующих записей
UPDATE `visits` SET `create_from_lk` = `employee_create`;

ALTER TABLE `visits` ADD INDEX `fk_visits_window_id_idx` (`window_id`);

ALTER TABLE `visits` ADD CONSTRAINT `fk_visits_window_id` FOREIGN KEY (`window_id`) REFERENCES `visit_windows` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;


-- записи какой юзер в каком окне, состояние окна
CREATE TABLE visits_users_log
(
    id        INT UNSIGNED AUTO_INCREMENT
        PRIMARY KEY,
    user_id   INT UNSIGNED                                                                        NOT NULL,
    window_id INT UNSIGNED                                                                        NULL,
    visit_id  INT UNSIGNED                                                                        NULL,
    ticket    CHAR(4)                                                                             NULL,
    `time`    DATETIME                                                                            NULL,
    `type`    ENUM ('WindowStart', 'WindowFinish', 'WindowTimeout', 'StartService', 'FinishService', 'ReRouteService', 'WindowWaiting') NOT NULL COMMENT 'Типы действия',
    comment   CHAR(64)                                                                            NULL,
    CONSTRAINT visits_user_users_id_fk
        FOREIGN KEY (user_id) REFERENCES users (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT visits_user_visit_windows_id_fk
        FOREIGN KEY (window_id) REFERENCES visit_windows (id)
            ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT visits_users_log_visits_id_fk
        FOREIGN KEY (visit_id) REFERENCES visits (id)
)
    CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci
    COMMENT 'записи какой юзер в каком окне, состояние окна';

CREATE INDEX visits_user_users_id_fk_idx ON visits_users_log (user_id);
CREATE INDEX visits_user_visit_windows_id_fk_idx ON visits_users_log (window_id);
CREATE INDEX visits_users_log_visits_id_fk_idx ON visits_users_log (visit_id);

-- Заявка на выдачу
CREATE TABLE issuance_requests(
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`receipt_date` DATE NOT NULL,
	`status` ENUM('New', 'Issued', 'PartiallyIssued') DEFAULT 'New' NOT NULL,
	`comment` TEXT NULL,
	`user_id` INT UNSIGNED NULL,
	`creation_date` DATETIME NULL DEFAULT NULL,
	PRIMARY KEY (`id`),
	CONSTRAINT `fk_issuance_request_user_id` FOREIGN KEY (`user_id`) REFERENCES users (`id`)
    	ON DELETE NO ACTION 
		ON UPDATE CASCADE,
	INDEX `issuance_request_user_id_idx` (`user_id` ASC) 
)
CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

-- Сотрудники в заявках на выдачу
CREATE TABLE employees_issuance_request(
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`employee_id` INT UNSIGNED NOT NULL,
	`issuance_request_id` INT UNSIGNED NOT NULL,
	PRIMARY KEY (`id`),
	CONSTRAINT `fk_employee_id` FOREIGN KEY (`employee_id`) REFERENCES employees (`id`)
    	ON DELETE CASCADE 
        ON UPDATE CASCADE,
	CONSTRAINT `fk_issuance_request_id` FOREIGN KEY  (`issuance_request_id`) REFERENCES issuance_requests (`id`)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
	INDEX `employee_id_idx` (`employee_id` ASC),
	INDEX `issuance_request_id_idx` (`issuance_request_id` ASC)
)
CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

-- Добавление ссылки на заявку в коллективную выдачу
ALTER TABLE stock_collective_expense
	ADD COLUMN `issuance_request_id` INT UNSIGNED NULL DEFAULT NULL AFTER `transfer_agent_id`,
    ADD CONSTRAINT `fk_collective_expense_issuance_request_id` FOREIGN KEY (`issuance_request_id`) 
        REFERENCES issuance_requests (`id`)
		ON DELETE NO ACTION 
		ON UPDATE CASCADE,
	ADD INDEX `issuance_request_id_idx` (`issuance_request_id` ASC);


-- Заполнение даты начала использования для выдач
UPDATE operation_issued_by_employee op1 SET StartOfUse =
	(SELECT DATE(operation_time) FROM (SELECT * FROM operation_issued_by_employee) op2
WHERE op1.id = op2.id)
WHERE StartOfUse IS NULL AND (issued != 0 AND returned = 0);

-- -----------------------------------
-- Учёт RFID
-- -----------------------------------
alter table barcodes
	modify title varchar(24);

alter table barcodes
	add type enum ('EAN13', 'EPC96') default 'EAN13' not null after title;

create index barcodes_type_idx
	on barcodes (type);

alter table barcodes
drop key value_UNIQUE;

alter table barcodes
	add constraint value_UNIQUE
		unique (type, title);
