-- Исправляем расхождения в между новой базой и обновленной для колонки warehouse_id в обновленной устанавливалось NOT NULL, что не соотствовало работе программы.

ALTER TABLE `workwear`.`objects` 
DROP FOREIGN KEY `fk_objects_1`;

ALTER TABLE `workwear`.`objects` 
CHANGE COLUMN `warehouse_id` `warehouse_id` INT(10) UNSIGNED NULL DEFAULT NULL ;

ALTER TABLE `workwear`.`objects` 
ADD CONSTRAINT `fk_objects_1`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `workwear`.`warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;