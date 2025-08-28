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
    ADD CONSTRAINT `fk_issuance_request_id` FOREIGN KEY (`issuance_request_id`) 
        REFERENCES issuance_requests (`id`)
		ON DELETE NO ACTION 
		ON UPDATE CASCADE,
	ADD INDEX `issuance_request_id_idx` (`issuance_request_id` ASC);
