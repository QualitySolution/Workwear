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


-- переименование таблиц: objects в subdivisions, wear_cards в employees и таблички связи с wear_cards , 
-- norms_professions в norms_posts, переименование ключей, индексов
-- вставка данных из старых таблиц в новые
	    
alter table clothing_service_claim
drop foreign key fk_clothing_service_claim_employee_id;

alter table departments
drop foreign key fk_departaments_1;

alter table employee_group_items
	add constraint employee_groups_items_unique
		unique (employee_id, employee_group_id);

alter table employee_group_items
drop key wear_card_groups_items_unique;

alter table employee_group_items
drop foreign key foreign_key_employee_groups_items_employees; 

alter table issuance_sheet
drop foreign key fk_issuance_sheet_2;

alter table issuance_sheet
drop foreign key fk_issuance_sheet_8;

alter table issuance_sheet_items
drop foreign key fk_issuance_sheet_items_2;

alter table leaders
drop foreign key fk_leaders_1;

create table norms_posts
(
	id      int unsigned auto_increment
		primary key,
	norm_id int unsigned not null,
	post_id int unsigned not null,
	constraint fk_norms_posts_1
		foreign key (norm_id) references norms (id)
			on update cascade on delete cascade,
	constraint fk_norms_posts_2
		foreign key (post_id) references posts (id)
			on delete restrict
			on update cascade
);

create index fk_norms_posts_1_idx
	on norms_posts (norm_id);

create index fk_norms_posts_2_idx
	on norms_posts (post_id);

insert into norms_posts
select * from norms_professions;

drop table norms_professions;

alter table operation_issued_by_employee
drop foreign key fk_operation_issued_by_employee_1;

alter table postomat_document_items
drop foreign key fk_postomat_document_items_employee_id;

alter table postomat_document_withdraw_items
drop foreign key fk_postomat_document_withdraw_items_employee_id;

alter table posts
drop foreign key fk_posts_subdivision;

alter table stock_collective_expense
drop foreign key fk_stock_collective_expense_3;

alter table stock_collective_expense_detail
drop foreign key fk_stock_collective_expense_detail_6;

alter table stock_expense
	change column wear_card_id employee_id int unsigned null;

create index fk_stock_expense_employee_idx
	on stock_expense (employee_id);

alter table stock_expense
	drop foreign key fk_stock_expense_wear_card;

drop index fk_stock_expense_wear_card_idx on stock_expense;

alter table stock_income
	change column wear_card_id employee_id int unsigned null;

create index fk_stock_income_employee_idx
	on stock_income (employee_id);

alter table stock_income
	drop foreign key fk_stock_income_wear_card;

drop index fk_stock_income_wear_card_idx on stock_income;

create table subdivisions
(
	id                    int unsigned auto_increment
		primary key,
	code                  varchar(20)  null default null,
	address               text         null default null,
	name                  varchar(240) not null,
	warehouse_id          int unsigned null default null,
	parent_subdivision_id int unsigned null default null,
	constraint fk_subdivisions_1
		foreign key (warehouse_id) references warehouse (id)
			on delete no action
			on update no action ,
	constraint fk_subdivisions_2
		foreign key (parent_subdivision_id) references subdivisions (id)
			on delete set null
			on update no action
)
	collate = utf8mb4_general_ci;

insert into subdivisions
select * from objects;

alter table departments
	add constraint fk_departaments_1
		foreign key (subdivision_id) references subdivisions (id)
			on update cascade on delete set null;

create table employees
(
	id                      int unsigned auto_increment
		primary key,
	last_update             timestamp  default current_timestamp() not null on update current_timestamp(),
	card_number             varchar(15)                            null default null,
	personnel_number        varchar(15)                            null default null,
	last_name               varchar(20)                            null,
	first_name              varchar(20)                            null,
	patronymic_name         varchar(20)                            null,
	card_key                varchar(16)                            null default null,
	subdivision_id          int unsigned                           null default null,
	department_id           int unsigned                           null default null,
	hire_date               date                                   null default null,
	change_of_position_date date                                   null default null,
	dismiss_date            date                                   null default null,
	post_id                 int unsigned                           null default null,
	leader_id               int unsigned                           null default null,
	sex                     enum ('F', 'M')                        null default null,
	birth_date              date                                   null default null,
	user_id                 int unsigned                           null default null,
	phone_number            varchar(16)                            null default null,
	lk_registered           tinyint(1) default 0                   not null default 0,
	email                   text                                   null default null,
	photo                   mediumblob                             null default null,
	comment                 text                                   null default null,
	constraint card_key_UNIQUE
		unique (card_key),
	constraint card_number_UNIQUE
		unique (card_number),
	constraint fk_employees_department
		foreign key (department_id) references departments (id)
			on update cascade on delete set null,
	constraint fk_employees_leader
		foreign key (leader_id) references leaders (id)
			on update cascade on delete set null,
	constraint fk_employees_post
		foreign key (post_id) references posts (id)
			on update cascade on delete set null,
	constraint fk_employees_subdivision
		foreign key (subdivision_id) references subdivisions (id)
			on update cascade on delete set null,
	constraint fk_employees_user
		foreign key (user_id) references users (id)
			on update cascade on delete set null
);

insert into employees
select * from wear_cards;

alter table clothing_service_claim
	add constraint fk_clothing_service_claim_employee_id
		foreign key (employee_id) references employees (id);

create table employee_cards_item
(
	id                    int unsigned auto_increment
		primary key,
	employee_id           int unsigned not null,
	protection_tools_id   int unsigned not null,
	norm_item_id          int unsigned null,
	created               date         null,
	next_issue            date         null,
	next_issue_annotation varchar(240) null default null,
	constraint fk_employee_cards_item_2
		foreign key (protection_tools_id) references protection_tools (id)
			on delete restrict
			on update cascade,
	constraint fk_employee_cards_item_3
		foreign key (norm_item_id) references norms_item (id)
			on delete restrict
			on update cascade,
	constraint fk_employees_item_1
		foreign key (employee_id) references employees (id)
			on delete restrict
			on update cascade
);

create index fk_employee_cards_item_2_idx
	on employee_cards_item (protection_tools_id);

create index fk_employee_cards_item_3_idx
	on employee_cards_item (norm_item_id);

create index fk_employees_item_1_idx
	on employee_cards_item (employee_id);

create index index_employee_cards_item_next_issue
	on employee_cards_item (next_issue);

alter table employee_group_items
	add constraint foreign_key_employee_groups_items_employees
		foreign key (employee_id) references employees (id)
			on update cascade on delete cascade;

create index fk_employees_department_idx
	on employees (department_id);

create index fk_employees_leader_idx
	on employees (leader_id);

create index fk_employees_post_idx
	on employees (post_id);

create index fk_employees_subdivision_idx
	on employees (subdivision_id);

create index fk_employees_user_idx
	on employees (user_id);

create index index_employees_dismiss_date
	on employees (dismiss_date);

create index index_employees_first_name
	on employees (first_name);

create index index_employees_last_name
	on employees (last_name);

create index index_employees_patronymic_name
	on employees (patronymic_name);

create index index_employees_personal_number
	on employees (personnel_number);

create index index_employees_phone_number
	on employees (phone_number);

create index last_update
	on employees (last_update);

create table employees_cost_allocation
(
	id             int unsigned auto_increment
		primary key,
	employee_id    int unsigned                        not null,
	cost_center_id int unsigned                        not null,
	percent        decimal(3, 2) unsigned default 1.00 not null,
	constraint employees_cost_allocation_ibfk_1
		foreign key (cost_center_id) references cost_center (id)
			on delete restrict
			on update cascade,
	constraint employees_cost_allocation_ibfk_2
		foreign key (employee_id) references employees (id)
			on update cascade on delete cascade
);

create table employees_norms
(
	id          int unsigned auto_increment
		primary key,
	employee_id int unsigned not null,
	norm_id     int unsigned not null,
	constraint fk_employees_norms_1
		foreign key (employee_id) references employees (id)
			on update cascade on delete cascade,
	constraint fk_employees_norms_2
		foreign key (norm_id) references norms (id)
			on update cascade on delete cascade
);

create index fk_employees_norms_1_idx
	on employees_norms (employee_id);

create index fk_employees_norms_2_idx
	on employees_norms (norm_id);

create table employees_sizes
(
	id           int unsigned auto_increment
		primary key,
	employee_id  int unsigned not null comment 'Сотрудник для которого установлен размер',
	size_type_id int unsigned not null comment 'Тип размера, не может быть установлено несколько размеров одного типа одному сотруднику',
	size_id      int unsigned not null,
	constraint employees_sizes_unique
		unique using btree(employee_id, size_type_id),
	constraint fk_employees_sizes_1
		foreign key (employee_id) references employees (id)
			on update cascade on delete cascade,
	constraint fk_employees_sizes_2
		foreign key (size_type_id) references size_types (id)
			on update cascade on delete cascade,
	constraint fk_employees_sizes_3
		foreign key (size_id) references sizes (id)
			on update cascade on delete cascade
);

create index fk_employees_sizes_1_idx
	on employees_sizes (employee_id);

create index fk_employees_sizes_2_idx
	on employees_sizes (size_type_id);

create index fk_employees_sizes_3_idx
	on employees_sizes (size_id);

create table employees_vacations
(
	id               int unsigned auto_increment
		primary key,
	employee_id      int unsigned not null,
	vacation_type_id int unsigned not null,
	begin_date       date         not null,
	end_date         date         not null,
	comment          text         null default null,
	constraint fk_employees_vacations_1
		foreign key (employee_id) references employees (id)
			on update cascade on delete cascade,
	constraint fk_employees_vacations_2
		foreign key (vacation_type_id) references vacation_type (id)
			on delete restrict
			on update cascade
);

create index fk_employees_vacations_1_idx
	on employees_vacations (employee_id);

create index fk_employees_vacations_2_idx
	on employees_vacations (vacation_type_id);

alter table issuance_sheet
	add constraint fk_issuance_sheet_2
		foreign key (subdivision_id) references subdivisions (id)
			on delete no action
			on update cascade;

alter table issuance_sheet
	add constraint fk_issuance_sheet_8
		foreign key (transfer_agent_id) references employees (id)
			on delete no action
			on update cascade;

alter table issuance_sheet_items
	add constraint fk_issuance_sheet_items_2
		foreign key (employee_id) references employees (id)
			on delete no action
			on update cascade;

alter table leaders
	add constraint fk_leaders_1
		foreign key (employee_id) references employees (id)
			on delete no action
			on update cascade;

alter table operation_issued_by_employee
	add constraint fk_operation_issued_by_employee_1
		foreign key (employee_id) references employees (id)
			on delete restrict
			on update cascade;

alter table postomat_document_items
	add constraint fk_postomat_document_items_employee_id
		foreign key (employee_id) references employees (id);

alter table postomat_document_withdraw_items
	add constraint fk_postomat_document_withdraw_items_employee_id
		foreign key (employee_id) references employees (id);

alter table posts
	add constraint fk_posts_subdivision
		foreign key (subdivision_id) references subdivisions (id)
			on update cascade on delete set null;

alter table stock_collective_expense
	add constraint fk_stock_collective_expense_3
		foreign key (transfer_agent_id) references employees (id)
			on delete restrict
			on update cascade;

alter table stock_collective_expense_detail
	add constraint fk_stock_collective_expense_detail_6
		foreign key (employee_id) references employees (id)
			on delete no action
			on update cascade;

alter table stock_expense
	add constraint fk_stock_expense_employee
		foreign key (employee_id) references employees (id)
			on delete restrict
			on update cascade;

alter table stock_income
	add constraint fk_stock_income_employee
		foreign key (employee_id) references employees (id)
			on delete restrict
			on update cascade;

create index fk_subdivisions_1_idx
	on subdivisions (warehouse_id);

create index fk_subdivisions_2_idx
	on subdivisions (parent_subdivision_id);

create index index_subdivisions_code
	on subdivisions (code);

insert into employees_cost_allocation
select * from wear_cards_cost_allocation;

insert into employee_cards_item
select * from wear_cards_item;

insert into employees_norms
select * from wear_cards_norms;

insert into employees_sizes
select * from wear_cards_sizes;

insert into employees_vacations
select * from wear_cards_vacations;

drop table wear_cards_cost_allocation;

drop table wear_cards_item;

drop table wear_cards_norms;

drop table wear_cards_sizes;

drop table wear_cards_vacations;

drop table wear_cards;

drop table objects;
