-- Доработка постоматов
ALTER TABLE `postomat_documents` ADD `confirm_time` DATETIME NULL DEFAULT NULL COMMENT 'Время выполнения(done) документа на постомате.' AFTER `create_time`;
ALTER TABLE `postomat_documents` ADD `confirm_user` INT UNSIGNED NULL DEFAULT NULL COMMENT 'Пользователь системы постоматов проводивший документ.' AFTER `confirm_time`;

-- Комментарий к штрихкоду
ALTER TABLE `barcodes` 
    add `comment` text null;

-- Удаление связи со штрихкодом при удалении операции
alter table operation_barcodes
	drop foreign key fk_operation_barcodes_2;

alter table operation_barcodes
	add constraint fk_operation_barcodes_2
		foreign key (employee_issue_operation_id) references operation_issued_by_employee (id)
			on update cascade on delete cascade;
