-- Заявка на выдачу
CREATE TABLE applications_for_issuance(
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`arrival_date` DATE NOT NULL,
	`last_issue_date` DATE NOT NULL,
	`status` ENUM('New', 'Issued', 'PartiallyIssued', 'Overdue', 'PartiallyOverdue') DEFAULT 'New' NOT NULL,
	`comment` TEXT NULL,
	`user_id` INT UNSIGNED NOT NULL,
	`creation_time` DATETIME NULL DEFAULT NULL,
	PRIMARY KEY (`id`),
	CONSTRAINT `fk_applications_for_issuance_user_id` FOREIGN KEY (`user_id`) REFERENCES users (`id`)
    	ON DELETE NO ACTION 
		ON UPDATE CASCADE,
	INDEX `applications_for_issuance_user_id_idx` (`user_id` ASC) 
);

-- Сотрудники в заявках на выдачу
CREATE TABLE employee_applications_for_issuance(
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`employee_id` INT UNSIGNED NOT NULL,
	`application_for_issuance_id` INT UNSIGNED NOT NULL,
	CONSTRAINT `fk_employee_applications_for_issuance_employee_id` FOREIGN KEY (`employee_id`) REFERENCES employees (`id`)
    	ON DELETE NO ACTION 
        ON UPDATE CASCADE,
	CONSTRAINT `fk_application_for_issuance_id` FOREIGN KEY  (`application_for_issuance_id`) REFERENCES applications_for_issuance (`id`)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
	INDEX `employee_applications_for_issuance_employee_id_idx` (`employee_id` ASC),
	INDEX `application_for_issuance_id_idx` (`application_for_issuance_id` ASC)
);

-- Добавление ссылки на заявку в коллективную выдачу
ALTER TABLE stock_collective_expense
	ADD COLUMN `application_for_issuance_id` INT UNSIGNED NULL DEFAULT NULL AFTER `transfer_agent_id`,
    ADD CONSTRAINT `fk_application_for_issuance_id` FOREIGN KEY (`application_for_issuance_id`) 
        REFERENCES applications_for_issuance (`id`)
		ON DELETE CASCADE
		ON UPDATE CASCADE,
	ADD INDEX `application_for_issuance_id_idx` (`application_for_issuance_id` ASC);
