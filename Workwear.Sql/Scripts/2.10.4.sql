
-- информация о окнах
CREATE TABLE visit_windows
(
    id   INT UNSIGNED AUTO_INCREMENT
        PRIMARY KEY,
    name CHAR(32) NULL
)
    COMMENT 'информация о окнах';

-- изменяем записи посещений на новый формат
ALTER TABLE `visits`
    ADD COLUMN `service_type` ENUM('GiveWear','NewEmployee','Unidentified','Dismiss','GiveReport','WriteOff','ClothingService','Appeal') NOT NULL DEFAULT 'Unidentified' AFTER `employee_id`,
    ADD COLUMN `create_from_lk` TINYINT(1) NOT NULL DEFAULT 1 AFTER `employee_create`,
    ADD COLUMN `status` ENUM('New','Queued','Serviced','Done','Canceled','Missing') NOT NULL DEFAULT 'New' AFTER `done`,
    ADD COLUMN `ticket_number` CHAR(4) NOT NULL DEFAULT '' COMMENT 'Талончик в очереди' AFTER `status`,
    ADD COLUMN `window_id` INT(10) UNSIGNED DEFAULT NULL COMMENT 'ID окна обслуживания' AFTER `ticket_number`,
    ADD COLUMN `time_entry` DATETIME DEFAULT NULL COMMENT 'Время постановки в очередь на ПВ' AFTER `window_id`,
    ADD COLUMN `time_start` DATETIME DEFAULT NULL COMMENT 'Начало обслуживания (перво посещение окна)' AFTER `time_entry`,
    ADD COLUMN `time_finish` DATETIME DEFAULT NULL COMMENT 'Завершение визита' AFTER `time_start`;

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
    tiket     CHAR(4)                                                                             NULL,
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
);

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
);

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
	(SELECT DATE(operation_time) FROM operation_issued_by_employee op2
WHERE op1.id = op2.id)
WHERE StartOfUse IS NULL AND (issued != 0 AND returned = 0);
