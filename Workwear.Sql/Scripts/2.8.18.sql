CREATE TABLE IF NOT EXISTS `protection_tools_category_for_analytics`
(
	`id`   INT(11) UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
	`name` VARCHAR(100)     NOT NULL,
	`comment` TEXT NULL DEFAULT NULL
);

ALTER TABLE `protection_tools`
	ADD COLUMN
		`category_for_analytic_id` INT UNSIGNED NULL DEFAULT NULL,
	ADD CONSTRAINT `FK_protection_tools_category_for_analytics`
		FOREIGN KEY (`category_for_analytic_id`)
			REFERENCES `protection_tools_category_for_analytics` (`id`)
			ON DELETE SET NULL
			ON UPDATE CASCADE;

ALTER TABLE `stock_collective_expense`
	ADD `doc_number` VARCHAR(16) NULL AFTER `id`;

ALTER TABLE `stock_expense`
	ADD `doc_number` VARCHAR(16) NULL AFTER `operation`;

ALTER TABLE `stock_income`
	ADD `doc_number` VARCHAR(16) NULL AFTER `operation`;

ALTER TABLE `issuance_sheet`
	ADD `doc_number` VARCHAR(16) NULL AFTER `id`;

ALTER TABLE `stock_write_off`
	ADD `doc_number` VARCHAR(16) NULL AFTER `id`;

ALTER TABLE `stock_inspection`
	ADD `doc_number` VARCHAR(16) NULL AFTER `id`;
