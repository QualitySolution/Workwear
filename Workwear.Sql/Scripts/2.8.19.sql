ALTER TABLE `postomat_document_items`
	MODIFY COLUMN `cell_number` VARCHAR(10) NULL DEFAULT NULL;

ALTER TABLE `posts`
	ADD `archival` TINYINT(1) NOT NULL DEFAULT 0 AFTER `cost_center_id`;

ALTER TABLE `norms`
	ADD `archival` TINYINT(1) NOT NULL DEFAULT 0  AFTER `dateto`;
