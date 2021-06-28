-- MySQL Workbench Synchronization
-- Generated: 2016-04-05 11:29
-- Model: New Model
-- Version: 1.0
-- Project: Name of the project
-- Author: Ганьков Андрей

ALTER TABLE `item_types` 
DROP FOREIGN KEY `fk_item_types_1`;

ALTER TABLE `posts` 
CHANGE COLUMN `name` `name` VARCHAR(180) NOT NULL ;

ALTER TABLE `nomenclature` 
CHANGE COLUMN `size` `size` VARCHAR(10) NULL DEFAULT NULL ,
CHANGE COLUMN `growth` `growth` VARCHAR(10) NULL DEFAULT NULL ,
ADD COLUMN `sex` ENUM('Women','Men', 'Universal') NULL DEFAULT NULL AFTER `type_id`,
ADD COLUMN `size_std` VARCHAR(20) NULL DEFAULT NULL AFTER `size`,
ADD COLUMN `growth_std` VARCHAR(20) NULL DEFAULT NULL AFTER `growth`;

ALTER TABLE `item_types` 
DROP COLUMN `norm_quantity`,
DROP COLUMN `on_norms`,
CHANGE COLUMN `name` `name` VARCHAR(180) NOT NULL ,
ADD COLUMN `wear_category` ENUM('Wear','Shoes','Headgear','Gloves') NULL DEFAULT NULL AFTER `category`;

ALTER TABLE `units` 
ADD COLUMN `digits` TINYINT(3) UNSIGNED NOT NULL DEFAULT 0 AFTER `name`,
ADD COLUMN `okei` VARCHAR(3) NULL DEFAULT NULL AFTER `digits`, RENAME TO  `measurement_units` ;

ALTER TABLE `wear_cards` 
ADD COLUMN `personnel_number` VARCHAR(15) NULL DEFAULT NULL AFTER `card_number`,
ADD COLUMN `wear_growth` VARCHAR(5) NULL DEFAULT NULL AFTER `user_id`,
ADD COLUMN `size_wear` VARCHAR(10) NULL DEFAULT NULL AFTER `wear_growth`,
ADD COLUMN `size_wear_std` VARCHAR(20) NULL DEFAULT NULL AFTER `size_wear`,
ADD COLUMN `size_shoes` VARCHAR(10) NULL DEFAULT NULL AFTER `size_wear_std`,
ADD COLUMN `size_shoes_std` VARCHAR(20) NULL DEFAULT NULL AFTER `size_shoes`,
ADD COLUMN `size_headdress` VARCHAR(10) NULL DEFAULT NULL AFTER `size_shoes_std`,
ADD COLUMN `size_headdress_std` VARCHAR(20) NULL DEFAULT NULL AFTER `size_headdress`,
ADD COLUMN `size_gloves` VARCHAR(10) NULL DEFAULT NULL AFTER `size_headdress_std`,
ADD COLUMN `size_gloves_std` VARCHAR(20) NULL DEFAULT NULL AFTER `size_gloves`;

-- Пробуем сконвертировать размеры одежды
UPDATE `wear_cards` SET `size_wear` = IF(`wear_size` IN ("38", "XXS", "40", "XS", "42", "S", "44", "M", "46", "48", "L", "50", "52", "XL", "54", "XXL", "56", "58", "XXXL", "60", "4XL", "62", "64", "66", "5XL", "68", "70"), `wear_size`, NULL),
`size_wear_std` = CASE 
WHEN `sex` = 'F' AND `wear_size` IN ("38", "40", "42", "44", "46", "48", "50", "52", "54", "56", "58","60", "62", "64", "66", "68", "70") THEN 'WomenWearRus' 
WHEN `sex` = 'F' AND `wear_size` IN ("XXS", "XS", "S", "M","L", "XL", "XXL","XXXL", "4XL", "5XL") THEN 'WomenWearIntl' 
WHEN `sex` = 'M' AND `wear_size` IN ("44", "46", "48", "50", "52", "54", "56", "58","60", "62", "64", "66", "68", "70") THEN 'MenWearRus' 
WHEN `sex` = 'M' AND `wear_size` IN ("XXS", "XS", "S", "M","L", "XL", "XXL","XXXL", "4XL", "5XL") THEN 'MenWearIntl' END;

-- Пробуем сконвертировать рост.
UPDATE `wear_cards` SET `wear_growth` = CASE 
WHEN `sex` = 'F' AND `growth` BETWEEN '143' AND '149' THEN '146'
WHEN `sex` = 'F' AND `growth` BETWEEN '149' AND '155' THEN '152'
WHEN `sex` = 'F' AND `growth` BETWEEN '155' AND '161' THEN '158'
WHEN `sex` = 'F' AND `growth` BETWEEN '161' AND '167' THEN '164'
WHEN `sex` = 'F' AND `growth` BETWEEN '167' AND '173' THEN '170'
WHEN `sex` = 'F' AND `growth` BETWEEN '173' AND '179' THEN '176'
WHEN `sex` = 'M' AND `growth` BETWEEN '155' AND '161' THEN '158'
WHEN `sex` = 'M' AND `growth` BETWEEN '161' AND '167' THEN '164'
WHEN `sex` = 'M' AND `growth` BETWEEN '167' AND '173' THEN '170'
WHEN `sex` = 'M' AND `growth` BETWEEN '173' AND '179' THEN '176'
WHEN `sex` = 'M' AND `growth` BETWEEN '179' AND '185' THEN '182'
WHEN `sex` = 'M' AND `growth` BETWEEN '185' AND '191' THEN '188'
WHEN `sex` = 'M' AND `growth` BETWEEN '191' AND '197' THEN '194'
END;

ALTER TABLE `wear_cards` 
DROP COLUMN `photo_size`,
DROP COLUMN `growth`,
DROP COLUMN `wear_size`;

ALTER TABLE `stock_income` 
CHANGE COLUMN `operation` `operation` ENUM('Enter','Return','Object') NOT NULL ;

ALTER TABLE `stock_income_detail` 
CHANGE COLUMN `life_percent` `life_percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 1 ;

ALTER TABLE `stock_expense` 
CHANGE COLUMN `operation` `operation` ENUM('Employee','Object') NOT NULL DEFAULT 'Employee' ;

ALTER TABLE `stock_expense_detail` 
ADD COLUMN `auto_writeoff_date` DATE NULL DEFAULT NULL AFTER `object_place_id`;

CREATE TABLE IF NOT EXISTS `norms` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `ton_number` VARCHAR(15) NULL DEFAULT NULL,
  `ton_attachment` VARCHAR(15) NULL DEFAULT NULL,
  `ton_paragraph` VARCHAR(15) NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;

CREATE TABLE IF NOT EXISTS `norms_item` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `norm_id` INT(10) UNSIGNED NOT NULL,
  `itemtype_id` INT(10) UNSIGNED NOT NULL,
  `amount` SMALLINT(5) UNSIGNED NOT NULL DEFAULT 1,
  `period_type` ENUM('Year','Month','Shift') NOT NULL DEFAULT 'Year',
  `period_count` TINYINT(3) UNSIGNED NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`),
  INDEX `fk_norms_item_1_idx` (`norm_id` ASC),
  INDEX `fk_norms_item_2_idx` (`itemtype_id` ASC),
  CONSTRAINT `fk_norms_item_1`
    FOREIGN KEY (`norm_id`)
    REFERENCES `norms` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_norms_item_2`
    FOREIGN KEY (`itemtype_id`)
    REFERENCES `item_types` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;

CREATE TABLE IF NOT EXISTS `norms_professions` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `norm_id` INT(10) UNSIGNED NOT NULL,
  `profession_id` INT(10) UNSIGNED NOT NULL,
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
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;

CREATE TABLE IF NOT EXISTS `wear_cards_norms` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `wear_card_id` INT(10) UNSIGNED NOT NULL,
  `norm_id` INT(10) UNSIGNED NOT NULL,
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
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;

CREATE TABLE IF NOT EXISTS `wear_cards_item` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `wear_card_id` INT(10) UNSIGNED NOT NULL,
  `itemtype_id` INT(10) UNSIGNED NOT NULL,
  `norm_item_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `created` DATE NULL DEFAULT NULL,
  `last_issue` DATE NULL DEFAULT NULL,
  `next_issue` DATE NULL DEFAULT NULL,
  `amount` INT(11) NOT NULL DEFAULT 0,
  `matched_nomenclature_id` INT(11) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_wear_cards_item_1_idx` (`wear_card_id` ASC),
  INDEX `fk_wear_cards_item_2_idx` (`itemtype_id` ASC),
  INDEX `fk_wear_cards_item_3_idx` (`norm_item_id` ASC),
  CONSTRAINT `fk_wear_cards_item_1`
    FOREIGN KEY (`wear_card_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_item_2`
    FOREIGN KEY (`itemtype_id`)
    REFERENCES `item_types` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_item_3`
    FOREIGN KEY (`norm_item_id`)
    REFERENCES `norms_item` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;

ALTER TABLE `item_types` 
ADD CONSTRAINT `fk_item_types_1`
  FOREIGN KEY (`units_id`)
  REFERENCES `measurement_units` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE;

-- Обновляем версию базы.

DELETE FROM base_parameters WHERE name = 'micro_updates';
UPDATE base_parameters SET str_value = '1.2' WHERE name = 'version';
