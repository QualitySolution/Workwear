-- Добавление дополнительной информации в номенклатуру
ALTER TABLE nomenclature
	ADD COLUMN additional_info TEXT NULL DEFAULT NULL AFTER number;
