-- Добавление причины выдачи в документ расхода
ALTER TABLE stock_expense
	ADD COLUMN cause_issue_id INT UNSIGNED NULL AFTER warehouse_id,
	ADD INDEX fk_stock_expense_cause_issue_idx (cause_issue_id ASC),
	ADD CONSTRAINT fk_stock_expense_cause_issue
		FOREIGN KEY (cause_issue_id) REFERENCES causes_issue (id)
			ON DELETE SET NULL ON UPDATE CASCADE;
