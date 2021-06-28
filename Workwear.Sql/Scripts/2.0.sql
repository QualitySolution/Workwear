ALTER TABLE `item_types` 
CHANGE COLUMN `wear_category` `wear_category` ENUM('Wear', 'Shoes', 'WinterShoes', 'Headgear', 'Gloves', 'PPE') NULL DEFAULT NULL ;

ALTER TABLE `wear_cards` 
ADD COLUMN `size_winter_shoes` VARCHAR(10) NULL DEFAULT NULL AFTER `size_shoes_std`,
ADD COLUMN `size_winter_shoes_std` VARCHAR(20) NULL DEFAULT NULL AFTER `size_winter_shoes`,
ADD COLUMN `maternity_leave_begin` DATE NULL DEFAULT NULL AFTER `size_gloves_std`,
ADD COLUMN `maternity_leave_end` DATE NULL DEFAULT NULL AFTER `maternity_leave_begin`;

CREATE TABLE IF NOT EXISTS `user_settings` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT(10) UNSIGNED NOT NULL,
  `toolbar_style` ENUM('Text', 'Icons', 'Both') NOT NULL DEFAULT 'Both',
  `toolbar_icons_size` ENUM('ExtraSmall', 'Small', 'Middle', 'Large') NOT NULL DEFAULT 'Middle',
  `toolbar_show` TINYINT(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`),
  INDEX `fk_user_settings_1_idx` (`user_id` ASC),
  CONSTRAINT `fk_user_settings_1`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;

-- Обновляем версию базы.

DELETE FROM base_parameters WHERE name = 'micro_updates';
UPDATE base_parameters SET str_value = '2.0' WHERE name = 'version';