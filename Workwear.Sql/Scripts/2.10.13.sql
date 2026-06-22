-- Добавление причины выдачи в документ расхода
ALTER TABLE stock_expense
	ADD COLUMN cause_issue_id INT UNSIGNED NULL AFTER warehouse_id,
	ADD INDEX fk_stock_expense_cause_issue_idx (cause_issue_id ASC),
	ADD CONSTRAINT fk_stock_expense_cause_issue
		FOREIGN KEY (cause_issue_id) REFERENCES causes_issue (id)
			ON DELETE SET NULL ON UPDATE CASCADE;ALTER TABLE stock_expense
	ADD cause_issue_id INT UNSIGNED NULL
		AFTER employee_id;

ALTER TABLE stock_expense
	ADD CONSTRAINT stock_expense_causes_issue_id_fk
		FOREIGN KEY (cause_issue_id)
			REFERENCES causes_issue (id)
			ON UPDATE CASCADE
			ON DELETE SET NULL;
