-- ---------------------------------------------
-- Каталог, выбор пользователем предпочтительных номенклатур
-- ---------------------------------------------
alter table nomenclature
	add catalog_id char(24) null after archival;

alter table protection_tools_nomenclature
	add can_choose boolean default false not null;

create table employees_selected_nomenclatures
(
	id                  int unsigned auto_increment,
	employee_id         int unsigned not null,
	protection_tools_id int unsigned not null,
	nomenclature_id     int unsigned not null,
	constraint employees_selected_nomenclatures_pk
		primary key (id),
	constraint employees_selected_nomenclatures_employees_id_fk
		foreign key (employee_id) references employees (id)
			on update cascade on delete cascade,
	constraint employees_selected_nomenclatures_nomenclature_id_fk
		foreign key (nomenclature_id) references nomenclature (id)
			on update cascade on delete cascade,
	constraint employees_selected_nomenclatures_protection_tools_id_fk
		foreign key (protection_tools_id) references protection_tools (id)
			on update cascade on delete cascade
)
	comment 'Номенклатуры выбранные пользователем, как предпочтительные к выдаче';

