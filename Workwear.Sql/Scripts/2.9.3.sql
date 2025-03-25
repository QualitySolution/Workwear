-- Добавление 3-х новых статусов в стирку DeliveryToLaundry, InDryCleaning, DeliveryToDispenseTerminal

ALTER TABLE `clothing_service_states` CHANGE `state` `state` ENUM('WaitService','InReceiptTerminal','InTransit','DeliveryToLaundry','InRepair','InWashing','InDryCleaning','AwaitIssue','DeliveryToDispenseTerminal','InDispenseTerminal','Returned') NOT NULL; 

ALTER TABLE stock_write_off_detail
	ADD COLUMN duty_norm_issue_operation_id int(10) unsigned NULL DEFAULT NULL AFTER warehouse_operation_id;
ALTER TABLE stock_write_off_detail
	ADD CONSTRAINT fk_stock_write_off_detail_duty_norm_issue_operation
		FOREIGN KEY (duty_norm_issue_operation_id) REFERENCES operation_issued_by_duty_norm(id)
			ON DELETE NO ACTION
			ON UPDATE NO ACTION;
CREATE INDEX fk_stock_write_off_detail_duty_norm_issue_operation_idx
	ON stock_write_off_detail (duty_norm_issue_operation_id ASC);
