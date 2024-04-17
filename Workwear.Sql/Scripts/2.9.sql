-- Удаляем механизм выдачи со списанием
ALTER TABLE stock_expense DROP FOREIGN KEY fk_stock_expense_2;
ALTER TABLE `stock_expense` DROP `write_off_doc`;

ALTER TABLE operation_issued_by_employee DROP FOREIGN KEY fk_operation_issued_by_employee_6;
ALTER TABLE `operation_issued_by_employee` DROP `operation_write_off_id`;

ALTER TABLE `stock_write_off_detail` DROP `akt_number`;

-- Удаляем номер бухгалтерского документа
ALTER TABLE `operation_issued_by_employee` DROP `buh_document`;

-- Удаляем аналоги из номенклатуры нормы
DROP TABLE `protection_tools_replacement`;

-- Удаляем выдачу на подразделения
-- stock_income
DELETE FROM `stock_income` WHERE `operation` = 'Object';

alter table stock_income
	modify operation enum ('Enter', 'Return') not null;

alter table stock_income
drop foreign key fk_stock_income_object;

alter table stock_income
drop column object_id;

alter table stock_income_detail
drop foreign key fk_stock_income_detail_3;

alter table stock_income_detail
drop column subdivision_issue_operation_id;

-- stock_expense
DELETE FROM `stock_expense` WHERE `operation` = 'Object';

alter table stock_expense
drop foreign key fk_stock_expense_object_id;

alter table stock_expense
drop column operation,
drop column object_id;
     
alter table stock_expense_detail
drop foreign key fk_stock_expense_detail_3,
drop foreign key fk_stock_expense_detail_placement;

alter table stock_expense_detail
drop column object_place_id,
drop column subdivision_issue_operation_id;
     
-- stock_write_off
DELETE FROM `stock_write_off_detail` WHERE subdivision_issue_operation_id IS NOT NULL;    
     
alter table stock_write_off_detail
drop foreign key fk_stock_write_off_detail_4;

alter table stock_write_off_detail
drop column subdivision_issue_operation_id;
     
-- operation_issued_in_subdivision
DROP TABLE `operation_issued_in_subdivision`;

-- object_places
drop table object_places;

-- удаляем тип номенклатур имущества

DELETE FROM protection_tools
WHERE (SELECT item_types.category FROM item_types WHERE item_types.id = protection_tools.item_types_id) = 'property';

DELETE FROM nomenclature
WHERE (SELECT item_types.category FROM item_types WHERE item_types.id = nomenclature.type_id) = 'property';

DELETE FROM `item_types` WHERE category = 'property';

alter table item_types 
	drop column category,
	drop column norm_life;
