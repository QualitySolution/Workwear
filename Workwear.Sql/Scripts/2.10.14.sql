-- Количество оказанной услуги (кг/шт/м и т.д.)

ALTER TABLE clothing_service_services_claim
	ADD COLUMN amount DECIMAL(10,2) UNSIGNED NOT NULL DEFAULT 1.00 AFTER cost;
