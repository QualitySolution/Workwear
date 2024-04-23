create table if not exists `protection_tools_category_for_analytics`
(
	`id`      int(11) unsigned not null auto_increment primary key,
	`name`    varchar(100)     not null,
	`comment` text             null default null
);

alter table `protection_tools`
	add column
		`category_for_analytics_id` int unsigned null default null,
	add constraint `FK_protection_tools_category_for_analytics`
		foreign key (`category_for_analytics_id`)
			references `protection_tools_category_for_analytics` (`id`)
			on delete set null
			on update cascade;

ALTER TABLE `operation_barcodes`
	ADD COLUMN `warehouse_id` INT UNSIGNED NULL,
	ADD CONSTRAINT `FK_operation_barcodes_warehouse` FOREIGN KEY (`warehouse_id`) REFERENCES `warehouse` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE `barcodes`
	ADD COLUMN `label`	varchar(50)  null default null; 

create table if not exists `operation_substitute_fund`
(
	`id`                              int unsigned                          not null auto_increment primary key,
	`operation_time`                  datetime                              not null,
	`last_update`                     timestamp default current_timestamp() not null on update current_timestamp(),
	`operation_issued_by_employee_id` int unsigned                          not null,
	`substitute_barcode_id`       	  int unsigned                          not null,
	`warehouse_operation_id`          int unsigned                          not null,
	`operation_write_off_id`          int unsigned                          null,
	constraint `FK_op_substitute_employee_issued`
		foreign key (`operation_issued_by_employee_id`) references `operation_issued_by_employee` (`id`)
			on update cascade on delete cascade,
	constraint `FK_op_substitute_barcode`
		foreign key (`substitute_barcode_id`) references `barcodes` (`id`)
			on update cascade on delete cascade,
	constraint `FK_op_substitute_write_off`
		foreign key (`operation_write_off_id`) references `operation_substitute_fund` (`id`)
			on update cascade on delete cascade,
	constraint `FK_op_substitute_op_warehouse`
		foreign key (`warehouse_operation_id`) references `operation_warehouse` (`id`)
			on update cascade on delete cascade
);

alter table `operation_barcodes`
	add column
		`operation_substitute_id` int unsigned null,
	add constraint `FK_op_barocodes_op_substitute`
		foreign key (`operation_substitute_id`) references `operation_substitute_fund` (`id`)
			on update cascade
			on delete cascade;

create table if not exists `substitute_fund_documents`
(
	`id`            int unsigned                          not null auto_increment primary key,
	`date`          date                                  not null,
	`creation_date` timestamp default current_timestamp() not null on update current_timestamp(),
	`user_id`       int unsigned                          null,
	`warehouse_id`  int unsigned                          not null,
	`comment`       text                                  null,
	constraint `FK_substitute_fund_docs_user`
		foreign key (`user_id`) references `users` (`id`)
			on update cascade on delete set null,
	constraint `FK_substitute_fund_docs_warehouse`
		foreign key (`warehouse_id`) references `warehouse` (`id`)
			on update cascade
) ENGINE = InnoDB
  AUTO_INCREMENT = 1
  DEFAULT CHARACTER SET = utf8mb4
  COLLATE = utf8mb4_general_ci;

create table if not exists `substitute_fund_document_items`
(
	`id`                     int unsigned not null auto_increment primary key,
	`document_id`            int unsigned not null,
	`operation_subsitute_id` int unsigned not null,
	constraint `FK_substitute_fund_document`
		foreign key (`document_id`) references `substitute_fund_documents` (`id`)
			on update cascade on delete cascade,
	constraint `FK_susbtitute_fund_operation`
		foreign key (`operation_subsitute_id`) references `operation_substitute_fund` (`id`)
			on update cascade on delete cascade
);
