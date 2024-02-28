-- Учет стирки
CREATE TABLE `clothing_service_claim` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `barcode_id` int(10) unsigned NOT NULL,
  `is_closed` tinyint(1) NOT NULL DEFAULT 0,
  `need_for_repair` tinyint(1) NOT NULL,
  `defect` text DEFAULT NULL COMMENT 'Описание дефекта при сдаче, который нужно починить.',
  PRIMARY KEY (`id`),
  KEY `barcode_id` (`barcode_id`),
  CONSTRAINT `fk_claim_barcode_id` FOREIGN KEY (`barcode_id`) REFERENCES `barcodes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE `clothing_service_states` (
   `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
   `claim_id` int(10) unsigned NOT NULL,
   `operation_time` datetime NOT NULL,
   `state` enum('WaitService','InTransit','InRepair','InWashing','AwaitIssue','Returned') NOT NULL,
   `user_id` int(10) unsigned DEFAULT NULL,
   `comment` text DEFAULT NULL,
   PRIMARY KEY (`id`),
   KEY `fk_clame_id` (`claim_id`),
   KEY `user_id` (`user_id`),
   CONSTRAINT `fk_clame_id` FOREIGN KEY (`claim_id`) REFERENCES `clothing_service_claim` (`id`),
   CONSTRAINT `fk_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Постаматы

CREATE TABLE `postomat_documents` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `last_update` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `create_time` datetime NOT NULL,
  `status` enum('New','Done','Deleted','') NOT NULL DEFAULT 'New',
  `type` enum('Income','Outgo','Correction','') NOT NULL,
  `terminal_id` int(11) unsigned NOT NULL,
  `comment` text DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `last_update` (`last_update`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE `postomat_document_items` (
   `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
   `document_id` int(10) unsigned NOT NULL,
   `nomenclature_id` int(10) unsigned NOT NULL,
   `barcode_id` int(10) unsigned DEFAULT NULL,
   `delta` int(11) NOT NULL,
   `loc_storage` int(11) unsigned NOT NULL,
   `loc_shelf` int(11) unsigned NOT NULL,
   `loc_cell` int(11) unsigned NOT NULL,
   PRIMARY KEY (`id`),
   KEY `fk_postomat_document_id` (`document_id`),
   KEY `fk_barcode_id` (`barcode_id`),
   KEY `fk_nomenclature_id` (`nomenclature_id`),
   CONSTRAINT `fk_barcode_id` FOREIGN KEY (`barcode_id`) REFERENCES `barcodes` (`id`),
   CONSTRAINT `fk_nomenclature_id` FOREIGN KEY (`nomenclature_id`) REFERENCES `nomenclature` (`id`),
   CONSTRAINT `fk_postomat_document_id` FOREIGN KEY (`document_id`) REFERENCES `postomat_documents` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

