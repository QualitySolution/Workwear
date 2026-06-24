-- Заменяем тип операции 'Object' (возврат из подразделения) на 'Enter',
-- дописывая в комментарий название подразделения из таблицы objects (поле object_id).
UPDATE stock_income si
    LEFT JOIN objects o ON o.id = si.object_id
SET si.operation = 'Enter',
    si.comment   = CONCAT_WS('. ', NULLIF(TRIM(si.comment), ''),
                       CONCAT('Возврат из подразделения: ',
                              COALESCE(o.name, CONCAT('ID=', si.object_id))))
WHERE si.operation = 'Object';

-- Удаляем значение 'Object' из enum поля operation в таблице stock_income,
-- чтобы версии приложения до 2.8.18 (в которой был удалён код работы
-- с подразделениями) не могли создавать новые возвраты из подразделений
-- и для обеспечения консистентности базы данных.
ALTER TABLE stock_income
    MODIFY COLUMN `operation` ENUM('Enter', 'Return') NOT NULL;

DELETE FROM `stock_expense` WHERE `operation` = 'Object';

ALTER TABLE stock_expense
	MODIFY COLUMN `operation` ENUM('Employee') NOT NULL DEFAULT 'Employee';
