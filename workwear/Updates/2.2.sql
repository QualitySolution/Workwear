CREATE TABLE IF NOT EXISTS `workwear`.`vacation_type` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  `exclude_from_wearing` TINYINT(1) NOT NULL DEFAULT 0,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;

INSERT INTO `vacation_type` (`id`, `name`, `exclude_from_wearing`, `comment`) VALUES (1, 'Основной', 0, NULL);

CREATE TABLE IF NOT EXISTS `workwear`.`wear_cards_vacations` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `wear_card_id` INT(10) UNSIGNED NOT NULL,
  `vacation_type_id` INT(10) UNSIGNED NOT NULL,
  `begin_date` DATE NOT NULL,
  `end_date` DATE NOT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_wear_cards_vacations_1_idx` (`wear_card_id` ASC),
  INDEX `fk_wear_cards_vacations_2_idx` (`vacation_type_id` ASC),
  CONSTRAINT `fk_wear_cards_vacations_1`
    FOREIGN KEY (`wear_card_id`)
    REFERENCES `workwear`.`wear_cards` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_vacations_2`
    FOREIGN KEY (`vacation_type_id`)
    REFERENCES `workwear`.`vacation_type` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8;

INSERT INTO wear_cards_vacations (wear_card_id, vacation_type_id, begin_date, end_date, comment)
SELECT wear_cards.id, 1, wear_cards.maternity_leave_begin, wear_cards.maternity_leave_end, "Автоматически перенесенный отпуск из версии программы 2.1"
FROM wear_cards 
WHERE wear_cards.maternity_leave_begin IS NOT NULL AND wear_cards.maternity_leave_end IS NOT NULL;

ALTER TABLE `workwear`.`wear_cards` 
DROP COLUMN `maternity_leave_end`,
DROP COLUMN `maternity_leave_begin`;

-- Добавляем информацию о том что база была созданна до 2.2 
INSERT INTO base_parameters (`name`, `str_value`) VALUES ('InstalledBefore2.2', 'True');

-- Обновляем версию базы.
DELETE FROM base_parameters WHERE name = 'micro_updates';
UPDATE base_parameters SET str_value = '2.2' WHERE name = 'version';