-- Версионирование для типов номенклатуры
ALTER TABLE `item_types` ADD `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `id`;
ALTER TABLE `item_types` ADD INDEX(`last_update`);

-- Версионирование для номенклатуры
ALTER TABLE `nomenclature` ADD `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `id`;
ALTER TABLE `nomenclature` ADD INDEX(`last_update`); 
