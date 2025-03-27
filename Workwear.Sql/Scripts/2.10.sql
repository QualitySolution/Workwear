-- Добавление id сотрудника в строки документа возврата

ALTER TABLE stock_return_items
	ADD COLUMN employee_id INT(10)UNSIGNED NULL AFTER quantity;
ALTER TABLE stock_return_items
	ADD CONSTRAINT stock_return_items_employee_id_fk FOREIGN KEY (employee_id) REFERENCES employees(id)
		ON UPDATE CASCADE
		ON DELETE RESTRICT;
CREATE INDEX stock_return_items_employee_id_index
	ON stock_return_items(employee_id ASC);

-- Добавление id дежурной нормы в строки документа возврата

ALTER TABLE stock_return_items
	ADD COLUMN duty_norm_id INT(10)UNSIGNED NULL AFTER warehouse_operation_id;
ALTER TABLE stock_return_items
	ADD CONSTRAINT stock_return_items_duty_norm_id_fk FOREIGN KEY (duty_norm_id) REFERENCES duty_norms (id)
		ON UPDATE CASCADE
		ON DELETE RESTRICT;
CREATE INDEX stock_return_items_duty_norm_id_index
	ON stock_return_items(duty_norm_id ASC);

-- Добавление id операции выдачи по деж. норме в строки документа возврата

ALTER TABLE stock_return_items
	ADD COLUMN duty_norm_issue_operation_id INT(10)UNSIGNED NULL AFTER duty_norm_id;
ALTER TABLE stock_return_items
	ADD CONSTRAINT stock_return_items_duty_norm_issue_operation_id_fk FOREIGN KEY (duty_norm_issue_operation_id) REFERENCES operation_issued_by_duty_norm (id)
		ON UPDATE NO ACTION
		ON DELETE NO ACTION;
CREATE INDEX stock_return_items_duty_norm_issue_operation_id_index
	ON stock_return_items(duty_norm_issue_operation_id ASC);

-- Изменение ограничения на null в операции выдачи сотруднику
-- Создание индекса на операцию выдачи сотруднику

ALTER TABLE stock_return_items
	MODIFY employee_issue_operation_id INT(10) UNSIGNED NULL;
CREATE INDEX stock_return_items_employee_issue_operation_id_index
	ON stock_return_items(employee_issue_operation_id ASC);

-- Перенос id сотрудников в строки документа возврата

UPDATE stock_return_items
	LEFT JOIN stock_return ON stock_return_items.stock_return_id = stock_return.id
	SET stock_return_items.employee_id = stock_return.employee_id;

-- Удаление id сотрудников из документа возврата
ALTER TABLE stock_return
DROP CONSTRAINT stock_return_employees_id_fk;
ALTER TABLE stock_return
DROP COLUMN employee_id;
