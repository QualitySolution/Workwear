alter table protection_tools
	add supply_type enum ('Unisex', 'TwoSex') default 'Unisex' not null after assessed_cost;

alter table protection_tools
	add supply_uni_id int(10) unsigned null after supply_type;

alter table protection_tools
	add supply_male_id int(10) unsigned null after supply_uni_id;

alter table protection_tools
	add supply_female_id int(10) unsigned null after supply_male_id;

alter table protection_tools
	add constraint protection_tools_nomenclature_female_id_fk
		foreign key (supply_female_id) references nomenclature (id)
			on update cascade on delete set null;

alter table protection_tools
	add constraint protection_tools_nomenclature_male_id_fk
		foreign key (supply_male_id) references nomenclature (id)
			on update cascade on delete set null;

alter table protection_tools
	add constraint protection_tools_nomenclature_uni_id_fk
		foreign key (supply_uni_id) references nomenclature (id)
			on update cascade on delete set null;
 create table causes_write_off
(
	id int auto_increment primary key,
	name varchar(120) not null
);
insert into causes_write_off (name) values ('увольнение'), ('преждевременный износ'), ('изменение должности'), ('прочее');

alter table stock_write_off_detail
	add column cause_write_off_id int
		references causes_write_off(id)
	after akt_number;

alter table stock_write_off_detail
	rename column cause to comment;

alter table stock_write_off_detail
	drop constraint stock_write_off_detail_ibfk_1;
alter table stock_write_off_detail
	add constraint stock_write_off_detail_ibfk_1 foreign key (cause_write_off_id) references causes_write_off (id)
		on update cascade on delete set null;
