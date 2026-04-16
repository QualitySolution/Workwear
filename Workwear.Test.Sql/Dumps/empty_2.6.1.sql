-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema workwear
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Table `users`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `users` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  `login` VARCHAR(45) NOT NULL,
  `deactivated` TINYINT(1) NOT NULL DEFAULT 0,
  `email` VARCHAR(60) NULL DEFAULT NULL,
  `description` TEXT NULL DEFAULT NULL,
  `admin` TINYINT(1) NOT NULL DEFAULT FALSE,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `base_parameters`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `base_parameters` (
  `name` VARCHAR(20) NOT NULL,
  `str_value` VARCHAR(100) NULL DEFAULT NULL,
  PRIMARY KEY (`name`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `warehouse`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `warehouse` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `objects`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `objects` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code` VARCHAR(20) NULL DEFAULT NULL,
  `address` TEXT NULL DEFAULT NULL,
  `name` VARCHAR(240) NOT NULL,
  `warehouse_id` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_objects_1_idx` (`warehouse_id` ASC),
  CONSTRAINT `fk_objects_1`
    FOREIGN KEY (`warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `departments`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `departments` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(254) NOT NULL,
  `subdivision_id` INT UNSIGNED NULL DEFAULT NULL,
  `comments` TEXT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_departaments_1_idx` (`subdivision_id` ASC),
  CONSTRAINT `fk_departaments_1`
    FOREIGN KEY (`subdivision_id`)
    REFERENCES `objects` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `professions`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `professions` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code` INT UNSIGNED NULL,
  `name` VARCHAR(200) NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `posts`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `posts` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(180) NOT NULL,
  `subdivision_id` INT UNSIGNED NULL,
  `department_id` INT UNSIGNED NULL,
  `profession_id` INT UNSIGNED NULL,
  `comments` TEXT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_posts_subdivision_idx` (`subdivision_id` ASC),
  INDEX `fk_posts_department_idx` (`department_id` ASC),
  INDEX `fk_posts_professions_idx` (`profession_id` ASC),
  CONSTRAINT `fk_posts_subdivision`
    FOREIGN KEY (`subdivision_id`)
    REFERENCES `objects` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_posts_department`
    FOREIGN KEY (`department_id`)
    REFERENCES `departments` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_posts_professions`
    FOREIGN KEY (`profession_id`)
    REFERENCES `professions` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;


-- -----------------------------------------------------
-- Table `wear_cards`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `wear_cards` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `card_number` VARCHAR(15) NULL DEFAULT NULL,
  `personnel_number` VARCHAR(15) NULL DEFAULT NULL,
  `last_name` VARCHAR(20) NULL,
  `first_name` VARCHAR(20) NULL,
  `patronymic_name` VARCHAR(20) NULL,
  `card_key` VARCHAR(16) NULL DEFAULT NULL,
  `object_id` INT UNSIGNED NULL DEFAULT NULL,
  `department_id` INT UNSIGNED NULL DEFAULT NULL,
  `hire_date` DATE NULL DEFAULT NULL,
  `change_of_position_date` DATE NULL DEFAULT NULL,
  `dismiss_date` DATE NULL DEFAULT NULL,
  `post_id` INT UNSIGNED NULL DEFAULT NULL,
  `leader_id` INT UNSIGNED NULL DEFAULT NULL,
  `sex` ENUM('F','M') NULL DEFAULT NULL,
  `fill_date` DATE NULL DEFAULT NULL,
  `user_id` INT UNSIGNED NULL DEFAULT NULL,
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
  `phone_number` VARCHAR(16) NULL DEFAULT NULL,
  `lk_registered` TINYINT(1) NOT NULL DEFAULT 0,
  `photo` MEDIUMBLOB NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_wear_cards_object_idx` (`object_id` ASC),
  INDEX `fk_wear_cards_post_idx` (`post_id` ASC),
  INDEX `fk_wear_cards_leader_idx` (`leader_id` ASC),
  INDEX `fk_wear_cards_user_idx` (`user_id` ASC),
  INDEX `fk_wear_cards_department_idx` (`department_id` ASC),
  UNIQUE INDEX `card_key_UNIQUE` (`card_key` ASC),
  UNIQUE INDEX `personnel_number_UNIQUE` (`personnel_number` ASC, `dismiss_date` ASC),
  UNIQUE INDEX `card_number_UNIQUE` (`card_number` ASC),
  CONSTRAINT `fk_wear_cards_object`
    FOREIGN KEY (`object_id`)
    REFERENCES `objects` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_post`
    FOREIGN KEY (`post_id`)
    REFERENCES `posts` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_leader`
    FOREIGN KEY (`leader_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_user`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_department`
    FOREIGN KEY (`department_id`)
    REFERENCES `departments` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `leaders`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `leaders` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `surname` VARCHAR(50) NULL DEFAULT NULL,
  `name` VARCHAR(50) NULL DEFAULT NULL,
  `patronymic` VARCHAR(50) NULL DEFAULT NULL,
  `position` VARCHAR(150) NULL DEFAULT NULL,
  `employee_id` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_leaders_1_idx` (`employee_id` ASC),
  CONSTRAINT `fk_leaders_1`
    FOREIGN KEY (`employee_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;


-- -----------------------------------------------------
-- Table `measurement_units`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `measurement_units` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(10) NOT NULL,
  `digits` TINYINT UNSIGNED NOT NULL DEFAULT 0,
  `okei` VARCHAR(3) NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `item_types`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `item_types` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(240) NOT NULL,
  `category` ENUM('wear', 'property') NULL DEFAULT 'wear',
  `wear_category` ENUM('Wear', 'Shoes', 'WinterShoes', 'Headgear', 'Gloves', 'Mittens', 'PPE') NULL DEFAULT NULL,
  `issue_type` ENUM('Personal', 'Collective') NOT NULL DEFAULT 'Personal',
  `units_id` INT UNSIGNED NULL DEFAULT NULL,
  `norm_life` INT UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_item_types_1_idx` (`units_id` ASC),
  CONSTRAINT `fk_item_types_1`
    FOREIGN KEY (`units_id`)
    REFERENCES `measurement_units` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;


-- -----------------------------------------------------
-- Table `nomenclature`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `nomenclature` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(240) NOT NULL,
  `type_id` INT UNSIGNED NULL DEFAULT NULL,
  `sex` ENUM('Women','Men', 'Universal') NULL DEFAULT NULL,
  `size_std` VARCHAR(20) NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  `number` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_nomenclature_type_idx` (`type_id` ASC),
  CONSTRAINT `fk_nomenclature_type`
    FOREIGN KEY (`type_id`)
    REFERENCES `item_types` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `stock_income`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_income` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `operation` ENUM('Enter','Return','Object') NOT NULL,
  `number` VARCHAR(8) NULL DEFAULT NULL,
  `date` DATE NOT NULL,
  `warehouse_id` INT(10) UNSIGNED NOT NULL,
  `wear_card_id` INT UNSIGNED NULL DEFAULT NULL,
  `user_id` INT UNSIGNED NULL DEFAULT NULL,
  `object_id` INT UNSIGNED NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_income_wear_card_idx` (`wear_card_id` ASC),
  INDEX `fk_stock_income_user_idx` (`user_id` ASC),
  INDEX `fk_stock_income_object_idx` (`object_id` ASC),
  INDEX `fk_stock_income_1_idx` (`warehouse_id` ASC),
  CONSTRAINT `fk_stock_income_1`
    FOREIGN KEY (`warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_income_object`
    FOREIGN KEY (`object_id`)
    REFERENCES `objects` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_income_wear_card`
    FOREIGN KEY (`wear_card_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_income_user`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `operation_warehouse`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `operation_warehouse` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `operation_time` DATETIME NOT NULL,
  `warehouse_receipt_id` INT UNSIGNED NULL,
  `warehouse_expense_id` INT UNSIGNED NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  `size` VARCHAR(10) NULL,
  `growth` VARCHAR(10) NULL,
  `amount` INT UNSIGNED NOT NULL,
  `wear_percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 0.00,
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
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `regulations`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `regulations` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(255) NOT NULL,
  `number` VARCHAR(10) NULL DEFAULT NULL,
  `doc_date` DATE NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
AUTO_INCREMENT = 1000;


-- -----------------------------------------------------
-- Table `regulations_annex`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `regulations_annex` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `regulations_id` INT UNSIGNED NOT NULL,
  `number` TINYINT(4) NOT NULL,
  `name` VARCHAR(255) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_regulations_appendix_1_idx` (`regulations_id` ASC),
  CONSTRAINT `fk_regulations_appendix_1`
    FOREIGN KEY (`regulations_id`)
    REFERENCES `regulations` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 10000;


-- -----------------------------------------------------
-- Table `norms`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `norms` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `regulations_id` INT UNSIGNED NULL DEFAULT NULL,
  `regulations_annex_id` INT UNSIGNED NULL DEFAULT NULL,
  `name` VARCHAR(200) NULL DEFAULT NULL,
  `ton_paragraph` VARCHAR(15) NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  `datefrom` DATETIME NULL DEFAULT NULL,
  `dateto` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_norms_1_idx` (`regulations_id` ASC),
  INDEX `fk_norms_2_idx` (`regulations_annex_id` ASC),
  CONSTRAINT `fk_norms_1`
    FOREIGN KEY (`regulations_id`)
    REFERENCES `regulations` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_norms_2`
    FOREIGN KEY (`regulations_annex_id`)
    REFERENCES `regulations_annex` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `protection_tools`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `protection_tools` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(240) NOT NULL,
  `item_types_id` INT UNSIGNED NOT NULL DEFAULT 1,
  `assessed_cost` DECIMAL(10,2) UNSIGNED NULL DEFAULT NULL,
  `comments` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_protection_tools_1_idx` (`item_types_id` ASC),
  CONSTRAINT `fk_protection_tools_1`
    FOREIGN KEY (`item_types_id`)
    REFERENCES `item_types` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `norm_conditions`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `norm_conditions` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(100) NOT NULL,
  `sex` ENUM('ForAll', 'OnlyMen', 'OnlyWomen') NOT NULL DEFAULT 'ForAll',
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `norms_item`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `norms_item` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `norm_id` INT UNSIGNED NOT NULL,
  `protection_tools_id` INT UNSIGNED NOT NULL,
  `amount` SMALLINT UNSIGNED NOT NULL DEFAULT 1,
  `period_type` ENUM('Year', 'Month', 'Shift', 'Wearout') NOT NULL DEFAULT 'Year',
  `period_count` TINYINT UNSIGNED NOT NULL DEFAULT 1,
  `condition_id` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_norms_item_1_idx` (`norm_id` ASC),
  INDEX `fk_norms_item_2_idx` (`protection_tools_id` ASC),
  INDEX `fk_norms_item_3_idx` (`condition_id` ASC),
  CONSTRAINT `fk_norms_item_1`
    FOREIGN KEY (`norm_id`)
    REFERENCES `norms` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_norms_item_2`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_norms_item_3`
    FOREIGN KEY (`condition_id`)
    REFERENCES `norm_conditions` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `operation_issued_by_employee`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `operation_issued_by_employee` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `employee_id` INT UNSIGNED NOT NULL,
  `operation_time` DATETIME NOT NULL,
  `nomenclature_id` INT UNSIGNED NULL DEFAULT NULL,
  `size` VARCHAR(10) NULL DEFAULT NULL,
  `growth` VARCHAR(10) NULL DEFAULT NULL,
  `wear_percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 1.00,
  `issued` INT NOT NULL DEFAULT 0,
  `returned` INT NOT NULL DEFAULT 0,
  `auto_writeoff` TINYINT(1) NOT NULL DEFAULT 1,
  `auto_writeoff_date` DATE NULL DEFAULT NULL,
  `protection_tools_id` INT UNSIGNED NULL DEFAULT NULL,
  `norm_item_id` INT UNSIGNED NULL DEFAULT NULL,
  `StartOfUse` DATE NULL DEFAULT NULL,
  `ExpiryByNorm` DATE NULL DEFAULT NULL,
  `issued_operation_id` INT UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `buh_document` VARCHAR(80) NULL DEFAULT NULL,
  `operation_write_off_id` INT UNSIGNED NULL DEFAULT NULL,
  `sign_key` VARCHAR(16) NULL DEFAULT NULL,
  `sign_timestamp` DATETIME NULL DEFAULT NULL,
  `manual_operation` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  INDEX `fk_operation_issued_by_employee_1_idx` (`employee_id` ASC),
  INDEX `fk_operation_issued_by_employee_2_idx` (`nomenclature_id` ASC),
  INDEX `fk_operation_issued_by_employee_3_idx` (`issued_operation_id` ASC),
  INDEX `operation_issued_by_employee_date` (`operation_time` ASC),
  INDEX `fk_operation_issued_by_employee_5_idx` (`norm_item_id` ASC),
  INDEX `fk_operation_issued_by_employee_4_idx` (`warehouse_operation_id` ASC),
  INDEX `index8` (`size` ASC),
  INDEX `index9` (`growth` ASC),
  INDEX `index10` (`wear_percent` ASC),
  INDEX `fk_operation_issued_by_employee_protection_tools_idx` (`protection_tools_id` ASC),
  INDEX `fk_operation_issued_by_employee_6_idx` (`operation_write_off_id` ASC),
  CONSTRAINT `fk_operation_issued_by_employee_1`
    FOREIGN KEY (`employee_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_2`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_3`
    FOREIGN KEY (`issued_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_4`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_5`
    FOREIGN KEY (`norm_item_id`)
    REFERENCES `norms_item` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_protection_tools`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_6`
    FOREIGN KEY (`operation_write_off_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `object_places`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `object_places` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  `object_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_object_places_object_id_idx` (`object_id` ASC),
  CONSTRAINT `fk_object_places_object_id`
    FOREIGN KEY (`object_id`)
    REFERENCES `objects` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;


-- -----------------------------------------------------
-- Table `operation_issued_in_subdivision`
-- -----------------------------------------------------
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
  `warehouse_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL,
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
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_in_subdivision_4`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_in_subdivision_5`
    FOREIGN KEY (`subdivision_place_id`)
    REFERENCES `object_places` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;


-- -----------------------------------------------------
-- Table `stock_income_detail`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_income_detail` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_income_id` INT UNSIGNED NOT NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  `quantity` INT UNSIGNED NOT NULL,
  `cost` DECIMAL(10,2) UNSIGNED NOT NULL DEFAULT 0,
  `certificate` VARCHAR(40) NULL DEFAULT NULL,
  `employee_issue_operation_id` INT UNSIGNED NULL DEFAULT NULL,
  `subdivision_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT UNSIGNED NOT NULL,
  `size` VARCHAR(10) NULL DEFAULT NULL,
  `growth` VARCHAR(10) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_income_detail_stock_income_idx` (`stock_income_id` ASC),
  INDEX `fk_stock_income_detail_nomenclature_idx` (`nomenclature_id` ASC),
  INDEX `fk_stock_income_detail_1_idx` (`employee_issue_operation_id` ASC),
  INDEX `fk_stock_income_detail_2_idx` (`warehouse_operation_id` ASC),
  INDEX `fk_stock_income_detail_3_idx` (`subdivision_issue_operation_id` ASC),
  CONSTRAINT `fk_stock_income_detail_1`
    FOREIGN KEY (`employee_issue_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_income_detail_2`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_income_detail_3`
    FOREIGN KEY (`subdivision_issue_operation_id`)
    REFERENCES `operation_issued_in_subdivision` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_income_detail_nomenclature`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_income_detail_stock_income`
    FOREIGN KEY (`stock_income_id`)
    REFERENCES `stock_income` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `stock_write_off`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_write_off` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `date` DATE NOT NULL,
  `user_id` INT UNSIGNED NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_write_off_user_idx` (`user_id` ASC),
  CONSTRAINT `fk_stock_write_off_user`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `stock_expense`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_expense` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `operation` ENUM('Employee','Object') NOT NULL DEFAULT 'Employee',
  `warehouse_id` INT(10) UNSIGNED NOT NULL,
  `wear_card_id` INT UNSIGNED NULL DEFAULT NULL,
  `object_id` INT UNSIGNED NULL DEFAULT NULL,
  `date` DATE NOT NULL,
  `user_id` INT UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  `write_off_doc` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_expense_wear_card_idx` (`wear_card_id` ASC),
  INDEX `fk_stock_expense_user_idx` (`user_id` ASC),
  INDEX `fk_stock_expense_object_id_idx` (`object_id` ASC),
  INDEX `fk_stock_expense_1_idx` (`warehouse_id` ASC),
  INDEX `fk_stock_expense_2_idx` (`write_off_doc` ASC),
  CONSTRAINT `fk_stock_expense_1`
    FOREIGN KEY (`warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_object_id`
    FOREIGN KEY (`object_id`)
    REFERENCES `objects` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_wear_card`
    FOREIGN KEY (`wear_card_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_user`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_2`
    FOREIGN KEY (`write_off_doc`)
    REFERENCES `stock_write_off` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `stock_expense_detail`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_expense_detail` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_expense_id` INT UNSIGNED NOT NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  `quantity` INT UNSIGNED NOT NULL,
  `object_place_id` INT UNSIGNED NULL DEFAULT NULL,
  `employee_issue_operation_id` INT UNSIGNED NULL DEFAULT NULL,
  `subdivision_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT UNSIGNED NOT NULL,
  `size` VARCHAR(10) NULL DEFAULT NULL,
  `growth` VARCHAR(10) NULL DEFAULT NULL,
  `protection_tools_id` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_expense_detail_stock_expense_idx` (`stock_expense_id` ASC),
  INDEX `fk_stock_expense_detail_nomenclature_idx` (`nomenclature_id` ASC),
  INDEX `fk_stock_expense_detail_placement_idx` (`object_place_id` ASC),
  INDEX `fk_stock_expense_detail_1_idx` (`employee_issue_operation_id` ASC),
  INDEX `fk_stock_expense_detail_2_idx` (`warehouse_operation_id` ASC),
  INDEX `fk_stock_expense_detail_3_idx` (`subdivision_issue_operation_id` ASC),
  INDEX `fk_stock_expense_detail_4_idx` (`protection_tools_id` ASC),
  CONSTRAINT `fk_stock_expense_detail_1`
    FOREIGN KEY (`employee_issue_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_detail_2`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_expense_detail_3`
    FOREIGN KEY (`subdivision_issue_operation_id`)
    REFERENCES `operation_issued_in_subdivision` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_detail_nomenclature`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_detail_placement`
    FOREIGN KEY (`object_place_id`)
    REFERENCES `object_places` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_detail_stock_expense`
    FOREIGN KEY (`stock_expense_id`)
    REFERENCES `stock_expense` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_detail_4`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `stock_write_off_detail`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_write_off_detail` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_write_off_id` INT UNSIGNED NOT NULL,
  `nomenclature_id` INT UNSIGNED NULL DEFAULT NULL,
  `quantity` INT UNSIGNED NOT NULL,
  `employee_issue_operation_id` INT UNSIGNED NULL DEFAULT NULL,
  `subdivision_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT UNSIGNED NULL DEFAULT NULL,
  `size` VARCHAR(10) NULL DEFAULT NULL,
  `growth` VARCHAR(10) NULL DEFAULT NULL,
  `akt_number` VARCHAR(45) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_write_off_detail_write_off_idx` (`stock_write_off_id` ASC),
  INDEX `fk_stock_write_off_detail_nomenclature_idx` (`nomenclature_id` ASC),
  INDEX `fk_stock_write_off_detail_1_idx` (`employee_issue_operation_id` ASC),
  INDEX `fk_stock_write_off_detail_2_idx` (`warehouse_operation_id` ASC),
  INDEX `fk_stock_write_off_detail_3_idx` (`warehouse_id` ASC),
  INDEX `fk_stock_write_off_detail_4_idx` (`subdivision_issue_operation_id` ASC),
  CONSTRAINT `fk_stock_write_off_detail_1`
    FOREIGN KEY (`employee_issue_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_write_off_detail_2`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_write_off_detail_3`
    FOREIGN KEY (`warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_write_off_detail_4`
    FOREIGN KEY (`subdivision_issue_operation_id`)
    REFERENCES `operation_issued_in_subdivision` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_write_off_detail_nomenclature`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_write_off_detail_write_off`
    FOREIGN KEY (`stock_write_off_id`)
    REFERENCES `stock_write_off` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `read_news`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `read_news` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NULL DEFAULT NULL,
  `feed_id` VARCHAR(64) NOT NULL,
  `items` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_read_news_user_id_idx` (`user_id` ASC),
  CONSTRAINT `fk_read_news_user_id`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `norms_professions`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `norms_professions` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `norm_id` INT UNSIGNED NOT NULL,
  `profession_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_norms_professions_1_idx` (`norm_id` ASC),
  INDEX `fk_norms_professions_2_idx` (`profession_id` ASC),
  CONSTRAINT `fk_norms_professions_1`
    FOREIGN KEY (`norm_id`)
    REFERENCES `norms` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_norms_professions_2`
    FOREIGN KEY (`profession_id`)
    REFERENCES `posts` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `wear_cards_norms`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `wear_cards_norms` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `wear_card_id` INT UNSIGNED NOT NULL,
  `norm_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_wear_cards_norms_1_idx` (`wear_card_id` ASC),
  INDEX `fk_wear_cards_norms_2_idx` (`norm_id` ASC),
  CONSTRAINT `fk_wear_cards_norms_1`
    FOREIGN KEY (`wear_card_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_norms_2`
    FOREIGN KEY (`norm_id`)
    REFERENCES `norms` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `wear_cards_item`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `wear_cards_item` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `wear_card_id` INT UNSIGNED NOT NULL,
  `protection_tools_id` INT UNSIGNED NOT NULL,
  `norm_item_id` INT UNSIGNED NULL,
  `created` DATE NULL,
  `last_issue` DATE NULL,
  `next_issue` DATE NULL,
  `amount` INT NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  INDEX `fk_wear_cards_item_1_idx` (`wear_card_id` ASC),
  INDEX `fk_wear_cards_item_3_idx` (`norm_item_id` ASC),
  INDEX `fk_wear_cards_item_2_idx` (`protection_tools_id` ASC),
  CONSTRAINT `fk_wear_cards_item_1`
    FOREIGN KEY (`wear_card_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_item_2`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_item_3`
    FOREIGN KEY (`norm_item_id`)
    REFERENCES `norms_item` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `organizations`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `organizations` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(300) NULL,
  `address` VARCHAR(300) NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `user_settings`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `user_settings` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL,
  `toolbar_style` ENUM('Text', 'Icons', 'Both') NOT NULL DEFAULT 'Both',
  `toolbar_icons_size` ENUM('ExtraSmall', 'Small', 'Middle', 'Large') NOT NULL DEFAULT 'Middle',
  `toolbar_show` TINYINT(1) NOT NULL DEFAULT 1,
  `maximize_on_start` TINYINT(1) NOT NULL DEFAULT 1,
  `default_warehouse_id` INT UNSIGNED NULL,
  `default_organization_id` INT UNSIGNED NULL,
  `default_responsible_person_id` INT UNSIGNED NULL,
  `default_leader_id` INT UNSIGNED NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_user_settings_1_idx` (`user_id` ASC),
  INDEX `fk_user_settings_warehouse_id_idx` (`default_warehouse_id` ASC),
  INDEX `fk_user_settings_organization_id_idx` (`default_organization_id` ASC),
  INDEX `fk_user_settings_leader_id_idx` (`default_leader_id` ASC),
  INDEX `fk_user_settings_responsible_person_id_idx` (`default_responsible_person_id` ASC),
  CONSTRAINT `fk_user_settings_1`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_user_settings_warehouse_id`
    FOREIGN KEY (`default_warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_user_settings_organization_id`
    FOREIGN KEY (`default_organization_id`)
    REFERENCES `organizations` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_user_settings_responsible_person_id`
    FOREIGN KEY (`default_responsible_person_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_user_settings_leader_id`
    FOREIGN KEY (`default_leader_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `vacation_type`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `vacation_type` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  `exclude_from_wearing` TINYINT(1) NOT NULL DEFAULT 0,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `wear_cards_vacations`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `wear_cards_vacations` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `wear_card_id` INT UNSIGNED NOT NULL,
  `vacation_type_id` INT UNSIGNED NOT NULL,
  `begin_date` DATE NOT NULL,
  `end_date` DATE NOT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_wear_cards_vacations_1_idx` (`wear_card_id` ASC),
  INDEX `fk_wear_cards_vacations_2_idx` (`vacation_type_id` ASC),
  CONSTRAINT `fk_wear_cards_vacations_1`
    FOREIGN KEY (`wear_card_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_vacations_2`
    FOREIGN KEY (`vacation_type_id`)
    REFERENCES `vacation_type` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_mass_expense`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_mass_expense` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `date` DATETIME NOT NULL,
  `warehouse_id` INT UNSIGNED NOT NULL DEFAULT 1,
  `user_id` INT UNSIGNED NOT NULL,
  `comment` TEXT NULL,
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
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_collective_expense`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_collective_expense` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `warehouse_id` INT(10) UNSIGNED NOT NULL,
  `date` DATE NOT NULL,
  `user_id` INT UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_expense_user_idx` (`user_id` ASC),
  INDEX `fk_stock_expense_1_idx` (`warehouse_id` ASC),
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


-- -----------------------------------------------------
-- Table `issuance_sheet`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `issuance_sheet` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `date` DATE NOT NULL,
  `organization_id` INT UNSIGNED NULL DEFAULT NULL,
  `subdivision_id` INT UNSIGNED NULL DEFAULT NULL,
  `responsible_person_id` INT UNSIGNED NULL DEFAULT NULL,
  `head_of_division_person_id` INT UNSIGNED NULL DEFAULT NULL,
  `stock_expense_id` INT UNSIGNED NULL DEFAULT NULL,
  `stock_mass_expense_id` INT UNSIGNED NULL DEFAULT NULL,
  `stock_collective_expense_id` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_issuance_sheet_1_idx` (`organization_id` ASC),
  INDEX `fk_issuance_sheet_3_idx` (`responsible_person_id` ASC),
  INDEX `fk_issuance_sheet_4_idx` (`head_of_division_person_id` ASC),
  INDEX `fk_issuance_sheet_5_idx` (`stock_expense_id` ASC),
  INDEX `fk_issuance_sheet_2_idx` (`subdivision_id` ASC),
  INDEX `fk_issuance_sheet_6_idx` (`stock_mass_expense_id` ASC),
  INDEX `fk_issuance_sheet_7_idx` (`stock_collective_expense_id` ASC),
  CONSTRAINT `fk_issuance_sheet_1`
    FOREIGN KEY (`organization_id`)
    REFERENCES `organizations` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_2`
    FOREIGN KEY (`subdivision_id`)
    REFERENCES `objects` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_3`
    FOREIGN KEY (`responsible_person_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_4`
    FOREIGN KEY (`head_of_division_person_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_5`
    FOREIGN KEY (`stock_expense_id`)
    REFERENCES `stock_expense` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_6`
    FOREIGN KEY (`stock_mass_expense_id`)
    REFERENCES `stock_mass_expense` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_7`
    FOREIGN KEY (`stock_collective_expense_id`)
    REFERENCES `stock_collective_expense` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_collective_expense_detail`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_collective_expense_detail` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_collective_expense_id` INT UNSIGNED NOT NULL,
  `employee_id` INT UNSIGNED NOT NULL,
  `protection_tools_id` INT UNSIGNED NULL DEFAULT NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  `quantity` INT UNSIGNED NOT NULL,
  `employee_issue_operation_id` INT UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT UNSIGNED NOT NULL,
  `size` VARCHAR(10) NULL DEFAULT NULL,
  `growth` VARCHAR(10) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_expense_detail_nomenclature_idx` (`nomenclature_id` ASC),
  INDEX `fk_stock_expense_detail_1_idx` (`employee_issue_operation_id` ASC),
  INDEX `fk_stock_expense_detail_2_idx` (`warehouse_operation_id` ASC),
  INDEX `fk_stock_expense_detail_4_idx` (`protection_tools_id` ASC),
  INDEX `fk_stock_collective_expense_detail_4_idx` (`stock_collective_expense_id` ASC),
  INDEX `fk_stock_collective_expense_detail_6_idx` (`employee_id` ASC),
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


-- -----------------------------------------------------
-- Table `issuance_sheet_items`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `issuance_sheet_items` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `issuance_sheet_id` INT UNSIGNED NOT NULL,
  `employee_id` INT UNSIGNED NOT NULL,
  `nomenclature_id` INT UNSIGNED NULL DEFAULT NULL,
  `protection_tools_id` INT UNSIGNED NULL DEFAULT NULL,
  `stock_expense_detail_id` INT UNSIGNED NULL DEFAULT NULL,
  `stock_collective_expense_item_id` INT UNSIGNED NULL DEFAULT NULL,
  `issued_operation_id` INT UNSIGNED NULL,
  `amount` SMALLINT UNSIGNED NOT NULL,
  `start_of_use` DATE NULL DEFAULT NULL,
  `lifetime` DECIMAL(5,2) UNSIGNED NULL DEFAULT NULL,
  `size` VARCHAR(10) NULL DEFAULT NULL,
  `growth` VARCHAR(10) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_issuance_sheet_items_1_idx` (`issuance_sheet_id` ASC),
  INDEX `fk_issuance_sheet_items_2_idx` (`employee_id` ASC),
  INDEX `fk_issuance_sheet_items_3_idx` (`nomenclature_id` ASC),
  INDEX `fk_issuance_sheet_items_4_idx` (`issued_operation_id` ASC),
  INDEX `fk_issuance_sheet_items_5_idx` (`stock_expense_detail_id` ASC),
  INDEX `fk_issuance_sheet_items_6_idx` (`protection_tools_id` ASC),
  INDEX `fk_issuance_sheet_items_7_idx` (`stock_collective_expense_item_id` ASC),
  CONSTRAINT `fk_issuance_sheet_items_1`
    FOREIGN KEY (`issuance_sheet_id`)
    REFERENCES `issuance_sheet` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_2`
    FOREIGN KEY (`employee_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_3`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_4`
    FOREIGN KEY (`issued_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_5`
    FOREIGN KEY (`stock_expense_detail_id`)
    REFERENCES `stock_expense_detail` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_6`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_7`
    FOREIGN KEY (`stock_collective_expense_item_id`)
    REFERENCES `stock_collective_expense_detail` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_mass_expense_employee`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_mass_expense_employee` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `employee_id` INT UNSIGNED NOT NULL,
  `stock_mass_expense_id` INT UNSIGNED NOT NULL,
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
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_mass_expense_nomenclatures`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_mass_expense_nomenclatures` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  `quantity` INT UNSIGNED NOT NULL,
  `stock_mass_expense_id` INT UNSIGNED NOT NULL,
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
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_mass_expense_operation`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_mass_expense_operation` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `operation_warehouse_id` INT UNSIGNED NOT NULL,
  `operation_issued_by_employee` INT UNSIGNED NOT NULL,
  `stock_mass_expense_id` INT UNSIGNED NOT NULL,
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
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_transfer`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_transfer` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `warehouse_from_id` INT UNSIGNED NOT NULL,
  `warehouse_to_id` INT UNSIGNED NOT NULL,
  `date` DATETIME NOT NULL,
  `user_id` INT(10) UNSIGNED NULL,
  `comment` TEXT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `id_UNIQUE` (`id` ASC),
  INDEX `fk_stock_transfer_1_idx` (`warehouse_from_id` ASC),
  INDEX `fk_stock_transfer_2_idx` (`warehouse_to_id` ASC),
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
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_transfer_detail`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_transfer_detail` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_transfer_id` INT UNSIGNED NOT NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  `quantity` INT UNSIGNED NULL,
  `warehouse_operation_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `id_UNIQUE` (`id` ASC),
  INDEX `fk_stock_transfer_detail_1_idx` (`stock_transfer_id` ASC),
  INDEX `fk_stock_transfer_detail_2_idx` (`nomenclature_id` ASC),
  INDEX `fk_stock_transfer_detail_3_idx` (`warehouse_operation_id` ASC),
  CONSTRAINT `fk_stock_transfer_detail_1`
    FOREIGN KEY (`stock_transfer_id`)
    REFERENCES `stock_transfer` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_detail_2`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_detail_3`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `protection_tools_replacement`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `protection_tools_replacement` (
  `protection_tools_id` INT UNSIGNED NOT NULL,
  `protection_tools_analog_id` INT UNSIGNED NOT NULL,
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
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `protection_tools_nomenclature`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `protection_tools_nomenclature` (
  `protection_tools_id` INT UNSIGNED NOT NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`protection_tools_id`, `nomenclature_id`),
  INDEX `fk_protection_tools_nomenclature_2_idx` (`nomenclature_id` ASC),
  CONSTRAINT `fk_protection_tools_nomenclature_1`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_protection_tools_nomenclature_2`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `message_templates`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `message_templates` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(100) NOT NULL,
  `message_title` VARCHAR(200) NOT NULL,
  `message_text` VARCHAR(400) NOT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- function count_issue
-- -----------------------------------------------------

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

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;

-- -----------------------------------------------------
-- Data for table `base_parameters`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `base_parameters` (`name`, `str_value`) VALUES ('product_name', 'workwear');
INSERT INTO `base_parameters` (`name`, `str_value`) VALUES ('version', '2.6.1');
INSERT INTO `base_parameters` (`name`, `str_value`) VALUES ('DefaultAutoWriteoff', 'True');

COMMIT;


-- -----------------------------------------------------
-- Data for table `warehouse`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `warehouse` (`id`, `name`) VALUES (DEFAULT, 'Основной склад');

COMMIT;


-- -----------------------------------------------------
-- Data for table `measurement_units`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `measurement_units` (`id`, `name`, `digits`, `okei`) VALUES (1, 'шт.', 0, '796');
INSERT INTO `measurement_units` (`id`, `name`, `digits`, `okei`) VALUES (2, 'пара', 0, '715');
INSERT INTO `measurement_units` (`id`, `name`, `digits`, `okei`) VALUES (3, 'компл.', 0, '839');
INSERT INTO `measurement_units` (`id`, `name`, `digits`, `okei`) VALUES (4, 'набор', 0, '704');

COMMIT;


-- -----------------------------------------------------
-- Data for table `item_types`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `item_types` (`id`, `name`, `category`, `wear_category`, `units_id`, `norm_life`, `comment`) VALUES (DEFAULT, 'Одежда', 'wear', 'Wear', 1, NULL, NULL);
INSERT INTO `item_types` (`id`, `name`, `category`, `wear_category`, `units_id`, `norm_life`, `comment`) VALUES (DEFAULT, 'Обувь', 'wear', 'Shoes', 2, NULL, NULL);
INSERT INTO `item_types` (`id`, `name`, `category`, `wear_category`, `units_id`, `norm_life`, `comment`) VALUES (DEFAULT, 'Зимняя обувь', 'wear', 'WinterShoes', 2, NULL, NULL);
INSERT INTO `item_types` (`id`, `name`, `category`, `wear_category`, `units_id`, `norm_life`, `comment`) VALUES (DEFAULT, 'Головные уборы', 'wear', 'Headgear', 1, NULL, NULL);
INSERT INTO `item_types` (`id`, `name`, `category`, `wear_category`, `units_id`, `norm_life`, `comment`) VALUES (DEFAULT, 'Перчатки', 'wear', 'Gloves', 2, NULL, NULL);
INSERT INTO `item_types` (`id`, `name`, `category`, `wear_category`, `units_id`, `norm_life`, `comment`) VALUES (DEFAULT, 'Варежки', 'wear', 'Mittens', 2, NULL, NULL);
INSERT INTO `item_types` (`id`, `name`, `category`, `wear_category`, `units_id`, `norm_life`, `comment`) VALUES (DEFAULT, 'СИЗ', 'wear', 'PPE', 1, NULL, NULL);

COMMIT;


-- -----------------------------------------------------
-- Data for table `organizations`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `organizations` (`id`, `name`, `address`) VALUES (DEFAULT, 'Моя организация', NULL);

COMMIT;


-- -----------------------------------------------------
-- Data for table `vacation_type`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `vacation_type` (`id`, `name`, `exclude_from_wearing`, `comment`) VALUES (1, 'Основной', 0, NULL);

COMMIT;
