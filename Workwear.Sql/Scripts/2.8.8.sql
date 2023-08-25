-- Удаляем глупый и бессмысленный индекс
ALTER TABLE `stock_transfer_detail`
    DROP INDEX `id_UNIQUE` ;

-- Добавляем комментарий к операциям
ALTER TABLE `operation_issued_by_employee` ADD `comment` TEXT NULL DEFAULT NULL AFTER `fixed_operation`; 
