-- Добавление операции выдачи по дежурной норме в ведомость
ALTER TABLE issuance_sheet_items 
	ADD COLUMN duty_norm_issue_operation_id int(10) unsigned NULL DEFAULT NULL AFTER issued_operation_id;

ALTER TABLE issuance_sheet_items
	ADD CONSTRAINT fk_issuance_sheet_items_duty_norm_issue_operation_id
		FOREIGN KEY (duty_norm_issue_operation_id) REFERENCES operation_issued_by_duty_norm(id)
			ON UPDATE CASCADE 
			ON DELETE NO ACTION;

CREATE INDEX fk_issuance_sheet_items_duty_norm_issue_operation_idx 
	ON issuance_sheet_items(duty_norm_issue_operation_id ASC);

-- Добавление операций выдачи по дежурной норме в операции со штрихкодами
ALTER TABLE operation_barcodes
	ADD COLUMN duty_norm_issue_operation_id int(10) unsigned NULL DEFAULT NULL AFTER employee_issue_operation_id;

ALTER TABLE operation_barcodes
	ADD CONSTRAINT fk_operation_barcodes_duty_norm_issue_operation_id
		FOREIGN KEY (duty_norm_issue_operation_id) REFERENCES operation_issued_by_duty_norm(id)
			ON UPDATE CASCADE
			ON DELETE NO ACTION;

CREATE INDEX fk_operation_barcodes_duty_norm_issue_operation_id_idx
	ON operation_barcodes(duty_norm_issue_operation_id ASC);



