alter table `postomat_documents`
	add `terminal_location` text null;

alter table `postomat_document_items`
	add `cell_number` int(11) UNSIGNED null;

create table if not exists `postomat_documents_withdraw`
(
	`id`           int(11) unsigned NOT NULL AUTO_INCREMENT,
	`create_time`  datetime         NOT NULL,
	`comment` text DEFAULT NULL,
	`user_id`     int unsigned null,
	PRIMARY KEY (`id`),
	constraint `fk_postomat_documents_withdraw_user_id`
		foreign key (user_id) references users (id)
			on update cascade on delete set null
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;

create table if not exists `postomat_document_withdraw_items`
(
	`id`                 int(11) unsigned NOT NULL AUTO_INCREMENT,
	`terminal_id`  		 int(11) unsigned NOT NULL,
	`terminal_location`  text 				   DEFAULT NULL,
	`document_withdraw_id` int(11) unsigned NOT NULL,
	`employee_id`        int UNSIGNED     NOT NULL,
	`nomenclature_id`    int(10) unsigned NOT NULL,
	`barcode_id`         int(10) unsigned      DEFAULT NULL,
	PRIMARY KEY (`id`),
	KEY `fk_postomat_document_withdraw_items_documents_withdraw_id` (`document_withdraw_id`),
	KEY `fk_postomat_document_withdraw_items_employee_id` (`employee_id`),
	KEY `fk_postomat_document_withdraw_items_barcode_id` (`barcode_id`),
	KEY `fk_postomat_document_withdraw_items_nomenclature_id` (`nomenclature_id`),
	constraint `fk_postomat_document_withdraw_items_employee_id` foreign key (employee_id) references wear_cards (id),
	CONSTRAINT `fk_postomat_document_withdraw_items_barcode_id` FOREIGN KEY (`barcode_id`) REFERENCES `barcodes` (`id`),
	CONSTRAINT `fk_postomat_document_withdraw_items_nomenclature_id` FOREIGN KEY (`nomenclature_id`) REFERENCES `nomenclature` (`id`),
	CONSTRAINT `fk_postomat_document_withdraw_items_postomat_documents_export_id` FOREIGN KEY (`document_withdraw_id`) REFERENCES `postomat_documents_withdraw` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_general_ci;
