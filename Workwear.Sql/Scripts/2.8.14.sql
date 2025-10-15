# Снимаем лишнее ограничение
ALTER TABLE norms_item
	MODIFY amount INT UNSIGNED DEFAULT 1 NOT NULL;

ALTER TABLE issuance_sheet_items
	MODIFY amount INT UNSIGNED NOT NULL;
