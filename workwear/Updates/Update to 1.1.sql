
ALTER TABLE `stock_income` 
DROP FOREIGN KEY `fk_stock_income_object`;

ALTER TABLE `stock_income_detail` 
DROP FOREIGN KEY `fk_stock_income_detail_stock_expense`;

ALTER TABLE `stock_expense` 
DROP FOREIGN KEY `fk_stock_expense_object_id`;

ALTER TABLE `stock_expense_detail` 
DROP FOREIGN KEY `fk_stock_expense_detail_enter_row`;

ALTER TABLE `item_types` 
CHANGE COLUMN `category` `category` ENUM('wear', 'property') NULL DEFAULT 'wear' AFTER `name`,
ADD COLUMN `units_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `category`,
ADD COLUMN `on_norms` TINYINT(1) NOT NULL DEFAULT 1 AFTER `units_id`,
ADD INDEX `fk_item_types_1_idx` (`units_id` ASC);

## Переносим единицы измерения из номенклатуры в тип номенклатуры.
UPDATE item_types, nomenclature SET item_types.units_id = nomenclature.units_id WHERE item_types.id = nomenclature.type_id;

ALTER TABLE `nomenclature` 
DROP FOREIGN KEY `fk_nomenclature_units`;

ALTER TABLE `nomenclature` 
DROP COLUMN `units_id`,
DROP INDEX `fk_nomenclature_units_idx` ;

ALTER TABLE `stock_income_detail` 
CHANGE COLUMN `life_percent` `life_percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 1 ;

CREATE TABLE IF NOT EXISTS `read_news` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `feed_id` VARCHAR(64) NOT NULL,
  `items` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_read_news_user_id_idx` (`user_id` ASC),
  CONSTRAINT `fk_read_news_user_id`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8
COLLATE = utf8_general_ci;

ALTER TABLE `item_types` 
ADD CONSTRAINT `fk_item_types_1`
  FOREIGN KEY (`units_id`)
  REFERENCES `units` (`id`)
  ON DELETE SET NULL
  ON UPDATE CASCADE;

ALTER TABLE `stock_income` 
ADD CONSTRAINT `fk_stock_income_object`
  FOREIGN KEY (`object_id`)
  REFERENCES `objects` (`id`)
  ON DELETE RESTRICT
  ON UPDATE CASCADE;

ALTER TABLE `stock_income_detail` 
ADD CONSTRAINT `fk_stock_income_detail_stock_expense`
  FOREIGN KEY (`stock_expense_detail_id`)
  REFERENCES `stock_expense_detail` (`id`)
  ON DELETE RESTRICT
  ON UPDATE CASCADE;

ALTER TABLE `stock_expense` 
DROP FOREIGN KEY `fk_stock_expense_wear_card`;

ALTER TABLE `stock_expense` ADD CONSTRAINT `fk_stock_expense_wear_card`
  FOREIGN KEY (`wear_card_id`)
  REFERENCES `wear_cards` (`id`)
  ON DELETE RESTRICT
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_stock_expense_object_id`
  FOREIGN KEY (`object_id`)
  REFERENCES `objects` (`id`)
  ON DELETE RESTRICT
  ON UPDATE CASCADE;

ALTER TABLE `stock_expense_detail` 
ADD CONSTRAINT `fk_stock_expense_detail_enter_row`
  FOREIGN KEY (`stock_income_detail_id`)
  REFERENCES `stock_income_detail` (`id`)
  ON DELETE RESTRICT
  ON UPDATE CASCADE;

-- Обновляем версию базы.

DELETE FROM base_parameters WHERE name = 'micro_updates';
UPDATE base_parameters SET str_value = '1.1' WHERE name = 'version';
