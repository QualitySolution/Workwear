-- Черновик документа выдачи
alter table stock_expense
	add issue_date date null DEFAULT date after date;

-- Записи  на посещение
create table visits
(
	id              int unsigned auto_increment,
	create_date     datetime              not null,
	visit_date      datetime              not null,
	employee_id     int unsigned          not null,
	employee_create boolean               null,
	done            boolean default FALSE null,
	cancelled       boolean default FALSE null,
	comment         text                  null,
	constraint visits_pk
		primary key (id),
	constraint visits_employees_id_fk
		foreign key (employee_id) references employees (id)
			on update cascade on delete set null
);

create index visits_create_date_index
	on visits (create_date);
create index visits_visit_date_index
	on visits (visit_date);


create table visits_documents
(
	id         int unsigned auto_increment
        primary key,
	visit_id   int unsigned not null,
	expence_id int unsigned null,
	writeof_id int unsigned null,
	return_id  int unsigned null,
	constraint visits_documents_stock_expense_id_fk
		foreign key (expence_id) references stock_expense (id)
			on update cascade on delete set null,
	constraint visits_documents_stock_return_id_fk
		foreign key (return_id) references stock_return (id)
			on update cascade on delete set null,
	constraint visits_documents_stock_write_off_organization_id_fk
		foreign key (writeof_id) references stock_write_off (id)
			on update cascade on delete set null,
	constraint visits_documents_visits_id_fk
		foreign key (visit_id) references visits (id)
			on update cascade on delete cascade
);

-- Учёт дней недели
create table work_days
(
	id          int unsigned auto_increment,
	date 		date 	not null,
	is_work_day boolean default true not null,
	comment 	text	null,
	constraint work_days_pk
		primary key (id)
);



