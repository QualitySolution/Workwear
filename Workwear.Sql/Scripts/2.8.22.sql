ALTER TABLE `stock_completion`
	ADD `doc_number` VARCHAR(16) NULL AFTER `id`;

ALTER TABLE `stock_transfer`
	ADD `doc_number` VARCHAR(16) NULL AFTER `id`;

ALTER TABLE `stock_transfer`
	ADD `organization_id` INT(10) UNSIGNED NULL AFTER `doc_number`;

ALTER TABLE `stock_transfer`
	ADD CONSTRAINT `fk_stock_transfer_4`
		FOREIGN KEY (`organization_id`)
			REFERENCES `organizations` (`id`)
			ON DELETE SET NULL
			ON UPDATE CASCADE;

CREATE INDEX `index_stock_transfer_organization`
	ON `stock_transfer` (`organization_id`);

DROP INDEX `index_stock_inspection_date` ON `stock_transfer`;

CREATE INDEX `index_stock_transfer_date`
	ON `stock_transfer` (`date`);
