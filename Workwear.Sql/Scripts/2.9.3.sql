-- Добавление 3-х новых статусов в стирку DeliveryToLaundry, InDryCleaning, DeliveryToDispenseTerminal

ALTER TABLE `clothing_service_states` CHANGE `state` `state` ENUM('WaitService','InReceiptTerminal','InTransit','DeliveryToLaundry','InRepair','InWashing','InDryCleaning','AwaitIssue','DeliveryToDispenseTerminal','InDispenseTerminal','Returned') NOT NULL; 
