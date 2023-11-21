-- Добавляем поле "Можно отдать в стирку" в номенклатуру
ALTER TABLE `nomenclature` ADD `washable` BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'Можно отдать в стирку' AFTER `use_barcode`;

-- Сдача с стрирку из постомата
ALTER TABLE `clothing_service_states` ADD `terminal_id` INT UNSIGNED NULL DEFAULT NULL COMMENT 'Номер постомата' AFTER `user_id`;

ALTER TABLE `clothing_service_states` CHANGE `state` `state` ENUM('WaitService','InReceiptTerminal','InTransit','InRepair','InWashing','AwaitIssue','Returned') CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL;

-- Версионирование для штрихкодов
ALTER TABLE `barcodes` ADD `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `creation_date`;
ALTER TABLE `barcodes` ADD INDEX(`last_update`);
