ALTER TABLE `nomenclature` 
ADD COLUMN `comment` TEXT NULL DEFAULT NULL AFTER `growth_std`;

ALTER TABLE `item_types` 
ADD COLUMN `comment` TEXT NULL DEFAULT NULL AFTER `norm_life`;

ALTER TABLE `wear_cards` 
ADD COLUMN `change_of_position_date` DATE NULL DEFAULT NULL AFTER `hire_date`,
ADD COLUMN `comment` TEXT NULL DEFAULT NULL AFTER `photo`;

ALTER TABLE `stock_income` 
ADD COLUMN `comment` TEXT NULL DEFAULT NULL AFTER `object_id`;

ALTER TABLE `stock_expense` 
ADD COLUMN `comment` TEXT NULL DEFAULT NULL AFTER `user_id`;

ALTER TABLE `stock_write_off` 
ADD COLUMN `comment` TEXT NULL DEFAULT NULL AFTER `user_id`;

ALTER TABLE `norms` 
ADD COLUMN `comment` TEXT NULL DEFAULT NULL AFTER `ton_paragraph`;

ALTER TABLE `norms` 
ADD COLUMN `regulations_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `id`,
ADD COLUMN `regulations_annex_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `regulations_id`,
ADD INDEX `fk_norms_1_idx` (`regulations_id` ASC),
ADD INDEX `fk_norms_2_idx` (`regulations_annex_id` ASC);

CREATE TABLE IF NOT EXISTS `regulations` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` TINYTEXT NOT NULL,
  `number` VARCHAR(10) NULL DEFAULT NULL,
  `doc_date` DATE NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
AUTO_INCREMENT = 1000
DEFAULT CHARACTER SET = utf8;

CREATE TABLE IF NOT EXISTS `regulations_annex` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `regulations_id` INT(10) UNSIGNED NOT NULL,
  `number` TINYINT(4) NOT NULL,
  `name` TINYTEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_regulations_appendix_1_idx` (`regulations_id` ASC),
  CONSTRAINT `fk_regulations_appendix_1`
    FOREIGN KEY (`regulations_id`)
    REFERENCES `regulations` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 10000
DEFAULT CHARACTER SET = utf8;

ALTER TABLE `norms` 
ADD CONSTRAINT `fk_norms_1`
  FOREIGN KEY (`regulations_id`)
  REFERENCES `regulations` (`id`)
  ON DELETE RESTRICT
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_norms_2`
  FOREIGN KEY (`regulations_annex_id`)
  REFERENCES `regulations_annex` (`id`)
  ON DELETE RESTRICT
  ON UPDATE CASCADE;
