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
