
#Миграция данных

ALTER TABLE operation_issued_by_employee ADD COLUMN temp_id INT(10) UNSIGNED;

#Выдача сотрудникам
INSERT INTO `operation_issued_by_employee`(`employee_id`, `operation_time`, `nomenclature_id`, `wear_percent`, `issued`, `returned`, auto_writeoff,
`auto_writeoff_date`, `issued_operation_id`, `stock_income_detail_id`, `buh_document`, `temp_id`, `StartOfUse`, `ExpiryByNorm`) 
SELECT stock_expense.wear_card_id, stock_expense.date, stock_expense_detail.nomenclature_id, 
1.0 - stock_income_detail.life_percent, stock_expense_detail.quantity, 0, stock_expense_detail.auto_writeoff_date IS NOT NULL, stock_expense_detail.auto_writeoff_date, 
NULL, stock_expense_detail.stock_income_detail_id, stock_expense.comment, stock_expense_detail.id, stock_expense.date, stock_expense_detail.auto_writeoff_date
FROM stock_expense_detail
LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id
LEFT JOIN stock_income_detail ON stock_expense_detail.stock_income_detail_id = stock_income_detail.id
WHERE stock_expense.operation = 'Employee';

UPDATE stock_expense_detail, operation_issued_by_employee 
SET stock_expense_detail.employee_issue_operation_id = operation_issued_by_employee.id
WHERE stock_expense_detail.id = operation_issued_by_employee.temp_id;

UPDATE operation_issued_by_employee SET operation_issued_by_employee.temp_id = NULL;

#Возвращение на склад
INSERT INTO `operation_issued_by_employee`(`employee_id`, `operation_time`, `nomenclature_id`, `wear_percent`, `issued`, `returned`, `auto_writeoff_date`, `issued_operation_id`, `stock_income_detail_id`, `buh_document`, `temp_id`) 
SELECT stock_income.wear_card_id, stock_income.date, stock_income_detail.nomenclature_id, 
1.0 - stock_income_detail.life_percent, 0, stock_income_detail.quantity, NULL, stock_expense_detail.employee_issue_operation_id, stock_expense_detail.stock_income_detail_id, stock_income.comment, stock_income_detail.id
FROM stock_income_detail
LEFT JOIN stock_income ON stock_income.id = stock_income_detail.stock_income_id
LEFT JOIN stock_expense_detail ON stock_expense_detail.id = stock_income_detail.stock_expense_detail_id
WHERE stock_income.operation = 'Return';

UPDATE stock_income_detail, operation_issued_by_employee 
SET stock_income_detail.employee_issue_operation_id = operation_issued_by_employee.id
WHERE stock_income_detail.id = operation_issued_by_employee.temp_id;

UPDATE operation_issued_by_employee SET operation_issued_by_employee.temp_id = NULL;

#Списание с сотрудника
INSERT INTO `operation_issued_by_employee`(`employee_id`, `operation_time`, `nomenclature_id`, `wear_percent`, `issued`, `returned`, `auto_writeoff_date`, `issued_operation_id`, `stock_income_detail_id`, `buh_document`, `temp_id`) 
SELECT stock_expense.wear_card_id, stock_write_off.date, stock_write_off_detail.nomenclature_id, 
IF(stock_write_off.date >= stock_expense_detail.auto_writeoff_date, 1,
                IFNULL(
                    DATEDIFF(stock_write_off.date, stock_expense.date) /
                    DATEDIFF(stock_expense_detail.auto_writeoff_date, stock_expense.date), 1))
     , 0, stock_write_off_detail.quantity, NULL, stock_expense_detail.employee_issue_operation_id, stock_expense_detail.stock_income_detail_id, stock_write_off.comment, stock_write_off_detail.id
FROM stock_write_off_detail
JOIN stock_write_off ON stock_write_off.id = stock_write_off_detail.stock_write_off_id
JOIN stock_expense_detail ON stock_expense_detail.id = stock_write_off_detail.stock_expense_detail_id
JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id
LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id
WHERE stock_expense.operation = 'Employee';

UPDATE stock_write_off_detail, operation_issued_by_employee 
SET stock_write_off_detail.employee_issue_operation_id = operation_issued_by_employee.id
WHERE stock_write_off_detail.id = operation_issued_by_employee.temp_id;

ALTER TABLE operation_issued_by_employee DROP COLUMN operation_issued_by_employee.temp_id;

INSERT INTO `base_parameters` (`name`, `str_value`) VALUES ('DefaultAutoWriteoff', 'True');

-- Обновляем версию базы.

DELETE FROM base_parameters WHERE name = 'micro_updates';
UPDATE base_parameters SET str_value = '2.1' WHERE name = 'version';