-- операции выдачи вне нормы
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
	`type`                            enum ('Simple', 'Substitute', 'Guest') not null,
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
	foreign key (`employee_id`) references `employees` (`id`)
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
	`type`          enum ('Simple', 'Substitute', 'Guest') not null,
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
