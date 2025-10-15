
-- Table `employee_groups`

create table employee_groups
(
	id int unsigned not null auto_increment,
	name varchar(128) null,
	comment text null,
	PRIMARY KEY (`id`)
);

create index employee_groups_name_index
	on employee_groups (name);

-- Table `employee_group_items`

create table employee_group_items
(
	id int unsigned not null auto_increment,
	employee_group_id int unsigned not null,
	employee_id int unsigned not null,
	comment text null,
	PRIMARY KEY (`id`),
	constraint wear_card_groups_items_unique
		unique (employee_id, employee_group_id),
	constraint `foreign_key_employee_groups_items_employees`
		foreign key (employee_id) references wear_cards (id)
			on update cascade on delete cascade,
	constraint `foreign_key_employee_groups_items_employee_groups`
		foreign key (employee_group_id) references employee_groups (id)
			on update cascade on delete cascade
);

create index employee_groups_items_employee_groups_id_index
	on employee_group_items (employee_group_id);

create index employee_groups_items_employees_id_index
	on employee_group_items (employee_id);

-- Добавляем настройку списка обращений

ALTER TABLE `user_settings` 
    ADD `default_claim_list_type` ENUM('NotAnswered','NotClosed','All') NOT NULL DEFAULT 'NotClosed' AFTER `default_leader_id`;

-- Добавление общих данных для документа списания
alter table stock_write_off	collate = utf8mb3_general_ci;
alter table stock_write_off	add organization_id int unsigned null after user_id;
alter table stock_write_off	add director_id int unsigned null after organization_id;
alter table stock_write_off	add chairman_id int unsigned null after director_id;
create index fk_stock_write_off_chairman_id_idx	on stock_write_off (chairman_id);
create index fk_stock_write_off_director_idx	on stock_write_off (director_id);
create index fk_stock_write_off_organization_idx	on stock_write_off (organization_id);
alter table stock_write_off
	add constraint fk_stock_write_off_chairman_id
		foreign key (chairman_id) references leaders (id)
			on update cascade on delete set null;
alter table stock_write_off
	add constraint fk_stock_write_off_organization_id
		foreign key (organization_id) references organizations (id)
			on update cascade on delete set null;
alter table stock_write_off
	add constraint stock_inspection_fk_director_id
		foreign key (director_id) references leaders (id)
			on update cascade on delete set null;

-- Добавление комментария в строку списания
alter table stock_write_off_detail	add cause text null;

-- Члены комиссии документа списания
create table stock_write_off_members
(
	id int unsigned auto_increment
		primary key,
	write_off_id int unsigned not null,
	member_id    int unsigned not null,
	constraint stock_write_off_members_fk1
		foreign key (write_off_id) references stock_write_off (id)
			on update cascade on delete cascade,
	constraint stock_write_off_members_fk2
		foreign key (member_id) references leaders (id)
			on update cascade
);
create index stock_write_off_members_fk1_idx	on stock_write_off_members (write_off_id);
create index stock_write_off_members_fk2_idx	on stock_write_off_members (member_id);
