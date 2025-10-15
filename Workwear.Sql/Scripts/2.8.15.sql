-- В документ постомата добавлена ссылка на сотрудника
ALTER TABLE postomat_document_items
	ADD employee_id INT UNSIGNED NOT NULL AFTER last_update;

ALTER TABLE postomat_document_items
	ADD CONSTRAINT fk_postomat_document_items_employee_id
		FOREIGN KEY (employee_id) REFERENCES wear_cards (id);

-- В заявку на стрику добавлена ссылка на сотрудника.
ALTER TABLE clothing_service_claim
	ADD employee_id INT UNSIGNED NOT NULL AFTER barcode_id;
	
ALTER TABLE clothing_service_claim
	ADD CONSTRAINT fk_clothing_service_claim_employee_id
		FOREIGN KEY (employee_id) REFERENCES wear_cards (id);

-- Добавлено время изменения в операции
ALTER TABLE `operation_issued_by_employee` ADD `last_update` TIMESTAMP ON UPDATE CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `operation_time`;
ALTER TABLE `operation_issued_by_employee` ADD INDEX `operation_issued_by_employee_last_update_idx` (`last_update` DESC);
