-- Добавление комментария в подразделения
ALTER TABLE `subdivisions` ADD COLUMN `comment` TEXT NULL DEFAULT NULL;

-- Связь услуг обслуживания со статусами
ALTER TABLE `clothing_service_services` ADD COLUMN `with_state` ENUM('WaitService','InReceiptTerminal','InTransit','DeliveryToLaundry','InRepair','InWashing','InDryCleaning','AwaitIssue','DeliveryToDispenseTerminal','InDispenseTerminal','Returned') NULL COMMENT 'Применять при проставлении статуса заявки на обслуживание' AFTER code;
