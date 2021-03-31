-- Исправляем расхождения в между новой базой и обновленной для колонки warehouse_id в обновленной устанавливалось NOT NULL, что не соотствовало работе программы.

ALTER TABLE `objects` 
DROP FOREIGN KEY `fk_objects_1`;

ALTER TABLE `objects` 
CHANGE COLUMN `warehouse_id` `warehouse_id` INT(10) UNSIGNED NULL DEFAULT NULL ;

ALTER TABLE `objects` 
ADD CONSTRAINT `fk_objects_1`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
  
-- Исправляем некорректный внешний ключ. Добаленный по ошибке в 2.4
  
ALTER TABLE `issuance_sheet` 
DROP FOREIGN KEY `fk_issuance_sheet_2`;
ALTER TABLE `issuance_sheet` 
ADD INDEX `fk_issuance_sheet_2_idx` (`subdivision_id` ASC);

ALTER TABLE `issuance_sheet` 
ADD CONSTRAINT `fk_issuance_sheet_2`
  FOREIGN KEY (`subdivision_id`)
  REFERENCES `objects` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;
