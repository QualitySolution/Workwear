ALTER TABLE `stock_income_detail` 
DROP FOREIGN KEY `fk_stock_income_detail_1`;

ALTER TABLE `user_settings` 
DROP FOREIGN KEY `fk_user_settings_organization_id`,
DROP FOREIGN KEY `fk_user_settings_responsible_person_id`;

ALTER TABLE `leaders` 
ADD COLUMN `employee_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `position`,
ADD INDEX `fk_leaders_1_idx` (`employee_id` ASC) VISIBLE;
;

#Стандарт роста, больше не используем
ALTER TABLE `nomenclature` 
DROP COLUMN `growth_std`;

ALTER TABLE `item_types` 
ADD COLUMN `issue_type` ENUM('Personal', 'Collective') NOT NULL DEFAULT 'Personal' AFTER `wear_category`;

ALTER TABLE `wear_cards` 
ADD COLUMN `phone_number` VARCHAR(16) NULL DEFAULT NULL AFTER `size_mittens`,
ADD COLUMN `lk_registered` TINYINT(1) NOT NULL DEFAULT 0 AFTER `phone_number`;
;

ALTER TABLE `norms_item` 
ADD COLUMN `condition_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `period_count`,
CHANGE COLUMN `period_type` `period_type` ENUM('Year', 'Month', 'Shift', 'Wearout') NOT NULL DEFAULT 'Year' ,
ADD INDEX `fk_norms_item_3_idx` (`condition_id` ASC) VISIBLE;
;

ALTER TABLE `operation_issued_by_employee` 
ADD COLUMN `manual_operation` TINYINT(1) NOT NULL DEFAULT 0 AFTER `sign_timestamp`,
CHANGE COLUMN `nomenclature_id` `nomenclature_id` INT(10) UNSIGNED NULL DEFAULT NULL; #Для ручной операции без номенклатуры

ALTER TABLE `issuance_sheet` 
ADD COLUMN `stock_collective_expense_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `stock_mass_expense_id`,
ADD INDEX `fk_issuance_sheet_6_idx` (`stock_mass_expense_id` ASC) VISIBLE,
ADD INDEX `fk_issuance_sheet_7_idx` (`stock_collective_expense_id` ASC) VISIBLE;
;

ALTER TABLE `issuance_sheet_items` 
ADD COLUMN `stock_collective_expense_item_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `stock_expense_detail_id`,
ADD INDEX `fk_issuance_sheet_items_7_idx` (`stock_collective_expense_item_id` ASC) VISIBLE;
;

ALTER TABLE `protection_tools` 
ADD COLUMN `assessed_cost` DECIMAL(10,2) UNSIGNED NULL DEFAULT NULL AFTER `item_types_id`;

CREATE TABLE IF NOT EXISTS `stock_collective_expense` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `warehouse_id` INT(10) UNSIGNED NOT NULL,
  `date` DATE NOT NULL,
  `user_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_expense_user_idx` (`user_id` ASC) VISIBLE,
  INDEX `fk_stock_expense_1_idx` (`warehouse_id` ASC) VISIBLE,
  CONSTRAINT `fk_stock_collective_expense_1`
    FOREIGN KEY (`warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_2`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `stock_collective_expense_detail` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_collective_expense_id` INT(10) UNSIGNED NOT NULL,
  `employee_id` INT(10) UNSIGNED NOT NULL,
  `protection_tools_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `nomenclature_id` INT(10) UNSIGNED NOT NULL,
  `quantity` INT(10) UNSIGNED NOT NULL,
  `employee_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT(10) UNSIGNED NOT NULL,
  `size` VARCHAR(10) NULL DEFAULT NULL,
  `growth` VARCHAR(10) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_expense_detail_nomenclature_idx` (`nomenclature_id` ASC) VISIBLE,
  INDEX `fk_stock_expense_detail_1_idx` (`employee_issue_operation_id` ASC) VISIBLE,
  INDEX `fk_stock_expense_detail_2_idx` (`warehouse_operation_id` ASC) VISIBLE,
  INDEX `fk_stock_expense_detail_4_idx` (`protection_tools_id` ASC) VISIBLE,
  INDEX `fk_stock_collective_expense_detail_4_idx` (`stock_collective_expense_id` ASC) VISIBLE,
  INDEX `fk_stock_collective_expense_detail_6_idx` (`employee_id` ASC) VISIBLE,
  CONSTRAINT `fk_stock_collective_expense_detail_1`
    FOREIGN KEY (`employee_issue_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_detail_2`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_detail_3`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_detail_4`
    FOREIGN KEY (`stock_collective_expense_id`)
    REFERENCES `stock_collective_expense` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_detail_5`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_collective_expense_detail_6`
    FOREIGN KEY (`employee_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `message_templates` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(100) NOT NULL,
  `message_title` VARCHAR(200) NOT NULL,
  `message_text` VARCHAR(400) NOT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `norm_conditions` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(100) NOT NULL,
  `sex` ENUM('ForAll', 'OnlyMen', 'OnlyWomen') NOT NULL DEFAULT 'ForAll',
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

ALTER TABLE `leaders` 
ADD CONSTRAINT `fk_leaders_1`
  FOREIGN KEY (`employee_id`)
  REFERENCES `wear_cards` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `stock_income_detail` 
ADD CONSTRAINT `fk_stock_income_detail_1`
  FOREIGN KEY (`employee_issue_operation_id`)
  REFERENCES `operation_issued_by_employee` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `norms_item` 
ADD CONSTRAINT `fk_norms_item_3`
  FOREIGN KEY (`condition_id`)
  REFERENCES `norm_conditions` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE;

ALTER TABLE `user_settings` 
DROP FOREIGN KEY `fk_user_settings_warehouse_id`,
DROP FOREIGN KEY `fk_user_settings_leader_id`;

ALTER TABLE `user_settings` ADD CONSTRAINT `fk_user_settings_warehouse_id`
  FOREIGN KEY (`default_warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_user_settings_organization_id`
  FOREIGN KEY (`default_organization_id`)
  REFERENCES `organizations` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_user_settings_responsible_person_id`
  FOREIGN KEY (`default_responsible_person_id`)
  REFERENCES `leaders` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_user_settings_leader_id`
  FOREIGN KEY (`default_leader_id`)
  REFERENCES `leaders` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE;

ALTER TABLE `issuance_sheet` 
ADD CONSTRAINT `fk_issuance_sheet_6`
  FOREIGN KEY (`stock_mass_expense_id`)
  REFERENCES `stock_mass_expense` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_issuance_sheet_7`
  FOREIGN KEY (`stock_collective_expense_id`)
  REFERENCES `stock_collective_expense` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `issuance_sheet_items` 
ADD CONSTRAINT `fk_issuance_sheet_items_7`
  FOREIGN KEY (`stock_collective_expense_item_id`)
  REFERENCES `stock_collective_expense_detail` (`id`)
  ON DELETE CASCADE
  ON UPDATE CASCADE;

﻿#Удаляем пустые строки из размеров и роста в операциях. Это приводит к неоднозначности в запросах
UPDATE `operation_warehouse` SET `size`= NULL WHERE size = "";
UPDATE `operation_warehouse` SET `growth`= NULL WHERE growth = "";
UPDATE `operation_issued_by_employee` SET `size`= NULL WHERE size = "";
UPDATE `operation_issued_by_employee` SET `growth`= NULL WHERE growth = "";

#У таблицы operation_issued_by_employee поменялась немного идеология теперь protection_tools_id является главным значением для выборки вместо номеклатуры
#Поэтому копируем для возвратов и списаний protection_tools_id из операций выдачи
UPDATE `operation_issued_by_employee` operation
    LEFT JOIN operation_issued_by_employee issued ON issued.id = operation.issued_operation_id
    SET operation.protection_tools_id = issued.protection_tools_id
WHERE issued.id IS NOT NULL AND operation.protection_tools_id IS NULL;

DELIMITER $$
CREATE FUNCTION `count_issue`(`amount` INT UNSIGNED, `norm_period` INT UNSIGNED, `next_month` INT UNSIGNED, `next_year` INT UNSIGNED, `begin_month` INT UNSIGNED, `begin_year` INT UNSIGNED, `end_month` INT UNSIGNED, `end_year` INT UNSIGNED) RETURNS int(10) unsigned
    NO SQL
    DETERMINISTIC
    COMMENT 'Функция рассчитывает количество необходимое к выдачи.'
BEGIN
DECLARE issue_count, total_month_next, total_month_begin, total_month_end INT;

IF norm_period <= 0 THEN RETURN 0; END IF;
IF next_month IS NULL OR next_year IS NULL THEN RETURN 0; END IF;

SET total_month_begin = begin_month + begin_year * 12;
SET total_month_end = end_month + end_year * 12;

SET issue_count = 0;
SET total_month_next = next_month + next_year * 12;

WHILE total_month_next <= total_month_end DO
    IF total_month_next >= total_month_begin THEN 
    	SET issue_count = issue_count + amount;
    END IF;
  SET total_month_next = total_month_next + norm_period;  
  END WHILE;
RETURN issue_count;
END$$

DELIMITER ;