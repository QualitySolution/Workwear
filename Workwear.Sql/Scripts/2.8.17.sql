ALTER TABLE `postomat_documents`
	ADD `terminal_location` TEXT NULL;

ALTER TABLE `postomat_document_items`
	ADD `cell_number` INT(11) UNSIGNED NULL AFTER loc_cell;

CREATE TABLE IF NOT EXISTS `postomat_documents_withdraw`
(
	`id`          INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	`create_time` DATETIME         NOT NULL,
	`comment`     TEXT DEFAULT NULL,
	`user_id`     INT UNSIGNED     NULL,
	PRIMARY KEY (`id`),
	CONSTRAINT `fk_postomat_documents_withdraw_user_id`
		FOREIGN KEY (user_id) REFERENCES users (id)
			ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;

CREATE TABLE IF NOT EXISTS `postomat_document_withdraw_items`
(
	`id`                   INT(11) UNSIGNED NOT NULL AUTO_INCREMENT,
	`terminal_id`          INT(11) UNSIGNED NOT NULL,
	`terminal_location`    TEXT             DEFAULT NULL,
	`document_withdraw_id` INT(11) UNSIGNED NOT NULL,
	`employee_id`          INT UNSIGNED     NOT NULL,
	`nomenclature_id`      INT(10) UNSIGNED NOT NULL,
	`barcode_id`           INT(10) UNSIGNED DEFAULT NULL,
	PRIMARY KEY (`id`),
	KEY `fk_postomat_document_withdraw_items_documents_withdraw_id` (`document_withdraw_id`),
	KEY `fk_postomat_document_withdraw_items_employee_id` (`employee_id`),
	KEY `fk_postomat_document_withdraw_items_barcode_id` (`barcode_id`),
	KEY `fk_postomat_document_withdraw_items_nomenclature_id` (`nomenclature_id`),
	CONSTRAINT `fk_postomat_document_withdraw_items_employee_id` FOREIGN KEY (employee_id) REFERENCES wear_cards (id),
	CONSTRAINT `fk_postomat_document_withdraw_items_barcode_id` FOREIGN KEY (`barcode_id`) REFERENCES `barcodes` (`id`),
	CONSTRAINT `fk_postomat_document_withdraw_items_nomenclature_id` FOREIGN KEY (`nomenclature_id`) REFERENCES `nomenclature` (`id`),
	CONSTRAINT `fk_postomat_document_withdraw_items_postomat_documents_export_id` FOREIGN KEY (`document_withdraw_id`) REFERENCES `postomat_documents_withdraw` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;

ALTER TABLE message_templates
	MODIFY message_text TEXT NOT NULL;
ALTER TABLE message_templates
	ADD COLUMN (
		link_title VARCHAR(100) NULL,
		link VARCHAR(100) NULL
		);
