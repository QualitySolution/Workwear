create table if not exists `protection_tools_category_for_analytics`
(
	`id`   int(11) unsigned not null auto_increment primary key,
	`name` varchar(100)     not null,
	`comment` text null default null
);

alter table `protection_tools`
	add column
		`category_for_analytic_id` int unsigned null default null,
	add constraint `FK_protection_tools_category_for_analytics`
		foreign key (`category_for_analytic_id`)
			references `protection_tools_category_for_analytics` (`id`)
			on delete set null
			on update cascade;

alter table stock_collective_expense
	add doc_number varchar(16) null after id;

alter table stock_expense
	add doc_number varchar(16) null after operation;

alter table stock_income
	add doc_number varchar(16) null after operation;

alter table issuance_sheet
	add doc_number varchar(16) null after id;

alter table stock_write_off
	add doc_number varchar(16) null after id;

alter table stock_inspection
	add doc_number varchar(16) null after id;
