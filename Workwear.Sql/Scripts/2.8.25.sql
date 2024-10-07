alter table protection_tools
	add supply_type enum ('Unisex', 'TwoSex') null after assessed_cost;

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


