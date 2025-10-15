-- Права на вход в настройки учета

ALTER TABLE `users`
	ADD COLUMN `can_accounting_settings` TINYINT(1) NOT NULL DEFAULT 1 AFTER `can_delete`;

-- Реализация поступления из поставки и поставки из прогноза
ALTER TABLE `shipment_items`
	ADD `diff_cause` VARCHAR(120) NULL AFTER `height_id`,
	ADD `ordered` INT UNSIGNED NOT NULL AFTER `quantity`,
	ADD `received` INT UNSIGNED NOT NULL AFTER `ordered`;

ALTER TABLE `shipment`
	MODIFY `status` ENUM ('Draft', 'New', 'Present', 'Accepted', 'Ordered', 'Received') DEFAULT 'Draft' NOT NULL,
	MODIFY `start_period` DATE NULL,
	MODIFY `end_period` DATE NULL,
	ADD `full_ordered` BOOLEAN DEFAULT FALSE NOT NULL AFTER `status`,
	ADD `full_received` BOOLEAN DEFAULT FALSE NOT NULL AFTER `full_ordered`,
	ADD `has_receive` BOOLEAN DEFAULT FALSE NOT NULL AFTER `full_received`,
	ADD `submitted` DATETIME NULL AFTER `has_receive`;

UPDATE `shipment` SET `full_ordered` = COALESCE((
												SELECT SUM(`shipment_items`.`quantity` > `shipment_items`.`ordered`) = 0
												FROM `shipment_items`
												WHERE `shipment_items`.`shipment_id` = `shipment`.`id`
											), FALSE);

ALTER TABLE `stock_income`
	ADD `shipment_id` INT UNSIGNED DEFAULT NULL NULL AFTER `warehouse_id`;
ALTER TABLE `stock_income`
	ADD CONSTRAINT `fk_stock_income_shipment`
		FOREIGN KEY (`shipment_id`) REFERENCES `shipment` (`id`);

ALTER TABLE `user_settings`
	ADD `buyer_email` VARCHAR(320) NULL AFTER `maximize_on_start`;
