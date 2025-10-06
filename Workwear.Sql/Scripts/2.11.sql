-- Удаление функции, которая считает количество, необходимое к выдаче
DROP FUNCTION count_issue;

-- Удаление таблицы work_days, которая больше не используется
DROP TABLE work_days;

-- Заполнение даты начала использования
UPDATE operation_issued_by_employee op1 SET StartOfUse = 
	(SELECT DATE(operation_time) FROM operation_issued_by_employee op2
	        WHERE op1.id = op2.id)
WHERE StartOfUse IS NULL;

-- Добавление ограничения на дату начала использования
ALTER TABLE operation_issued_by_employee MODIFY StartOfUse DATE NOT NULL;
