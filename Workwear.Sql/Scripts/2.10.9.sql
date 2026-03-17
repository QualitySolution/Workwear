alter table clothing_service_services_claim
	add service_date datetime null comment 'Время последней активации услуги для заявки';

-- Удаляем строки с NULL значениями перед установкой NOT NULL
delete from clothing_service_services_claim where service_id is null;
delete from clothing_service_services_claim where claim_id is null;

alter table clothing_service_services_claim
	modify service_id int unsigned not null;
alter table clothing_service_services_claim
	modify claim_id int unsigned not null;
