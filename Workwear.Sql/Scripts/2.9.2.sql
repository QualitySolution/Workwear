--
-- Добавляем внешние ключи для документа выдачи по дежурной норме в ведомость
--

ALTER TABLE issuance_sheet
	ADD stock_expense_duty_norm_id int unsigned null after stock_expense_id;;
ALTER TABLE issuance_sheet
	ADD CONSTRAINT fk_stock_expense_duty_norm_id
		FOREIGN KEY (stock_expense_duty_norm_id) REFERENCES stock_expense_duty_norm (id)
			ON UPDATE CASCADE ON DELETE NO ACTION;

ALTER TABLE issuance_sheet_items
	ADD stock_expense_duty_norm_item_id integer unsigned null after stock_expense_detail_id;
ALTER TABLE issuance_sheet_items
	ADD CONSTRAINT fk_stock_expense_duty_norm_item_id
		FOREIGN KEY (stock_expense_duty_norm_item_id) REFERENCES stock_expense_duty_norm_items (id)
			ON UPDATE CASCADE ON DELETE CASCADE ;
