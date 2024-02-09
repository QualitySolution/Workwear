-- В документ постомата добавлена ссылка на сотрудника
alter table postomat_document_items
	add employee_id int UNSIGNED NOT NULL after last_update;

alter table postomat_document_items
	add constraint fk_postomat_document_items_employee_id
		foreign key (employee_id) references wear_cards (id);

-- В заявку на стрику добавлена ссылка на сотрудника.
alter table clothing_service_claim
	add employee_id int UNSIGNED NOT NULL after barcode_id;
	
alter table clothing_service_claim
	add constraint fk_clothing_service_claim_employee_id
		foreign key (employee_id) references wear_cards (id);

-- Добавлено время изменения в операции
ALTER TABLE `operation_issued_by_employee` ADD `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `operation_time`;
ALTER TABLE `operation_issued_by_employee` ADD INDEX `operation_issued_by_employee_last_update_idx` (`last_update` DESC);
