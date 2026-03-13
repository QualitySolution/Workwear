alter table clothing_service_services_claim
	add service_date datetime null comment 'Время последней активации услуги для заявки';
alter table clothing_service_services_claim
	modify service_id int unsigned not null;
alter table clothing_service_services_claim
	modify claim_id int unsigned not null;
