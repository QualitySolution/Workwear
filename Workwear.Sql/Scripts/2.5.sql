-- Очищаем старые параметры базы
DELETE FROM base_parameters WHERE name = 'micro_updates';
DELETE FROM base_parameters WHERE name = 'edition';

-- Обновление схемы
ALTER SCHEMA DEFAULT CHARACTER SET utf8mb4  DEFAULT COLLATE utf8mb4_general_ci ;

-- Удаляем обновляемые ключи
ALTER TABLE `stock_expense` 
DROP FOREIGN KEY `fk_stock_expense_1`;

ALTER TABLE `stock_expense_detail` 
DROP FOREIGN KEY `fk_stock_expense_detail_1`;

ALTER TABLE `stock_write_off_detail` 
DROP FOREIGN KEY `fk_stock_write_off_detail_1`;

ALTER TABLE `norms` 
DROP FOREIGN KEY `fk_norms_1`,
DROP FOREIGN KEY `fk_norms_2`;

ALTER TABLE `norms_item` 
DROP FOREIGN KEY `fk_norms_item_2`;

ALTER TABLE `wear_cards_item` 
DROP FOREIGN KEY `fk_wear_cards_item_2`;

ALTER TABLE `issuance_sheet` 
DROP FOREIGN KEY `fk_issuance_sheet_5`;

ALTER TABLE `issuance_sheet_items` 
DROP FOREIGN KEY `fk_issuance_sheet_items_6`;

-- Обновляем таблицы
ALTER TABLE `base_parameters` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

ALTER TABLE `posts` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ,
ADD COLUMN `subdivision_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `name`,
ADD COLUMN `department_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `subdivision_id`,
ADD COLUMN `profession_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `department_id`,
ADD COLUMN `comments` TEXT NULL DEFAULT NULL AFTER `profession_id`,
ADD INDEX `fk_posts_subdivision_idx` (`subdivision_id` ASC),
ADD INDEX `fk_posts_department_idx` (`department_id` ASC),
ADD INDEX `fk_posts_professions_idx` (`profession_id` ASC);

ALTER TABLE `leaders` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

ALTER TABLE `item_types` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ,
CHANGE COLUMN `wear_category` `wear_category` ENUM('Wear', 'Shoes', 'WinterShoes', 'Headgear', 'Gloves', 'Mittens', 'PPE') NULL DEFAULT NULL ;

ALTER TABLE `wear_cards` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ,
ADD COLUMN `card_key` VARCHAR(16) NULL DEFAULT NULL AFTER `patronymic_name`,
ADD COLUMN `department_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `object_id`,
ADD COLUMN `size_mittens` VARCHAR(10) NULL DEFAULT NULL AFTER `size_gloves_std`,
ADD INDEX `fk_wear_cards_department_idx` (`department_id` ASC),
ADD UNIQUE INDEX `card_key_UNIQUE` (`card_key` ASC),
ADD UNIQUE INDEX `personnel_number_UNIQUE` (`personnel_number` ASC),
ADD UNIQUE INDEX `card_number_UNIQUE` (`card_number` ASC);

ALTER TABLE `stock_expense` 
ADD COLUMN `write_off_doc` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `comment`,
ADD INDEX `fk_stock_expense_2_idx` (`write_off_doc` ASC);

ALTER TABLE `stock_expense_detail` 
ADD COLUMN `protection_tools_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `growth`,
ADD INDEX `fk_stock_expense_detail_4_idx` (`protection_tools_id` ASC);

ALTER TABLE `stock_write_off_detail` 
ADD COLUMN `akt_number` VARCHAR(45) NULL DEFAULT NULL AFTER `growth`;

ALTER TABLE `object_places` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

ALTER TABLE `read_news` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

ALTER TABLE `norms_professions` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

ALTER TABLE `wear_cards_norms` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

ALTER TABLE `user_settings` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ,
ADD COLUMN `default_warehouse_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `toolbar_show`,
ADD COLUMN `default_organization_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `default_warehouse_id`,
ADD COLUMN `default_responsible_person_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `default_organization_id`,
ADD COLUMN `default_leader_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `default_responsible_person_id`,
ADD INDEX `fk_user_settings_warehouse_id_idx` (`default_warehouse_id` ASC),
ADD INDEX `fk_user_settings_organization_id_idx` (`default_organization_id` ASC),
ADD INDEX `fk_user_settings_leader_id_idx` (`default_leader_id` ASC),
ADD INDEX `fk_user_settings_responsible_person_id_idx` (`default_responsible_person_id` ASC);

ALTER TABLE `operation_issued_by_employee` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ,
ADD COLUMN `protection_tools_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `auto_writeoff_date`,
ADD COLUMN `operation_write_off_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `buh_document`,
ADD COLUMN `sign_key` VARCHAR(16) NULL DEFAULT NULL AFTER `operation_write_off_id`,
ADD COLUMN `sign_timestamp` DATETIME NULL DEFAULT NULL AFTER `sign_key`,
ADD INDEX IF NOT EXISTS `fk_operation_issued_by_employee_4_idx` (`warehouse_operation_id` ASC),
ADD INDEX `fk_operation_issued_by_employee_protection_tools_idx` (`protection_tools_id` ASC),
DROP INDEX IF EXISTS `fk_operation_issued_by_employee_6_idx`;

ALTER TABLE `operation_issued_by_employee`
ADD INDEX `fk_operation_issued_by_employee_6_idx` (`operation_write_off_id` ASC);

ALTER TABLE `vacation_type` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

ALTER TABLE `wear_cards_vacations` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

ALTER TABLE `organizations` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

ALTER TABLE `issuance_sheet` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ,
ADD COLUMN `stock_mass_expense_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `stock_expense_id`;

ALTER TABLE `warehouse` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

ALTER TABLE `operation_issued_in_subdivision` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

ALTER TABLE `operation_warehouse` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

-- Создаем новые таблицы
CREATE TABLE IF NOT EXISTS `stock_mass_expense` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `date` DATETIME NOT NULL,
  `warehouse_id` INT(10) UNSIGNED NOT NULL DEFAULT 1,
  `user_id` INT(10) UNSIGNED NOT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_mass_sending_document_1_idx` (`user_id` ASC),
  INDEX `fk_stock_mass_expense_warehouse_idx` (`warehouse_id` ASC),
  CONSTRAINT `fk_stock_mass_sending_document_1`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_mass_expense_warehouse`
    FOREIGN KEY (`warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `stock_mass_expense_employee` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `employee_id` INT(10) UNSIGNED NOT NULL,
  `stock_mass_expense_id` INT(10) UNSIGNED NOT NULL,
  `sex` ENUM('None', 'F', 'M') NULL DEFAULT NULL,
  `wear_growth` VARCHAR(10) NULL DEFAULT NULL,
  `size_wear` VARCHAR(10) NULL DEFAULT NULL,
  `size_wear_std` VARCHAR(20) NULL DEFAULT NULL,
  `size_shoes` VARCHAR(10) NULL DEFAULT NULL,
  `size_shoes_std` VARCHAR(20) NULL DEFAULT NULL,
  `size_winter_shoes` VARCHAR(10) NULL DEFAULT NULL,
  `size_winter_shoes_std` VARCHAR(20) NULL DEFAULT NULL,
  `size_headdress` VARCHAR(10) NULL DEFAULT NULL,
  `size_headdress_std` VARCHAR(20) NULL DEFAULT NULL,
  `size_gloves` VARCHAR(10) NULL DEFAULT NULL,
  `size_gloves_std` VARCHAR(20) NULL DEFAULT NULL,
  `size_mittens` VARCHAR(10) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_mass_sending_document_employee_1_idx` (`employee_id` ASC),
  INDEX `fk_stock_mass_expense_employee_1_idx` (`stock_mass_expense_id` ASC),
  CONSTRAINT `fk_stock_mass_sending_document_employee_1`
    FOREIGN KEY (`employee_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_mass_expense_employee_1`
    FOREIGN KEY (`stock_mass_expense_id`)
    REFERENCES `stock_mass_expense` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `stock_mass_expense_nomenclatures` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `nomenclature_id` INT(10) UNSIGNED NOT NULL,
  `quantity` INT(10) UNSIGNED NOT NULL,
  `stock_mass_expense_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_mass_expense_nomenclatures_1_idx` (`nomenclature_id` ASC),
  INDEX `fk_stock_mass_expense_nomenclatures_2_idx` (`stock_mass_expense_id` ASC),
  CONSTRAINT `fk_stock_mass_expense_nomenclatures_1`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_mass_expense_nomenclatures_2`
    FOREIGN KEY (`stock_mass_expense_id`)
    REFERENCES `stock_mass_expense` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `stock_mass_expense_operation` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `operation_warehouse_id` INT(10) UNSIGNED NOT NULL,
  `operation_issued_by_employee` INT(10) UNSIGNED NOT NULL,
  `stock_mass_expense_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_mass_expense_operation_1_idx` (`operation_warehouse_id` ASC),
  INDEX `fk_stock_mass_expense_operation_3_idx` (`stock_mass_expense_id` ASC),
  INDEX `fk_stock_mass_expense_operation_2_idx` (`operation_issued_by_employee` ASC),
  CONSTRAINT `fk_stock_mass_expense_operation_2`
    FOREIGN KEY (`operation_issued_by_employee`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_mass_expense_operation_3`
    FOREIGN KEY (`stock_mass_expense_id`)
    REFERENCES `stock_mass_expense` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

ALTER TABLE `stock_transfer` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ;

CREATE TABLE IF NOT EXISTS `protection_tools` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(240) NOT NULL,
  `item_types_id` INT(10) UNSIGNED NOT NULL DEFAULT 1,
  `comments` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_protection_tools_1_idx` (`item_types_id` ASC),
  CONSTRAINT `fk_protection_tools_1` 
    FOREIGN KEY (`item_types_id`)
    REFERENCES `item_types` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `protection_tools_replacement` (
  `protection_tools_id` INT(10) UNSIGNED NOT NULL,
  `protection_tools_analog_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`protection_tools_id`, `protection_tools_analog_id`),
  INDEX `fk_item_types_replacement_2_idx` (`protection_tools_analog_id` ASC),
  CONSTRAINT `fk_item_types_replacement_1`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_item_types_replacement_2`
    FOREIGN KEY (`protection_tools_analog_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `departments` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(254) NOT NULL,
  `subdivision_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `comments` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_departaments_1_idx` (`subdivision_id` ASC),
  CONSTRAINT `fk_departaments_1`
    FOREIGN KEY (`subdivision_id`)
    REFERENCES `objects` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `professions` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `code` INT(10) UNSIGNED NULL DEFAULT NULL,
  `name` VARCHAR(200) NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `protection_tools_nomenclature` (
  `protection_tools_id` INT(10) UNSIGNED NOT NULL,
  `nomenclature_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`protection_tools_id`, `nomenclature_id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

-- Заполнение данными

-- Добавляем название нормы
ALTER TABLE `norms` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ,
ADD COLUMN `name` VARCHAR(200) NULL DEFAULT NULL AFTER `regulations_annex_id`;
-- Заполняем названия норма старыми данными
UPDATE `norms` SET `name`= CONCAT_WS(' ', CONCAT('ТОН №', `ton_number`), concat('прил. ', `ton_attachment`), CONCAT('п. ', `ton_paragraph`));
-- Удаляем старые поля нормы
ALTER TABLE `norms` 
DROP COLUMN `ton_attachment`,
DROP COLUMN `ton_number`;

-- Создаем protection_tools
INSERT INTO protection_tools
(name, item_types_id, comments)
SELECT item_types.name, item_types.id, "Перенос из версии 2.4"
FROM item_types;

-- Создаем связь с номеклатурой
INSERT INTO protection_tools_nomenclature
(protection_tools_id, nomenclature_id)
SELECT protection_tools.id, nomenclature.id
FROM nomenclature
JOIN protection_tools ON protection_tools.item_types_id = nomenclature.type_id;

-- Заполняем stock_expense_detail.`protection_tools_id`
UPDATE stock_expense_detail
JOIN protection_tools_nomenclature ON protection_tools_nomenclature.nomenclature_id = stock_expense_detail.nomenclature_id
SET stock_expense_detail.protection_tools_id = protection_tools_nomenclature.protection_tools_id;

-- Заполняем norms_item
ALTER TABLE `norms_item` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ,
ADD COLUMN `protection_tools_id` INT(10) UNSIGNED NOT NULL AFTER `norm_id`,
DROP INDEX `fk_norms_item_2_idx` ,
ADD INDEX `fk_norms_item_2_idx` (`protection_tools_id` ASC);

UPDATE norms_item
JOIN protection_tools ON protection_tools.item_types_id = norms_item.itemtype_id
SET norms_item.protection_tools_id = protection_tools.id;

ALTER TABLE `norms_item` 
DROP COLUMN `itemtype_id`;

-- Заменяем id в wear_cards_item
ALTER TABLE `wear_cards_item` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ,
ADD COLUMN `protection_tools_id` INT(10) UNSIGNED NOT NULL AFTER `wear_card_id`,
DROP INDEX `fk_wear_cards_item_2_idx` ,
ADD INDEX `fk_wear_cards_item_2_idx` (`protection_tools_id` ASC);

UPDATE wear_cards_item
JOIN protection_tools ON protection_tools.item_types_id = wear_cards_item.itemtype_id
SET wear_cards_item.protection_tools_id = protection_tools.id;

ALTER TABLE `wear_cards_item` 
DROP COLUMN `itemtype_id`;

-- Обновляем операции выдачи
UPDATE operation_issued_by_employee
JOIN norms_item ON norms_item.id = operation_issued_by_employee.norm_item_id
SET operation_issued_by_employee.protection_tools_id = norms_item.protection_tools_id;

-- Обновляем ведомости
ALTER TABLE `issuance_sheet_items` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ,
ADD COLUMN `protection_tools_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `nomenclature_id`,
DROP INDEX `fk_issuance_sheet_items_6_idx` ,
ADD INDEX `fk_issuance_sheet_items_6_idx` (`protection_tools_id` ASC);

UPDATE issuance_sheet_items
JOIN protection_tools ON protection_tools.item_types_id = issuance_sheet_items.itemtype_id
SET issuance_sheet_items.protection_tools_id = protection_tools.id;

ALTER TABLE `issuance_sheet_items` 
DROP COLUMN `itemtype_id`;

-- Восстанавливаем ключи
ALTER TABLE `posts` 
ADD CONSTRAINT `fk_posts_subdivision`
  FOREIGN KEY (`subdivision_id`)
  REFERENCES `objects` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_posts_department`
  FOREIGN KEY (`department_id`)
  REFERENCES `departments` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_posts_professions`
  FOREIGN KEY (`profession_id`)
  REFERENCES `professions` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE;

ALTER TABLE `wear_cards` 
ADD CONSTRAINT `fk_wear_cards_department`
  FOREIGN KEY (`department_id`)
  REFERENCES `departments` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE;

ALTER TABLE `stock_expense` 
ADD CONSTRAINT `fk_stock_expense_1`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_stock_expense_2`
  FOREIGN KEY (`write_off_doc`)
  REFERENCES `stock_write_off` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `stock_expense_detail` 
ADD CONSTRAINT `fk_stock_expense_detail_1`
  FOREIGN KEY (`employee_issue_operation_id`)
  REFERENCES `operation_issued_by_employee` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_stock_expense_detail_4`
  FOREIGN KEY (`protection_tools_id`)
  REFERENCES `protection_tools` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `stock_write_off_detail` 
ADD CONSTRAINT `fk_stock_write_off_detail_1`
  FOREIGN KEY (`employee_issue_operation_id`)
  REFERENCES `operation_issued_by_employee` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `norms` 
ADD CONSTRAINT `fk_norms_1`
  FOREIGN KEY (`regulations_id`)
  REFERENCES `regulations` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_norms_2`
  FOREIGN KEY (`regulations_annex_id`)
  REFERENCES `regulations_annex` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `norms_item` 
ADD CONSTRAINT `fk_norms_item_2`
  FOREIGN KEY (`protection_tools_id`)
  REFERENCES `protection_tools` (`id`)
  ON DELETE RESTRICT
  ON UPDATE CASCADE;

ALTER TABLE `wear_cards_item` 
ADD CONSTRAINT `fk_wear_cards_item_2`
  FOREIGN KEY (`protection_tools_id`)
  REFERENCES `protection_tools` (`id`)
  ON DELETE RESTRICT
  ON UPDATE CASCADE;

ALTER TABLE `user_settings` 
ADD CONSTRAINT `fk_user_settings_warehouse_id`
  FOREIGN KEY (`default_warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_user_settings_organization_id`
  FOREIGN KEY (`default_organization_id`)
  REFERENCES `organizations` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_user_settings_responsible_person_id`
  FOREIGN KEY (`default_responsible_person_id`)
  REFERENCES `wear_cards` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_user_settings_leader_id`
  FOREIGN KEY (`default_leader_id`)
  REFERENCES `leaders` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `operation_issued_by_employee` 
ADD CONSTRAINT `fk_operation_issued_by_employee_protection_tools`
  FOREIGN KEY (`protection_tools_id`)
  REFERENCES `protection_tools` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_operation_issued_by_employee_6`
  FOREIGN KEY (`operation_write_off_id`)
  REFERENCES `operation_issued_by_employee` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `issuance_sheet` 
ADD CONSTRAINT `fk_issuance_sheet_5`
  FOREIGN KEY (`stock_expense_id`)
  REFERENCES `stock_expense` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `issuance_sheet_items` 
ADD CONSTRAINT `fk_issuance_sheet_items_6`
  FOREIGN KEY (`protection_tools_id`)
  REFERENCES `protection_tools` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE;
