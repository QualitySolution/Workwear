-- Исправляем индексы таблицы документа перемещения
ALTER TABLE `stock_transfer_detail` 
DROP FOREIGN KEY `fk_stock_transfer_detail_1`,
DROP FOREIGN KEY `fk_stock_transfer_detail_2`,
DROP FOREIGN KEY `fk_stock_transfer_detail_3`;

ALTER TABLE `stock_transfer_detail` 
CHARACTER SET = utf8mb4 , COLLATE = utf8mb4_general_ci ,
ADD INDEX `fk_stock_transfer_detail_1_idx` (`stock_transfer_id` ASC),
ADD INDEX `fk_stock_transfer_detail_2_idx` (`nomenclature_id` ASC),
ADD INDEX `fk_stock_transfer_detail_3_idx` (`warehouse_operation_id` ASC);

ALTER TABLE `stock_transfer_detail` 
ADD CONSTRAINT `fk_stock_transfer_detail_1`
  FOREIGN KEY (`stock_transfer_id`)
  REFERENCES `stock_transfer` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_stock_transfer_detail_2`
  FOREIGN KEY (`nomenclature_id`)
  REFERENCES `nomenclature` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_stock_transfer_detail_3`
  FOREIGN KEY (`warehouse_operation_id`)
  REFERENCES `operation_warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
