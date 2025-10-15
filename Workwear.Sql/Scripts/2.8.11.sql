-- Table `employee_groups`

CREATE TABLE `employee_groups`
(
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(128) NULL,
	`comment` TEXT NULL,
	PRIMARY KEY (`id`)
);

CREATE INDEX `employee_groups_name_index`
	ON `employee_groups` (`name`);

-- Table `employee_group_items`

CREATE TABLE `employee_group_items`
(
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`employee_group_id` INT UNSIGNED NOT NULL,
	`employee_id` INT UNSIGNED NOT NULL,
	`comment` TEXT NULL,
	PRIMARY KEY (`id`),
	CONSTRAINT `wear_card_groups_items_unique`
		UNIQUE (`employee_id`, `employee_group_id`),
	CONSTRAINT `foreign_key_employee_groups_items_employees`
		FOREIGN KEY (`employee_id`) REFERENCES `wear_cards` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `foreign_key_employee_groups_items_employee_groups`
		FOREIGN KEY (`employee_group_id`) REFERENCES `employee_groups` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE INDEX `employee_groups_items_employee_groups_id_index`
	ON `employee_group_items` (`employee_group_id`);

CREATE INDEX `employee_groups_items_employees_id_index`
	ON `employee_group_items` (`employee_id`);

-- Добавляем настройку списка обращений

ALTER TABLE `user_settings` 
    ADD `default_claim_list_type` ENUM('NotAnswered','NotClosed','All') NOT NULL DEFAULT 'NotClosed' AFTER `default_leader_id`;

-- Добавление общих данных для документа списания
ALTER TABLE `stock_write_off` COLLATE = utf8mb3_general_ci;
ALTER TABLE `stock_write_off` ADD `organization_id` INT UNSIGNED NULL AFTER `user_id`;
ALTER TABLE `stock_write_off` ADD `director_id` INT UNSIGNED NULL AFTER `organization_id`;
ALTER TABLE `stock_write_off` ADD `chairman_id` INT UNSIGNED NULL AFTER `director_id`;
CREATE INDEX `fk_stock_write_off_chairman_id_idx` ON `stock_write_off` (`chairman_id`);
CREATE INDEX `fk_stock_write_off_director_idx` ON `stock_write_off` (`director_id`);
CREATE INDEX `fk_stock_write_off_organization_idx` ON `stock_write_off` (`organization_id`);
ALTER TABLE `stock_write_off`
	ADD CONSTRAINT `fk_stock_write_off_chairman_id`
		FOREIGN KEY (`chairman_id`) REFERENCES `leaders` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL;
ALTER TABLE `stock_write_off`
	ADD CONSTRAINT `fk_stock_write_off_organization_id`
		FOREIGN KEY (`organization_id`) REFERENCES `organizations` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL;
ALTER TABLE `stock_write_off`
	ADD CONSTRAINT `stock_inspection_fk_director_id`
		FOREIGN KEY (`director_id`) REFERENCES `leaders` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL;

-- Добавление комментария в строку списания
ALTER TABLE `stock_write_off_detail` ADD `cause` TEXT NULL;

-- Члены комиссии документа списания
CREATE TABLE `stock_write_off_members`
(
	`id` INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`write_off_id` INT UNSIGNED NOT NULL,
	`member_id`    INT UNSIGNED NOT NULL,
	CONSTRAINT `stock_write_off_members_fk1`
		FOREIGN KEY (`write_off_id`) REFERENCES `stock_write_off` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `stock_write_off_members_fk2`
		FOREIGN KEY (`member_id`) REFERENCES `leaders` (`id`)
			ON UPDATE CASCADE
);
CREATE INDEX `stock_write_off_members_fk1_idx` ON `stock_write_off_members` (`write_off_id`);
CREATE INDEX `stock_write_off_members_fk2_idx` ON `stock_write_off_members` (`member_id`);
