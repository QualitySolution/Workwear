################### Создание таблиц #############
# Создание склада
CREATE TABLE IF NOT EXISTS `warehouse` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

INSERT INTO `warehouse` (`id`, `name`) VALUES (DEFAULT, 'Основной склад');

# Создание складских операций
CREATE TABLE IF NOT EXISTS `operation_warehouse` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `operation_time` DATETIME NOT NULL,
  `warehouse_receipt_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_expense_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `nomenclature_id` INT(10) UNSIGNED NOT NULL,
  `size` VARCHAR(10) NULL DEFAULT NULL,
  `growth` VARCHAR(10) NULL DEFAULT NULL,
  `amount` INT(10) UNSIGNED NOT NULL,
  `wear_percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 0,
  `cost` DECIMAL(10,2) UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_operation_warehouse_2_idx` (`warehouse_receipt_id` ASC),
  INDEX `fk_operation_warehouse_3_idx` (`warehouse_expense_id` ASC),
  INDEX `index4` (`size` ASC),
  INDEX `index5` (`growth` ASC),
  INDEX `fk_operation_warehouse_1_idx` (`nomenclature_id` ASC),
  CONSTRAINT `fk_operation_warehouse_1`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_warehouse_2`
    FOREIGN KEY (`warehouse_receipt_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_operation_warehouse_3`
    FOREIGN KEY (`warehouse_expense_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

# Созданий операций на подразделение
CREATE TABLE IF NOT EXISTS `operation_issued_in_subdivision` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `operation_time` DATETIME NOT NULL,
  `subdivision_id` INT(10) UNSIGNED NOT NULL,
  `subdivision_place_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `nomenclature_id` INT(10) UNSIGNED NOT NULL,
  `size` VARCHAR(10) NULL DEFAULT NULL,
  `growth` VARCHAR(10) NULL DEFAULT NULL,
  `wear_percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 0.00,
  `issued` INT(11) NOT NULL DEFAULT 0,
  `returned` INT(11) NOT NULL DEFAULT 0,
  `auto_writeoff` TINYINT(1) NOT NULL DEFAULT 1,
  `auto_writeoff_date` DATE NULL DEFAULT NULL,
  `start_of_use` DATE NULL DEFAULT NULL,
  `expiry_on` DATE NULL DEFAULT NULL,
  `issued_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `operation_issued_by_employee_date` (`operation_time` ASC),
  INDEX `index8` (`size` ASC),
  INDEX `index9` (`growth` ASC),
  INDEX `index10` (`wear_percent` ASC),
  INDEX `fk_operation_issued_in_subdivision_1_idx` (`subdivision_id` ASC),
  INDEX `fk_operation_issued_in_subdivision_2_idx` (`nomenclature_id` ASC),
  INDEX `fk_operation_issued_in_subdivision_3_idx` (`issued_operation_id` ASC),
  INDEX `fk_operation_issued_in_subdivision_4_idx` (`warehouse_operation_id` ASC),
  INDEX `fk_operation_issued_in_subdivision_5_idx` (`subdivision_place_id` ASC),
  CONSTRAINT `fk_operation_issued_in_subdivision_1`
    FOREIGN KEY (`subdivision_id`)
    REFERENCES `objects` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_in_subdivision_2`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_operation_issued_in_subdivision_3`
    FOREIGN KEY (`issued_operation_id`)
    REFERENCES `operation_issued_in_subdivision` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_in_subdivision_4`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_in_subdivision_5`
    FOREIGN KEY (`subdivision_place_id`)
    REFERENCES `object_places` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

# Создание документа перемещения
CREATE TABLE IF NOT EXISTS `stock_transfer` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `warehouse_from_id` INT(10) UNSIGNED NOT NULL,
  `warehouse_to_id` INT(10) UNSIGNED NOT NULL,
  `date` DATETIME NOT NULL,
  `user_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_transfer_1_idx` (`warehouse_from_id` ASC),
  INDEX `fk_stock_transfer_2_idx` (`warehouse_to_id` ASC),
  UNIQUE INDEX `id_UNIQUE` (`id` ASC),
  INDEX `fk_stock_transfer_3_idx` (`user_id` ASC),
  CONSTRAINT `fk_stock_transfer_1`
    FOREIGN KEY (`warehouse_from_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_2`
    FOREIGN KEY (`warehouse_to_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_3`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

# Создание строк документа перемещения
CREATE TABLE IF NOT EXISTS `stock_transfer_detail` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_transfer_id` INT(10) UNSIGNED NOT NULL,
  `nomenclature_id` INT(10) UNSIGNED NOT NULL,
  `quantity` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE INDEX `id_UNIQUE` (`id` ASC),
  CONSTRAINT `fk_stock_transfer_detail_1`
    FOREIGN KEY (`id`)
    REFERENCES `stock_transfer` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_detail_2`
    FOREIGN KEY (`id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_detail_3`
    FOREIGN KEY (`id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

##################Изменение таблиц и удаление старых связей##################

# Добавление склада в таблицу подразделений (objects)
ALTER TABLE `objects` 
CHANGE COLUMN `address` `address` TEXT NULL DEFAULT NULL AFTER `code`,
ADD COLUMN `warehouse_id` INT(10) UNSIGNED NOT NULL DEFAULT 1 AFTER `name`,
ADD INDEX `fk_objects_1_idx` (`warehouse_id` ASC);

# Добавление склада в таблицу прихода (stock_income)

ALTER TABLE `stock_income` 
ADD COLUMN `warehouse_id` INT(10) UNSIGNED NULL DEFAULT 1 AFTER `date`,
ADD INDEX `fk_stock_income_warehouse_idx` (`warehouse_id` ASC);
ALTER TABLE `stock_income` 
ADD CONSTRAINT `fk_stock_income_warehouse`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
  
# Изменение строк документа прихода (stock_income_detail)

ALTER TABLE `stock_income_detail` 
DROP FOREIGN KEY `fk_stock_income_detail_stock_expense`;

ALTER TABLE `stock_income_detail` 
ADD COLUMN `subdivision_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `employee_issue_operation_id`,
ADD COLUMN `warehouse_operation_id` INT(10) UNSIGNED NOT NULL AFTER `subdivision_issue_operation_id`,
ADD COLUMN `size` VARCHAR(10) NULL DEFAULT NULL AFTER `warehouse_operation_id`,
ADD COLUMN `growth` VARCHAR(10) NULL DEFAULT NULL AFTER `size`,
ADD INDEX `fk_stock_income_detail_2_idx` (`warehouse_operation_id` ASC),
ADD INDEX `fk_stock_income_detail_3_idx` (`subdivision_issue_operation_id` ASC) ;  

# Измение документа выдачи
ALTER TABLE `stock_expense` 
ADD COLUMN `warehouse_id` INT(10) UNSIGNED NOT NULL DEFAULT 1 AFTER `operation`,
ADD INDEX `fk_stock_expense_1_idx` (`warehouse_id` ASC);

# Измение строк документа выдачи
ALTER TABLE `stock_expense_detail` 
ADD COLUMN `subdivision_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `employee_issue_operation_id`,
ADD COLUMN `warehouse_operation_id` INT(10) UNSIGNED NOT NULL AFTER `subdivision_issue_operation_id`,
ADD COLUMN `size` VARCHAR(10) NULL DEFAULT NULL AFTER `warehouse_operation_id`,
ADD COLUMN `growth` VARCHAR(10) NULL DEFAULT NULL AFTER `size`,
ADD INDEX `fk_stock_expense_detail_2_idx` (`warehouse_operation_id` ASC),
ADD INDEX `fk_stock_expense_detail_3_idx` (`subdivision_issue_operation_id` ASC);

ALTER TABLE `stock_expense_detail` 
DROP FOREIGN KEY `fk_stock_expense_detail_enter_row`;

# Добавление колонок в строки документа списания
ALTER TABLE `stock_write_off_detail` 
ADD COLUMN `subdivision_issue_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `employee_issue_operation_id`,
ADD COLUMN `warehouse_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `subdivision_issue_operation_id`,
ADD COLUMN `warehouse_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `warehouse_id`,
ADD COLUMN `size` VARCHAR(10) NULL DEFAULT NULL AFTER `warehouse_operation_id`,
ADD COLUMN `growth` VARCHAR(10) NULL DEFAULT NULL AFTER `size`,
ADD INDEX `fk_stock_write_off_detail_2_idx` (`warehouse_operation_id` ASC),
ADD INDEX `fk_stock_write_off_detail_3_idx` (`warehouse_id` ASC),
ADD INDEX `fk_stock_write_off_detail_4_idx` (`subdivision_issue_operation_id` ASC);

ALTER TABLE `stock_write_off_detail` 
DROP FOREIGN KEY `fk_stock_write_off_detail_income`,
DROP FOREIGN KEY `fk_stock_write_off_detail_expense`;

#Добавление колонок в нормы
ALTER TABLE `norms` 
ADD COLUMN `datefrom` DATETIME NULL DEFAULT NULL AFTER `comment`,
ADD COLUMN `dateto` DATETIME NULL DEFAULT NULL AFTER `datefrom`;

# Добавление колонок в таблицу операций сотрудников
ALTER TABLE `operation_issued_by_employee` 
DROP FOREIGN KEY `fk_operation_issued_by_employee_4`;

ALTER TABLE `operation_issued_by_employee` 
ADD COLUMN `size` VARCHAR(10) NULL DEFAULT NULL AFTER `nomenclature_id`,
ADD COLUMN `growth` VARCHAR(10) NULL DEFAULT NULL AFTER `size`,
ADD COLUMN `warehouse_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `issued_operation_id`,
ADD INDEX `fk_operation_issued_by_employee_6_idx` (`warehouse_operation_id` ASC),
ADD INDEX `index8` (`size` ASC),
ADD INDEX `index9` (`growth` ASC),
ADD INDEX `index10` (`wear_percent` ASC);

# Добавление колонок в строки документа ведомости на выдачу
ALTER TABLE `issuance_sheet_items` 
ADD COLUMN `size` VARCHAR(10) NULL DEFAULT NULL AFTER `lifetime`,
ADD COLUMN `growth` VARCHAR(10) NULL DEFAULT NULL AFTER `size`,
CHANGE COLUMN `lifetime` `lifetime` DECIMAL(5,2) UNSIGNED NULL DEFAULT NULL ;

ALTER TABLE `issuance_sheet` 
DROP FOREIGN KEY `fk_issuance_sheet_2`;

# Добавление колонки в таблицу номенклатуры
ALTER TABLE `nomenclature` 
ADD COLUMN `number` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `comment`;

################################### Перенос данных##############################

###########################      Для stock_income  и  stock_income_detail    ###################

#Добавление склада в таблицу прихода
update stock_income set warehouse_id = (select id from warehouse limit 1);

#Добавление размеров в строки документа прихода из справочника номенклатур
update stock_income_detail
JOIN nomenclature on stock_income_detail.nomenclature_id = nomenclature.id 
SET stock_income_detail.size = nomenclature.size, stock_income_detail.growth = nomenclature.growth;

# Добавление временного столбца для сохранения id stock_income_detail
ALTER TABLE `operation_warehouse` 
ADD COLUMN `work_id` INT NULL AFTER `cost`;

#Создание складских операций по строкам документа прихода
INSERT INTO operation_warehouse (operation_time, warehouse_receipt_id, warehouse_expense_id, nomenclature_id, size, growth, amount, wear_percent, cost, work_id)
SELECT  stock_income.date, (select id from warehouse limit 1) as warehouse, null, 
		uniq_nomenclature.uniq_id_nomen, stock_income_detail.size, stock_income_detail.growth, stock_income_detail.quantity,
		case when stock_income_detail.life_percent <= 0 then 0 else 1 - stock_income_detail.life_percent end, 
		stock_income_detail.cost,
        stock_income_detail.id
FROM stock_income_detail
JOIN stock_income on stock_income_detail.stock_income_id = stock_income.id
JOIN nomenclature on nomenclature.id = stock_income_detail.nomenclature_id
JOIN (
		SELECT n.id as id_nomen, n.name, n.type_id, n.sex, n.size, n.growth, uniq.id as uniq_id_nomen 
        FROM nomenclature as n
		JOIN (
			select id, name, type_id, sex 
            from nomenclature 
			group by BINARY name, type_id, sex ) 
			as uniq on BINARY uniq.name = BINARY n.name and uniq.type_id = n.type_id and uniq.sex = n.sex) as uniq_nomenclature on uniq_nomenclature.id_nomen = nomenclature.id;
   
  
#  В operation_issued_by_employee добавляется size growth wear_percent, проставляется верный nomenclature_id
# ТОЛЬКО ОПЕРАЦИИ ВОЗВРАТА ОТ СОТРУДНИКОВ

UPDATE operation_issued_by_employee 
JOIN stock_income_detail on  stock_income_detail.employee_issue_operation_id = operation_issued_by_employee.id
JOIN (
		SELECT n.id as id_nomen, n.name, n.type_id, n.sex, n.size, n.growth, uniq.id as uniq_id_nomen 
        FROM nomenclature as n
		JOIN (
			SELECT id, name, type_id, sex 
            FROM nomenclature 
			GROUP BY BINARY name, type_id, sex ) 
			as uniq on BINARY uniq.name = BINARY n.name and uniq.type_id = n.type_id and uniq.sex = n.sex) as uniq_nomenclature on uniq_nomenclature.id_nomen = operation_issued_by_employee.nomenclature_id
SET operation_issued_by_employee.nomenclature_id =  uniq_nomenclature.uniq_id_nomen , operation_issued_by_employee.size = uniq_nomenclature.size, 
operation_issued_by_employee.growth = uniq_nomenclature.growth, 
operation_issued_by_employee.wear_percent = CASE WHEN stock_income_detail.life_percent <= 0 THEN 0 ELSE 1 - stock_income_detail.life_percent END;
 
 
# В самом stock_income_detail заменяется id номенклатуры на уникальный
UPDATE stock_income_detail
JOIN (
	SELECT n.id as id_nomen, n.name, n.type_id, n.sex, n.size, n.growth, uniq.id as uniq_id_nomen 
        FROM nomenclature as n
		JOIN (
			select id, name, type_id, sex 
            from nomenclature 
			group by BINARY name, type_id, sex ) 
			as uniq on BINARY uniq.name = BINARY n.name and uniq.type_id = n.type_id and uniq.sex = n.sex) as uniq_nomenclature on uniq_nomenclature.id_nomen = stock_income_detail.nomenclature_id
SET stock_income_detail.nomenclature_id = uniq_nomenclature.uniq_id_nomen; 
 
#   Проставление ссылок на складские операции. Версия запроса №2    
    UPDATE stock_income_detail
        JOIN
    operation_warehouse ON operation_warehouse.work_id = stock_income_detail.id
SET 
    warehouse_operation_id = operation_warehouse.id;   
    
    ###########################      Для stock_expense  и  stock_expense_detail    ###################
    
#Добавление склада в таблицу выдачи
update stock_expense set warehouse_id = (select id from warehouse limit 1);

#Добавление размеров в строки документа выдачи из справочника номенклатур
update stock_expense_detail
JOIN nomenclature on stock_expense_detail.nomenclature_id = nomenclature.id 
SET stock_expense_detail.size = nomenclature.size, stock_expense_detail.growth = nomenclature.growth;

#Создание складских операций по строкам документа выдачи
INSERT INTO operation_warehouse (operation_time, warehouse_receipt_id, warehouse_expense_id, nomenclature_id, size, growth, amount, wear_percent, cost, work_id)
SELECT  stock_expense.date, null, (select id from warehouse limit 1) as warehouse,  
		uniq_nomenclature.uniq_id_nomen, stock_expense_detail.size, stock_expense_detail.growth, stock_expense_detail.quantity,
		case when stock_income_detail.life_percent <= 0 then 0 else 1 - stock_income_detail.life_percent end, 
		stock_income_detail.cost,
        stock_expense_detail.id
FROM stock_expense_detail
JOIN stock_expense on stock_expense_detail.stock_expense_id = stock_expense.id
LEFT JOIN stock_income_detail on stock_expense_detail.stock_income_detail_id = stock_income_detail.id 
JOIN nomenclature on nomenclature.id = stock_expense_detail.nomenclature_id
JOIN (
		SELECT n.id as id_nomen, n.name, n.type_id, n.sex, n.size, n.growth, uniq.id as uniq_id_nomen 
        FROM nomenclature as n
		JOIN (
			select id, name, type_id, sex 
            from nomenclature 
			group by BINARY name, type_id, sex ) 
			as uniq on BINARY uniq.name = BINARY n.name and uniq.type_id = n.type_id and uniq.sex = n.sex) as uniq_nomenclature on uniq_nomenclature.id_nomen = nomenclature.id;
            
#  В operation_issued_by_employee добавляется size growth wear_percent, проставляется верный nomenclature_id
# ТОЛЬКО ОПЕРАЦИИ ВЫДАЧИ СОТРУДНИКАМ

UPDATE operation_issued_by_employee 
JOIN stock_expense_detail on  stock_expense_detail.employee_issue_operation_id = operation_issued_by_employee.id
LEFT JOIN stock_income_detail on stock_expense_detail.stock_income_detail_id = stock_income_detail.id 
JOIN (
		SELECT n.id as id_nomen, n.name, n.type_id, n.sex, n.size, n.growth, uniq.id as uniq_id_nomen 
        FROM nomenclature as n
		JOIN (
			SELECT id, name, type_id, sex 
            FROM nomenclature 
			GROUP BY BINARY name, type_id, sex ) 
			as uniq on BINARY uniq.name = BINARY n.name and uniq.type_id = n.type_id and uniq.sex = n.sex) as uniq_nomenclature on uniq_nomenclature.id_nomen = operation_issued_by_employee.nomenclature_id
SET operation_issued_by_employee.nomenclature_id =  uniq_nomenclature.uniq_id_nomen , operation_issued_by_employee.size = uniq_nomenclature.size, 
operation_issued_by_employee.growth = uniq_nomenclature.growth, 
operation_issued_by_employee.wear_percent = CASE WHEN stock_income_detail.life_percent <= 0 THEN 0 ELSE 1 - stock_income_detail.life_percent END;

# В самом stock_expense_detail заменяется id номенклатуры на уникальный
UPDATE stock_expense_detail
JOIN (
	SELECT n.id as id_nomen, n.name, n.type_id, n.sex, n.size, n.growth, uniq.id as uniq_id_nomen 
        FROM nomenclature as n
		JOIN (
			select id, name, type_id, sex 
            from nomenclature 
			group by BINARY name, type_id, sex ) 
			as uniq on BINARY uniq.name = BINARY n.name and uniq.type_id = n.type_id and uniq.sex = n.sex) as uniq_nomenclature on uniq_nomenclature.id_nomen = stock_expense_detail.nomenclature_id
SET stock_expense_detail.nomenclature_id = uniq_nomenclature.uniq_id_nomen; 
 
#   Проставление ссылок на складские операции. Версия запроса №2    
    UPDATE stock_expense_detail
        JOIN
    operation_warehouse ON operation_warehouse.work_id = stock_expense_detail.id
SET 
    warehouse_operation_id = operation_warehouse.id
    WHERE operation_warehouse.id > (SELECT max(warehouse_operation_id) FROM stock_income_detail);
   
      ###### Проставление ссылок на warehouse_operation в operation_issued_by_employee #######
    #### Операции выдачи сотруднику
    
    UPDATE operation_issued_by_employee
    JOIN stock_expense_detail ON operation_issued_by_employee.id = stock_expense_detail.employee_issue_operation_id
    SET operation_issued_by_employee.warehouse_operation_id = stock_expense_detail.warehouse_operation_id;
    
    
    #### Операции возврата от сотрудника
    
	UPDATE operation_issued_by_employee
    JOIN stock_income_detail ON operation_issued_by_employee.id = stock_income_detail.employee_issue_operation_id
    SET operation_issued_by_employee.warehouse_operation_id = stock_income_detail.warehouse_operation_id;
    
    

### Обновление ведомости на выдачу issuance_sheet_items. Обновление id номенклатуры и вставка размеров.

UPDATE issuance_sheet_items
JOIN (
		SELECT n.id as id_nomen, n.name, n.type_id, n.sex, n.size, n.growth, uniq.id as uniq_id_nomen
        FROM nomenclature as n
		JOIN (
			select id, name, type_id, sex
            from nomenclature
			group by name, type_id, sex )
			as uniq on uniq.name = n.name and uniq.type_id = n.type_id and uniq.sex = n.sex) as uniq_nomenclature on uniq_nomenclature.id_nomen = issuance_sheet_items.nomenclature_id
SET issuance_sheet_items.nomenclature_id = uniq_nomenclature.uniq_id_nomen,
 issuance_sheet_items.size = uniq_nomenclature.size,
  issuance_sheet_items.growth = uniq_nomenclature.growth;
  
  ###########################     Для stock_write_off  и  stock_write_off_detail    ###################
  
#Добавление склада в строки документа списания
update stock_write_off_detail set warehouse_id = (select id from warehouse limit 1)
where stock_write_off_detail.employee_issue_operation_id is null;

#Добавление размеров в строки документа выдачи из справочника номенклатур
update stock_write_off_detail
JOIN nomenclature on stock_write_off_detail.nomenclature_id = nomenclature.id 
SET stock_write_off_detail.size = nomenclature.size, stock_write_off_detail.growth = nomenclature.growth;

#Создание складских операций по строкам документа списания
INSERT INTO operation_warehouse (operation_time, warehouse_receipt_id, warehouse_expense_id, nomenclature_id, size, growth, amount, wear_percent, cost, work_id)
SELECT
stock_write_off.date,
null,
(select id from warehouse limit 1) as warehouse,
uniq_nomenclature.uniq_id_nomen,
stock_write_off_detail.size,
stock_write_off_detail.growth,
stock_write_off_detail.quantity,
stock_income_detail.life_percent,
stock_income_detail.cost,
stock_write_off_detail.id

FROM stock_write_off_detail
JOIN stock_write_off on stock_write_off_detail.stock_write_off_id = stock_write_off.id
JOIN stock_income_detail on stock_income_detail.id = stock_write_off_detail.stock_income_detail_id
JOIN nomenclature on nomenclature.id = stock_write_off_detail.nomenclature_id
JOIN (
		SELECT n.id as id_nomen, n.name, n.type_id, n.sex, n.size, n.growth, uniq.id as uniq_id_nomen
        FROM nomenclature as n
		JOIN (
			select id, name, type_id, sex
            from nomenclature
			group by name, type_id, sex )
			as uniq on uniq.name = n.name and uniq.type_id = n.type_id and uniq.sex = n.sex) as uniq_nomenclature on uniq_nomenclature.id_nomen = nomenclature.id
WHERE stock_write_off_detail.stock_income_detail_id is not null AND  stock_write_off_detail.stock_expense_detail_id is null ;

# В самом stock_expense_detail заменяется id номенклатуры на уникальный
UPDATE stock_write_off_detail
JOIN (
	SELECT n.id as id_nomen, n.name, n.type_id, n.sex, n.size, n.growth, uniq.id as uniq_id_nomen 
        FROM nomenclature as n
		JOIN (
			select id, name, type_id, sex 
            from nomenclature 
			group by BINARY name, type_id, sex ) 
			as uniq on BINARY uniq.name = BINARY n.name and uniq.type_id = n.type_id and uniq.sex = n.sex) as uniq_nomenclature on uniq_nomenclature.id_nomen = stock_write_off_detail.nomenclature_id
SET stock_write_off_detail.nomenclature_id = uniq_nomenclature.uniq_id_nomen; 
 
#   Проставление ссылок на складские операции. Версия запроса №2    
    UPDATE stock_write_off_detail
        JOIN
    operation_warehouse ON operation_warehouse.work_id = stock_write_off_detail.id
SET 
    warehouse_operation_id = operation_warehouse.id
    WHERE operation_warehouse.id > (SELECT max(warehouse_operation_id) FROM stock_expense_detail);
    
    ###### Удаление лишнего #########

##### Удаление дублей в таблице номенклатур ########
CREATE TEMPORARY TABLE t_temp
as  (
			SELECT uniq.id as uniq_id_nomen
			FROM nomenclature as n
		JOIN (
			SELECT id, name, type_id, sex 
            FROM nomenclature 
			GROUP BY BINARY name, type_id, sex ) 
			as uniq on BINARY uniq.name = BINARY n.name and uniq.type_id = n.type_id and uniq.sex = n.sex
);

DELETE from nomenclature
WHERE nomenclature.id not in (
   SELECT uniq_id_nomen FROM t_temp
);

### Удаление рабочего id 
ALTER TABLE `operation_warehouse` 
DROP COLUMN `work_id`;

-- Создание связей

ALTER TABLE `objects` 
ADD CONSTRAINT `fk_objects_1`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
  
ALTER TABLE `stock_income` 
ADD CONSTRAINT `fk_stock_income_1`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `stock_income_detail` 
ADD CONSTRAINT `fk_stock_income_detail_2`
  FOREIGN KEY (`warehouse_operation_id`)
  REFERENCES `operation_warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_stock_income_detail_3`
  FOREIGN KEY (`subdivision_issue_operation_id`)
  REFERENCES `operation_issued_in_subdivision` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `stock_expense` 
ADD CONSTRAINT `fk_stock_expense_1`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE RESTRICT
  ON UPDATE CASCADE;

ALTER TABLE `stock_expense_detail` 
ADD CONSTRAINT `fk_stock_expense_detail_2`
  FOREIGN KEY (`warehouse_operation_id`)
  REFERENCES `operation_warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_stock_expense_detail_3`
  FOREIGN KEY (`subdivision_issue_operation_id`)
  REFERENCES `operation_issued_in_subdivision` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `stock_write_off_detail` 
ADD CONSTRAINT `fk_stock_write_off_detail_2`
  FOREIGN KEY (`warehouse_operation_id`)
  REFERENCES `operation_warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_stock_write_off_detail_3`
  FOREIGN KEY (`warehouse_id`)
  REFERENCES `warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION,
ADD CONSTRAINT `fk_stock_write_off_detail_4`
  FOREIGN KEY (`subdivision_issue_operation_id`)
  REFERENCES `operation_issued_in_subdivision` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `operation_issued_by_employee` 
ADD CONSTRAINT `fk_operation_issued_by_employee_4`
  FOREIGN KEY (`warehouse_operation_id`)
  REFERENCES `operation_warehouse` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `issuance_sheet` 
ADD CONSTRAINT `fk_issuance_sheet_2`
  FOREIGN KEY (`organization_id`)
  REFERENCES `objects` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;
    
-- Удаление старых колонок
ALTER TABLE `nomenclature` 
DROP COLUMN `growth`,
DROP COLUMN `size`;

ALTER TABLE `stock_income_detail` 
DROP COLUMN `stock_expense_detail_id`,
DROP COLUMN `life_percent`,
DROP INDEX `fk_stock_income_detail_stock_expense_idx` ;

ALTER TABLE `stock_expense_detail` 
DROP COLUMN `auto_writeoff_date`,
DROP COLUMN `stock_income_detail_id`,
DROP INDEX `fk_stock_expense_detail_enter_row_idx` ;

ALTER TABLE `stock_write_off_detail` 
DROP COLUMN `stock_income_detail_id`,
DROP COLUMN `stock_expense_detail_id`,
DROP INDEX `fk_stock_write_off_detail_income_idx` ,
DROP INDEX `fk_stock_write_off_detail_expense_idx` ;

ALTER TABLE `wear_cards_item` 
DROP COLUMN `matched_nomenclature_id`;

ALTER TABLE `operation_issued_by_employee` 
DROP COLUMN `stock_income_detail_id`,
DROP INDEX `fk_operation_issued_by_employee_4_idx`;

ALTER TABLE `issuance_sheet` 
DROP INDEX `fk_issuance_sheet_2_idx` ;
  
-- Очистка истории прочитанных новостей
DELETE FROM `read_news` WHERE `feed_id` = "workwearnews";

-- Обновляем версию базы.
DELETE FROM base_parameters WHERE name = 'micro_updates';
UPDATE base_parameters SET str_value = '2.4' WHERE name = 'version';