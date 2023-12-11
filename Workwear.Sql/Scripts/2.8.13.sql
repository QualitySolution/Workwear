-- Доработка постоматов
ALTER TABLE `postomat_documents` ADD `confirm_time` DATETIME NULL DEFAULT NULL COMMENT 'Время выполнения(done) документа на постомате.' AFTER `create_time`;
ALTER TABLE `postomat_documents` ADD `confirm_user` INT UNSIGNED NULL DEFAULT NULL COMMENT 'Пользователь системы постоматов проводивший документ.' AFTER `confirm_time`;
ALTER TABLE `postomat_document_items` ADD `dispense_time` DATETIME NULL DEFAULT NULL COMMENT 'Время выдачи постоматом' AFTER `loc_cell`; 

-- Комментарий к штрихкоду
ALTER TABLE `barcodes` 
    add `comment` text null;
