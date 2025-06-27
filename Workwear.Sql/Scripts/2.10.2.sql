-- Оказываемые услуги
create table clothing_service_services
(
	id   	int unsigned auto_increment,
	name 	varchar(60)       not null,
	cost 	decimal default 0 not null,
	code    varchar(13)       null,
	comment text       		  null
	constraint clothing_service_services_pk
		primary key (id)
);

create table clothing_service_services_nomenclature
(
	id   			int unsigned auto_increment,
	nomenclature_id int unsigned not null,
	service_id     	int unsigned not null,
	constraint clothing_service_services_pk
		primary key (id),
	constraint fk_services_nomenclature_nomenclature_id
		foreign key (nomenclature_id) references nomenclature (id)
			on update cascade on delete cascade,
	constraint fk_services_nomenclature_service_id
		foreign key (service_id) references clothing_service_services (id)
			on update cascade on delete cascade
);

