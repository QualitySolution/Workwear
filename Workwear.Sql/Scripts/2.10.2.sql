-- Оказываемые услуги
create table clothing_service_services
(
	id   	int unsigned auto_increment,
	name 	varchar(60)       not null,
	cost 	decimal default 0 not null,
	code    varchar(13)       null,
	comment text       		  null,
	constraint clothing_service_services_pk
		primary key (id)
)
	auto_increment = 101;

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

create table clothing_service_services_claim
(
	id         int unsigned auto_increment,
	service_id int unsigned null,
	claim_id   int unsigned,
	constraint clothing_service_services_claim_pk
		primary key (id),
	constraint clothing_service_services_claim_clothing_service_claim_id_fk
		foreign key (claim_id) references clothing_service_claim (id)
			on update cascade on delete cascade,
	constraint clothing_service_services_claim_clothing_service_services_id_fk
		foreign key (service_id) references clothing_service_services (id)
			on update cascade on delete cascade
);

