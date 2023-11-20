-- Добавляем поле "Можно отдать в стирку" в номенклатуру
ALTER TABLE `nomenclature` ADD `washable` BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'Можно отдать в стирку' AFTER `use_barcode`; 
