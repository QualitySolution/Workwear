-- Удаляем механизм выдачи со списанием
ALTER TABLE stock_expense DROP FOREIGN KEY fk_stock_expense_2;
ALTER TABLE `stock_expense` DROP `write_off_doc`;

ALTER TABLE operation_issued_by_employee DROP FOREIGN KEY fk_operation_issued_by_employee_6;
ALTER TABLE `operation_issued_by_employee` DROP `operation_write_off_id`;

ALTER TABLE `stock_write_off_detail` DROP `akt_number`;

-- Удаляем номер бухгалтерского документа
ALTER TABLE `operation_issued_by_employee` DROP `buh_document`;
