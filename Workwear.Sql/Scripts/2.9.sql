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

-- операции сверх нормы
ALTER TABLE `operation_barcodes`
	ADD COLUMN `warehouse_id` INT UNSIGNED NULL,
	ADD CONSTRAINT `FK_operation_barcodes_warehouse` FOREIGN KEY (`warehouse_id`) REFERENCES `warehouse` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `barcodes`
	ADD COLUMN `label` varchar(50) null default null;

create table if not exists `operation_over_norm`
(
	`id`                              int unsigned                           not null auto_increment primary key,
	`operation_time`                  datetime                               not null,
	`last_update`                     timestamp                              not null default current_timestamp() not null on update current_timestamp(),
	`type`                            enum ('Repair', 'Substitute', 'Guest') not null,
	`employee_id` 					  int unsigned 							 not null,
	`operation_warehouse_id`          int unsigned                           not null,
	`operation_issued_by_employee_id` int unsigned                           null     default null,
	`operation_write_off_id`          int unsigned                           null     default null,
	constraint `FK_over_norm_employee_issued`
		foreign key (`operation_issued_by_employee_id`) references `operation_issued_by_employee` (`id`)
			on update cascade on delete no action,
	constraint `FK_over_norm_write_off`
		foreign key (`operation_write_off_id`) references `operation_over_norm` (`id`)
			on update cascade on delete cascade ,
	constraint `FK_over_norm_op_warehouse`
		foreign key (`operation_warehouse_id`) references `operation_warehouse` (`id`)
			on update cascade on delete cascade ,
	constraint `FK_over_norm_employee`
		foreign key (`employee_id`) references `wear_cards` (`id`)
			on update cascade on delete cascade
);

alter table operation_barcodes
	add column `over_norm_id` int unsigned null,
	add constraint `FK_op_barcodes_op_over_norm`
		foreign key (`over_norm_id`) references `operation_over_norm` (`id`)
			on update cascade on delete cascade;

create table if not exists `over_norm_documents`
(
	`id`            int unsigned                           not null auto_increment primary key,
	`doc_number`    varchar(16)                            null,
	`date`          date                                   not null,
	`creation_date` timestamp                                   default current_timestamp() not null on update current_timestamp(),
	`type`          enum ('Repair', 'Substitute', 'Guest') not null,
	`user_id`       int unsigned                           null default null,
	`warehouse_id`  int unsigned                           not null,
	`comment`       text                                   null,
	constraint `FK_over_norm_docs_user`
		foreign key (`user_id`) references `users` (`id`)
			on update cascade on delete set null,
	constraint `FK_over_norm_docs_warehouse`
		foreign key (`warehouse_id`) references `warehouse` (`id`)
			on update cascade on delete no action
);

create table if not exists `over_norm_document_items`
(
	`id`           int unsigned not null auto_increment primary key,
	`document_id`  int unsigned not null,
	`over_norm_id` int unsigned not null,
	constraint `FK_over_norm_document`
		foreign key (`document_id`) references `over_norm_documents` (`id`)
			on update cascade on delete cascade ,
	constraint `FK_op_over_norm`
		foreign key (`over_norm_id`) references `operation_over_norm` (`id`)
			on update cascade on delete cascade
);
