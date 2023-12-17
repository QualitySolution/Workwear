-- Доработка постоматов
ALTER TABLE `postomat_documents` ADD `confirm_time` DATETIME NULL DEFAULT NULL COMMENT 'Время выполнения(done) документа на постомате.' AFTER `create_time`;
ALTER TABLE `postomat_documents` ADD `confirm_user` INT UNSIGNED NULL DEFAULT NULL COMMENT 'Пользователь системы постоматов проводивший документ.' AFTER `confirm_time`;
ALTER TABLE `postomat_document_items` ADD `dispense_time` DATETIME NULL DEFAULT NULL COMMENT 'Время выдачи постоматом' AFTER `loc_cell`;
-- Версионирование для документа постомата
ALTER TABLE `postomat_document_items` ADD `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `document_id`;
ALTER TABLE `postomat_document_items` ADD INDEX(`last_update`);


-- Комментарий к штрихкоду
ALTER TABLE `barcodes` 
    add `comment` text null;

-- Исправление с удалением ручных операций со штрихкодами
alter table operation_barcodes
	drop foreign key fk_operation_barcodes_2;

alter table operation_barcodes
	add constraint fk_operation_barcodes_2
		foreign key (employee_issue_operation_id) references operation_issued_by_employee (id)
			on update cascade on delete cascade;
