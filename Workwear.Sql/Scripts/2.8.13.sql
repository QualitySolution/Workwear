-- Доработка постоматов
ALTER TABLE `postomat_documents` ADD `confirm_time` DATETIME NULL DEFAULT NULL COMMENT 'Время выполнения(done) документа на постомате.' AFTER `create_time`;
ALTER TABLE `postomat_documents` ADD `confirm_user` INT UNSIGNED NULL DEFAULT NULL COMMENT 'Пользователь системы постоматов проводивший документ.' AFTER `confirm_time`;
ALTER TABLE `postomat_document_items` ADD `dispense_time` DATETIME NULL DEFAULT NULL COMMENT 'Время выдачи постоматом' AFTER `loc_cell`;
-- Версионирование для документа постомата
ALTER TABLE `postomat_document_items` ADD `last_update` TIMESTAMP ON UPDATE CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `document_id`;
ALTER TABLE `postomat_document_items` ADD INDEX(`last_update`);


-- Комментарий к штрихкоду
ALTER TABLE `barcodes` 
    ADD `comment` TEXT NULL;

-- Исправление с удалением ручных операций со штрихкодами
ALTER TABLE operation_barcodes
	DROP FOREIGN KEY fk_operation_barcodes_2;

ALTER TABLE operation_barcodes
	ADD CONSTRAINT fk_operation_barcodes_2
		FOREIGN KEY (employee_issue_operation_id) REFERENCES operation_issued_by_employee (id)
			ON UPDATE CASCADE ON DELETE CASCADE;
