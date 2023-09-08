-- Удаляем глупый и бессмысленный индекс
ALTER TABLE `stock_transfer_detail`
    DROP INDEX `id_UNIQUE` ;

-- Добавляем комментарий к операциям
ALTER TABLE `operation_issued_by_employee` ADD `comment` TEXT NULL DEFAULT NULL AFTER `fixed_operation`; 

-- Версионирование для сотрудников
ALTER TABLE `wear_cards` ADD `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `id`;
ALTER TABLE `wear_cards` ADD INDEX(`last_update`); 
