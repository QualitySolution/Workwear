ALTER TABLE `protection_tools`
	ADD `supply_type` ENUM ('Unisex', 'TwoSex') DEFAULT 'Unisex' NOT NULL AFTER `assessed_cost`;

ALTER TABLE `protection_tools`
	ADD `supply_uni_id` INT(10) UNSIGNED NULL AFTER `supply_type`;

ALTER TABLE `protection_tools`
	ADD `supply_male_id` INT(10) UNSIGNED NULL AFTER `supply_uni_id`;

ALTER TABLE `protection_tools`
	ADD `supply_female_id` INT(10) UNSIGNED NULL AFTER `supply_male_id`;

ALTER TABLE `protection_tools`
	ADD CONSTRAINT `protection_tools_nomenclature_female_id_fk`
		FOREIGN KEY (`supply_female_id`) REFERENCES `nomenclature` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL;

ALTER TABLE `protection_tools`
	ADD CONSTRAINT `protection_tools_nomenclature_male_id_fk`
		FOREIGN KEY (`supply_male_id`) REFERENCES `nomenclature` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL;

ALTER TABLE `protection_tools`
	ADD CONSTRAINT `protection_tools_nomenclature_uni_id_fk`
		FOREIGN KEY (`supply_uni_id`) REFERENCES `nomenclature` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL;

CREATE TABLE `causes_write_off`
(
	`id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
	`name` VARCHAR(120) NOT NULL
);
INSERT INTO `causes_write_off` (`name`) VALUES ('Увольнение'), ('Преждевременный износ'), ('Изменение должности'), ('Прочее');

ALTER TABLE `stock_write_off_detail`
	ADD COLUMN `cause_write_off_id` INT UNSIGNED AFTER `akt_number`;

ALTER TABLE `stock_write_off_detail`
	ADD CONSTRAINT `fk_stock_write_off_detail_cause_write_off` FOREIGN KEY (`cause_write_off_id`) REFERENCES `causes_write_off` (`id`)
		ON UPDATE CASCADE ON DELETE SET NULL;
