
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
	constraint foreign_key_employee_groups_items_employees
		foreign key (employee_id) references wear_cards (id)
			on update cascade on delete cascade,
	constraint foreign_key_employee_groups_items_employee_groups
		foreign key (employee_group_id) references employee_groups (id)
			on update cascade on delete cascade
);

create index employee_groups_items_employee_groups_id_index
	on employee_group_items (employee_group_id);

create index employee_groups_items_employees_id_index
	on employee_group_items (employee_id);
