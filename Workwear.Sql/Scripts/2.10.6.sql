-- Добавление даты прогноза в поставку
ALTER TABLE shipment ADD COLUMN warehouse_forecasting_date DATETIME NULL DEFAULT NULL AFTER user_id;
-- Добавление периода для каждой строки документа поставки
ALTER TABLE shipment_items ADD COLUMN start_period DATE NULL DEFAULT NULL AFTER diff_cause;
ALTER TABLE shipment_items ADD COLUMN end_period DATE NULL DEFAULT NULL AFTER start_period;

ALTER TABLE organizations
	ADD `okpo` CHAR(10) NULL comment 'Общероссийский классификатор предприятий и организаций' AFTER `name`;
