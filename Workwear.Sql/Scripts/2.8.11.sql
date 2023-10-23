
-- Table `employee_groups`

create table employee_groups
(
	id int unsigned auto_increment
		constraint `PRIMARY`
		primary key,
	name varchar(128) null,
	comment text null
);

create index employee_groups_name_index
	on employee_groups (name);

-- Table `employee_group_items`

create table employee_group_items
(
	id int unsigned auto_increment
		constraint `PRIMARY`
		primary key,
	employee_group_id int unsigned not null,
	employee_id int unsigned not null,
	comment text null,
	constraint foreign_key_employee_groups_items_employees
		foreign key (employee_id) references employees (id)
			on update cascade on delete cascade,
	constraint foreign_key_employee_groups_items_employee_groups
		foreign key (employee_group_id) references employee_groups (id)
			on update cascade on delete cascade
);

create index employee_groups_items_employee_groups_id_index
	on employee_group_items (employee_group_id);

create index employee_groups_items_employees_id_index
	on employee_group_items (employee_id);
