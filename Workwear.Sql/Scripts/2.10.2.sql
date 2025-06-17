-- Оказываемые услуги
create table clothing_service_services
(
	id   	int unsigned auto_increment,
	name 	varchar(60)       not null,
	cost 	decimal default 0 not null,
	comment text       		  null
	constraint clothing_service_services_pk
		primary key (id)
);

