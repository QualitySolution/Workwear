
-- Удаление связей
ALTER TABLE `stock_income_detail` 
DROP FOREIGN KEY `fk_stock_income_detail_stock_expense`;

ALTER TABLE `stock_expense_detail` 
DROP FOREIGN KEY `fk_stock_expense_detail_enter_row`;

ALTER TABLE `stock_write_off_detail` 
DROP FOREIGN KEY `fk_stock_write_off_detail_income`,
DROP FOREIGN KEY `fk_stock_write_off_detail_expense`;

ALTER TABLE `operation_issued_by_employee` 
DROP FOREIGN KEY `fk_operation_issued_by_employee_4`;

ALTER TABLE `issuance_sheet` 
DROP FOREIGN KEY `fk_issuance_sheet_2`;

-- Добавление колонок в старые таблицы
ALTER TABLE `objects` 
CHANGE COLUMN `address` `address` TEXT NULL DEFAULT NULL AFTER `code`,
ADD COLUMN `warehouse_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `name`,
ADD INDEX `fk_objects_1_idx` (`warehouse_id` ASC);

ALTER TABLE `stock_income` 
ADD COLUMN `warehouse_id` INT(10) UNSIGNED NOT NULL AFTER `date`,
ADD INDEX `fk_stock_income_1_idx` (`warehouse_id` ASC);

ALTER TABLE `stock_income_detail` 
ADD COLUMN `subdivision_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `employee_issue_operation_id`,
ADD COLUMN `warehouse_operation_id` INT(10) UNSIGNED NOT NULL AFTER `subdivision_issue_operation_id`,
ADD COLUMN `size` VARCHAR(10) NULL DEFAULT NULL AFTER `warehouse_operation_id`,
ADD COLUMN `growth` VARCHAR(10) NULL DEFAULT NULL AFTER `size`,
ADD INDEX `fk_stock_income_detail_2_idx` (`warehouse_operation_id` ASC),
ADD INDEX `fk_stock_income_detail_3_idx` (`subdivision_issue_operation_id` ASC) ;

ALTER TABLE `stock_expense` 
ADD COLUMN `warehouse_id` INT(10) UNSIGNED NOT NULL AFTER `operation`,
ADD INDEX `fk_stock_expense_1_idx` (`warehouse_id` ASC);

ALTER TABLE `stock_expense_detail` 
ADD COLUMN `subdivision_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `employee_issue_operation_id`,
ADD COLUMN `warehouse_operation_id` INT(10) UNSIGNED NOT NULL AFTER `subdivision_issue_operation_id`,
ADD COLUMN `size` VARCHAR(10) NULL DEFAULT NULL AFTER `warehouse_operation_id`,
ADD COLUMN `growth` VARCHAR(10) NULL DEFAULT NULL AFTER `size`,
ADD INDEX `fk_stock_expense_detail_2_idx` (`warehouse_operation_id` ASC),
ADD INDEX `fk_stock_expense_detail_3_idx` (`subdivision_issue_operation_id` ASC);

ALTER TABLE `stock_write_off_detail` 
ADD COLUMN `subdivision_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `employee_issue_operation_id`,
ADD COLUMN `warehouse_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `subdivision_issue_operation_id`,
ADD COLUMN `warehouse_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `warehouse_id`,
ADD COLUMN `size` VARCHAR(10) NULL DEFAULT NULL AFTER `warehouse_operation_id`,
ADD COLUMN `growth` VARCHAR(10) NULL DEFAULT NULL AFTER `size`,
ADD INDEX `fk_stock_write_off_detail_2_idx` (`warehouse_operation_id` ASC),
ADD INDEX `fk_stock_write_off_detail_3_idx` (`warehouse_id` ASC),
ADD INDEX `fk_stock_write_off_detail_4_idx` (`subdivision_issue_operation_id` ASC);

ALTER TABLE `norms` 
ADD COLUMN `datefrom` DATETIME NULL DEFAULT NULL AFTER `comment`,
ADD COLUMN `dateto` DATETIME NULL DEFAULT NULL AFTER `datefrom`;

ALTER TABLE `operation_issued_by_employee` 
ADD COLUMN `size` VARCHAR(10) NULL DEFAULT NULL AFTER `nomenclature_id`,
ADD COLUMN `growth` VARCHAR(10) NULL DEFAULT NULL AFTER `size`,
ADD COLUMN `warehouse_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `issued_operation_id`,
ADD INDEX `fk_operation_issued_by_employee_4_idx` (`warehouse_operation_id` ASC),
ADD INDEX `index8` (`size` ASC),
ADD INDEX `index9` (`growth` ASC),
ADD INDEX `index10` (`wear_percent` ASC);

ALTER TABLE `issuance_sheet_items` 
ADD COLUMN `size` VARCHAR(10) NULL DEFAULT NULL AFTER `lifetime`,
ADD COLUMN `growth` VARCHAR(10) NULL DEFAULT NULL AFTER `size`
CHANGE COLUMN `lifetime` `lifetime` DECIMAL(5,2) UNSIGNED NULL DEFAULT NULL ;

ALTER TABLE `nomenclature` 
ADD COLUMN `number` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `comment`;

-- Создание новых таблиц

CREATE TABLE IF NOT EXISTS `warehouse` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `operation_warehouse` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `operation_time` DATETIME NOT NULL,
  `warehouse_receipt_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_expense_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `nomenclature_id` INT(10) UNSIGNED NOT NULL,
  `size` VARCHAR(10) NULL DEFAULT NULL,
  `growth` VARCHAR(10) NULL DEFAULT NULL,
  `amount` INT(10) UNSIGNED NOT NULL,
  `wear_percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 0,
  `cost` DECIMAL(10,2) UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_operation_warehouse_2_idx` (`warehouse_receipt_id` ASC),
  INDEX `fk_operation_warehouse_3_idx` (`warehouse_expense_id` ASC),
  INDEX `index4` (`size` ASC),
  INDEX `index5` (`growth` ASC),
  INDEX `fk_operation_warehouse_1_idx` (`nomenclature_id` ASC),
  CONSTRAINT `fk_operation_warehouse_1`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_warehouse_2`
    FOREIGN KEY (`warehouse_receipt_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_operation_warehouse_3`
    FOREIGN KEY (`warehouse_expense_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `stock_transfer` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `warehouse_from_id` INT(10) UNSIGNED NOT NULL,
  `warehouse_to_id` INT(10) UNSIGNED NOT NULL,
  `date` DATETIME NOT NULL,
  `user_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_transfer_1_idx` (`warehouse_from_id` ASC),
  INDEX `fk_stock_transfer_2_idx` (`warehouse_to_id` ASC),
  UNIQUE INDEX `id_UNIQUE` (`id` ASC),
  INDEX `fk_stock_transfer_3_idx` (`user_id` ASC),
  CONSTRAINT `fk_stock_transfer_1`
    FOREIGN KEY (`warehouse_from_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_2`
    FOREIGN KEY (`warehouse_to_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_3`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `stock_transfer_detail` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_transfer_id` INT(10) UNSIGNED NOT NULL,
  `nomenclature_id` INT(10) UNSIGNED NOT NULL,
  `quantity` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `id_UNIQUE` (`id` ASC),
  CONSTRAINT `fk_stock_transfer_detail_1`
    FOREIGN KEY (`id`)
    REFERENCES `stock_transfer` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_detail_2`
    FOREIGN KEY (`id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_detail_3`
    FOREIGN KEY (`id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `operation_issued_in_subdivision` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `operation_time` DATETIME NOT NULL,
  `subdivision_id` INT(10) UNSIGNED NOT NULL,
  `subdivision_place_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `nomenclature_id` INT(10) UNSIGNED NOT NULL,
  `size` VARCHAR(10) NULL DEFAULT NULL,
  `growth` VARCHAR(10) NULL DEFAULT NULL,
  `wear_percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 0.00,
  `issued` INT(11) NOT NULL DEFAULT 0,
  `returned` INT(11) NOT NULL DEFAULT 0,
  `auto_writeoff` TINYINT(1) NOT NULL DEFAULT 1,
  `auto_writeoff_date` DATE NULL DEFAULT NULL,
  `start_of_use` DATE NULL DEFAULT NULL,
  `expiry_on` DATE NULL DEFAULT NULL,
  `issued_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `operation_issued_by_employee_date` (`operation_time` ASC),
  INDEX `index8` (`size` ASC),
  INDEX `index9` (`growth` ASC),
  INDEX `index10` (`wear_percent` ASC),
  INDEX `fk_operation_issued_in_subdivision_1_idx` (`subdivision_id` ASC),
  INDEX `fk_operation_issued_in_subdivision_2_idx` (`nomenclature_id` ASC),
  INDEX `fk_operation_issued_in_subdivision_3_idx` (`issued_operation_id` ASC),
  INDEX `fk_operation_issued_in_subdivision_4_idx` (`warehouse_operation_id` ASC),
  INDEX `fk_operation_issued_in_subdivision_5_idx` (`subdivision_place_id` ASC),
  CONSTRAINT `fk_operation_issued_in_subdivision_1`
    FOREIGN KEY (`subdivision_id`)
    REFERENCES `objects` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_in_subdivision_2`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_operation_issued_in_subdivision_3`
    FOREIGN KEY (`issued_operation_id`)
    REFERENCES `operation_issued_in_subdivision` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_in_subdivision_4`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_in_subdivision_5`
    FOREIGN KEY (`subdivision_place_id`)
    REFERENCES `object_places` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

-- Создание связей

ALTER TABLE `objects` 
ADD CONSTRAINT `fk_objects_1`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;

ALTER TABLE `stock_income` 
ADD CONSTRAINT `fk_stock_income_1`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `stock_income_detail` 
ADD CONSTRAINT `fk_stock_income_detail_2`
  FOREIGN KEY (`warehouse_operation_id`)
  REFERENCES `operation_warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_stock_income_detail_3`
  FOREIGN KEY (`subdivision_issue_operation_id`)
  REFERENCES `operation_issued_in_subdivision` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `stock_expense` 
ADD CONSTRAINT `fk_stock_expense_1`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE RESTRICT
  ON UPDATE CASCADE;

ALTER TABLE `stock_expense_detail` 
ADD CONSTRAINT `fk_stock_expense_detail_2`
  FOREIGN KEY (`warehouse_operation_id`)
  REFERENCES `operation_warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_stock_expense_detail_3`
  FOREIGN KEY (`subdivision_issue_operation_id`)
  REFERENCES `operation_issued_in_subdivision` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `stock_write_off_detail` 
ADD CONSTRAINT `fk_stock_write_off_detail_2`
  FOREIGN KEY (`warehouse_operation_id`)
  REFERENCES `operation_warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_stock_write_off_detail_3`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_stock_write_off_detail_4`
  FOREIGN KEY (`subdivision_issue_operation_id`)
  REFERENCES `operation_issued_in_subdivision` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `operation_issued_by_employee` 
ADD CONSTRAINT `fk_operation_issued_by_employee_4`
  FOREIGN KEY (`warehouse_operation_id`)
  REFERENCES `operation_warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `issuance_sheet` 
ADD CONSTRAINT `fk_issuance_sheet_2`
  FOREIGN KEY (`organization_id`)
  REFERENCES `objects` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;
  
-- Миграция данных

-- -----------------------------------------------------
-- Data for table `warehouse`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `warehouse` (`id`, `name`) VALUES (DEFAULT, 'Основной склад');

COMMIT;

-- Номенклатуры
-- Складские операции
-- Операции выдачи сотрудникам
-- Операции выдачи на объекты 

-- Удаление старых колонок
ALTER TABLE `nomenclature` 
DROP COLUMN `growth`,
DROP COLUMN `size`;

ALTER TABLE `stock_income_detail` 
DROP COLUMN `stock_expense_detail_id`,
DROP COLUMN `life_percent`,
DROP INDEX `fk_stock_income_detail_stock_expense_idx` ;

ALTER TABLE `stock_expense_detail` 
DROP COLUMN `auto_writeoff_date`,
DROP COLUMN `stock_income_detail_id`,
DROP INDEX `fk_stock_expense_detail_enter_row_idx` ;

ALTER TABLE `stock_write_off_detail` 
DROP COLUMN `stock_income_detail_id`,
DROP COLUMN `stock_expense_detail_id`,
DROP INDEX `fk_stock_write_off_detail_income_idx` ,
DROP INDEX `fk_stock_write_off_detail_expense_idx` ;

ALTER TABLE `wear_cards_item` 
DROP COLUMN `matched_nomenclature_id`;

ALTER TABLE `operation_issued_by_employee` 
DROP COLUMN `stock_income_detail_id`,
DROP INDEX `fk_operation_issued_by_employee_4_idx`;

ALTER TABLE `issuance_sheet` 
DROP INDEX `fk_issuance_sheet_2_idx` ;
  
-- Очистка истории прочитанных новостей
DELETE FROM `read_news` WHERE `feed_id` = "workwearnews"