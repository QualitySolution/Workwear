-- Удаляем пустые строки из размеров и роста в операциях. Это приводит к неоднозначности в запросах
UPDATE `operation_warehouse` SET `size`= NULL WHERE size = "";
UPDATE `operation_warehouse` SET `growth`= NULL WHERE growth = "";
UPDATE `operation_issued_by_employee` SET `size`= NULL WHERE size = "";
UPDATE `operation_issued_by_employee` SET `growth`= NULL WHERE growth = "";

-- У таблицы operation_issued_by_employee поменялась немного идеология теперь protection_tools_id является главным значением для выборки вместо номеклатуры
-- Поэтому копируем для возвратов и списаний protection_tools_id из операций выдачи
UPDATE `operation_issued_by_employee` operation
    LEFT JOIN operation_issued_by_employee issued ON issued.id = operation.issued_operation_id
    SET operation.protection_tools_id = issued.protection_tools_id
WHERE issued.id IS NOT NULL AND operation.protection_tools_id IS NULL;