-- Остаток на складе, зафиксированный при добавлении строки в планируемую поставку, и дата добавления строки.
ALTER TABLE shipment_items
	ADD COLUMN `in_stock_on_create` INT NULL DEFAULT NULL
		AFTER received,
	ADD COLUMN `creation_date` DATETIME NULL DEFAULT NULL
		AFTER in_stock_on_create;
