--
-- Добавляем Дежурные нормы
--

create table duty_norms
(
	id                   int unsigned auto_increment
        primary key,
	name                 varchar(200) charset utf8mb4 null,
	responsible_leder_id int unsigned 				  null,
	responsible_employee_id int unsigned 			  null,
	subdivision_id 		 int unsigned 				  null, 
	datefrom             datetime                     null,
	dateto               datetime                     null,
	comment              text charset utf8mb4         null,
	constraint duty_norms_employees_id_fk
		foreign key (responsible_employee_id) references employees (id)
			on delete set null on update cascade,
	constraint duty_norms_leaders_surname_fk
		foreign key (responsible_leder_id) references leaders (id)
			on delete set null on update cascade,
	constraint duty_norms_subdivisions_id_fk
		foreign key (subdivision_id) references subdivisions (id)
			on delete set null on update cascade
);

create table duty_norm_items
(
	id                  int unsigned auto_increment
        primary key,
	duty_norm_id        int unsigned                                              not null,
	protection_tools_id int unsigned                                              not null,
	amount              int unsigned                               default 1      not null,
	period_type         enum ('Year', 'Month', 'Wearout') default 'Year' not null,
	period_count        tinyint unsigned                           default 1      not null,
	next_issue          date                                                      null,
	norm_paragraph      varchar(200)                                              null comment 'Пункт норм, основание выдачи',
	comment             text                                                      null,
	constraint fk_duty_norms_item_norm
		foreign key (duty_norm_id) references duty_norms (id)
			on update cascade,
	constraint fk_duty_norms_item_protection_tools
		foreign key (protection_tools_id) references protection_tools (id)
			on update cascade
);

create table operation_issued_by_duty_norm
(
	id                     int unsigned auto_increment
        primary key,
	operation_time         datetime                                           not null,
	last_update            timestamp              default current_timestamp() not null on update current_timestamp(),
	nomenclature_id        int unsigned                                       null,
	duty_norm_item_id      int unsigned                                       null,
	duty_norm_id           int unsigned                                       not null,
	size_id                int unsigned                                       null,
	height_id              int unsigned                                       null,
	wear_percent           decimal(3, 2) unsigned default 1.00                not null,
	issued                 int                    default 0                   not null,
	returned               int                    default 0                   not null,
	auto_writeoff          tinyint(1)             default 1                   not null,
	auto_writeoff_date     date                                               null,
	protection_tools_id    int unsigned                                       null,
	start_of_use           date                                               null,
	expiry_by_norm         date                                               null,
	issued_operation_id    int unsigned                                       null,
	warehouse_operation_id int unsigned                                       null,
	override_before        tinyint(1)             default 0                   not null,
	comment                text                                               null,
	constraint fk_operation_issued_by_duty_norm_height
		foreign key (height_id) references sizes (id)
			on update cascade,
	constraint fk_operation_issued_by_duty_norm_issued_operation
		foreign key (issued_operation_id) references operation_issued_by_duty_norm (id)
			on update cascade on delete cascade,
	constraint fk_operation_issued_by_duty_norm_nomenclature
		foreign key (nomenclature_id) references nomenclature (id)
			on update cascade,
	constraint fk_operation_issued_by_duty_norm_norm
		foreign key (duty_norm_id) references duty_norms (id)
			on update cascade on delete cascade,
	constraint fk_operation_issued_by_duty_norm_operation_warehouse
		foreign key (warehouse_operation_id) references operation_warehouse (id)
			on update cascade,
	constraint fk_operation_issued_by_duty_norm_protection_tools
		foreign key (protection_tools_id) references protection_tools (id)
			on update cascade on delete set null,
	constraint fk_operation_issued_by_duty_norm_size
		foreign key (size_id) references sizes (id)
			on update cascade,
	constraint fk_operation_issued_by_employee_duty_norm_item
		foreign key (duty_norm_item_id) references duty_norm_items (id)
			on update cascade on delete set null
);

create index operation_issued_by_duty_norm_last_update_idx
	on operation_issued_by_duty_norm (last_update);

create index operation_issued_by_duty_norm_operation_time_idx
	on operation_issued_by_duty_norm (operation_time);

create index operation_issued_by_duty_norm_wear_percent_idx
	on operation_issued_by_duty_norm (wear_percent);

create table stock_expense_duty_norm
(
	id                      int unsigned auto_increment
        primary key,
	doc_number              varchar(16)  null,
	creation_date           datetime     not null  default (CURRENT_DATE()),
	date                    date         not null,
	duty_norm_id            int unsigned null,
	warehouse_id            int unsigned not null,
	responsible_employee_id int unsigned null,
	user_id                 int unsigned null,
	comment                 text         null,
	constraint fk_stock_expense_duty_norm_norm
		foreign key (duty_norm_id) references duty_norms (id)
			on update cascade,
	constraint fk_stock_expense_duty_norm_responsible_employee
		foreign key (responsible_employee_id) references employees (id)
			on update cascade,
	constraint fk_stock_expense_duty_norm_user
		foreign key (user_id) references users (id)
			on update cascade on delete set null,
	constraint fk_stock_expense_duty_norm_warehouse
		foreign key (warehouse_id) references warehouse (id)
			on update cascade
)
	charset = utf8mb4;

create index fk_stock_expense_duty_norm_employee_idx
	on stock_expense_duty_norm (responsible_employee_id);

create index fk_stock_expense_duty_norm_user_idx
	on stock_expense_duty_norm (user_id);

create index fk_stock_expense_duty_norm_warehouse_idx
	on stock_expense_duty_norm (warehouse_id);

create index stock_expense_duty_norm_expense_date_idx
	on stock_expense_duty_norm (date);

create table stock_expense_duty_norm_items
(
	id                               int unsigned auto_increment
		primary key,
	stock_expense_duty_norm_id       int unsigned not null,
	operation_issued_by_duty_norm_id int unsigned null,
	warehouse_operation_id           int unsigned not null,
	constraint fk_stock_expense_duty_norm_items_operation_issued_by_duty_norm
		foreign key (operation_issued_by_duty_norm_id) references operation_issued_by_duty_norm (id)
			on update cascade,
	constraint fk_stock_expense_duty_norm_items_operation_warehouse
		foreign key (warehouse_operation_id) references operation_warehouse (id),
	constraint fk_stock_expense_duty_norm_items_stock_expense_duty_norm
		foreign key (stock_expense_duty_norm_id) references stock_expense_duty_norm (id)
			on update cascade on delete cascade
);

create index fk_stock_expense_duty_norm_items_operation_idx
	on stock_expense_duty_norm_items (operation_issued_by_duty_norm_id);
create index fk_stock_expense_duty_norm_items_warehouse_operation_idx
	on stock_expense_duty_norm_items (warehouse_operation_id);
create index fk_stock_expense_duty_norm_items_stock_expense_duty_norm_idx
	on stock_expense_duty_norm_items (stock_expense_duty_norm_id);
