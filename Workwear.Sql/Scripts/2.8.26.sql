ALTER TABLE `norms`
	ADD COLUMN `last_update` TIMESTAMP DEFAULT CURRENT_TIMESTAMP() NOT NULL ON UPDATE CURRENT_TIMESTAMP() AFTER `id`,
	ADD INDEX `norms_last_update_idx` (`last_update` DESC);

ALTER TABLE `norms_item`
	ADD COLUMN `last_update` TIMESTAMP DEFAULT CURRENT_TIMESTAMP() NOT NULL ON UPDATE CURRENT_TIMESTAMP() AFTER `id`,
	ADD INDEX `norms_item_last_update_idx` (`last_update` DESC);

ALTER TABLE `protection_tools`
	ADD `dermal_ppe` TINYINT(1) DEFAULT 0 NOT NULL AFTER `item_types_id`;

ALTER TABLE `protection_tools`
	ADD `dispenser` TINYINT(1) DEFAULT 0 NOT NULL AFTER `dermal_ppe`;
