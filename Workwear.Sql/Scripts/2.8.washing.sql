-- Учет стирки
CREATE TABLE IF NOT EXISTS `clothing_service_states` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `clame_id` INT(10) UNSIGNED NOT NULL,
  `operation_time` DATETIME NOT NULL,
  `state` ENUM('WaitService', 'InTransit', 'InRepair', 'InWashing', 'AwaitIssue') NOT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `clothing_service_claim` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `barcode_id` INT(11) NULL DEFAULT NULL,
  `need_for_repair` TINYINT(1) NOT NULL,
  `defect` TEXT NULL DEFAULT NULL COMMENT 'Описание дефекта при сдаче, который нужно починить.',
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;
