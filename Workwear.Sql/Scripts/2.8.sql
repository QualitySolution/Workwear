-- Создает новые таблицы размеров

CREATE TABLE IF NOT EXISTS `size_types` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  `use_in_employee` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Указывает отображается ли размер в карточке сотрудника',
  `category` ENUM('Size', 'Height') NOT NULL DEFAULT 'Size' COMMENT 'Вид размера, по сути определяет колонку для хранения в номенклатуре',
  `position` INT(11) NOT NULL DEFAULT 0 COMMENT 'Порядок сортировки антропометрических характеристик в карточке сотрудника',
  PRIMARY KEY (`id`))
ENGINE = InnoDB
AUTO_INCREMENT = 100
DEFAULT CHARACTER SET = utf8mb4
COMMENT = 'Внимание id до 100 пользователем создаваться не должны';

CREATE TABLE IF NOT EXISTS `sizes` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(10) NOT NULL,
  `size_type_id` INT(10) UNSIGNED NOT NULL,
  `use_in_employee` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Можно ли использовать в сотруднике',
  `use_in_nomenclature` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Можно ли использовать в складской номенклатуре',
  `alternative_name` VARCHAR(10) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_sizes_1_idx` (`size_type_id` ASC),
  CONSTRAINT `fk_sizes_1`
    FOREIGN KEY (`size_type_id`)
    REFERENCES `size_types` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1000
DEFAULT CHARACTER SET = utf8mb4
COMMENT = 'до 1000 id пользователь не может редактировать данные.';

CREATE TABLE IF NOT EXISTS `size_suitable` (
  `size_id` INT(10) UNSIGNED NOT NULL,
  `size_suitable_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`size_id`, `size_suitable_id`),
  INDEX `fk_size_suitable_2_idx` (`size_suitable_id` ASC),
  CONSTRAINT `fk_size_suitable_1`
    FOREIGN KEY (`size_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_size_suitable_2`
    FOREIGN KEY (`size_suitable_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `wear_cards_sizes` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `employee_id` INT(10) UNSIGNED NOT NULL COMMENT 'Сотрудник для которого установлен размер',
  `size_type_id` INT(10) UNSIGNED NOT NULL COMMENT 'Тип размера, не может быть установлено несколько размеров одного типа одному сотруднику',
  `size_id` INT(10) UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_wear_cards_sizes_1_idx` (`employee_id` ASC),
  INDEX `fk_wear_cards_sizes_2_idx` (`size_type_id` ASC),
  INDEX `fk_wear_cards_sizes_3_idx` (`size_id` ASC),
  CONSTRAINT `fk_wear_cards_sizes_1`
    FOREIGN KEY (`employee_id`)
    REFERENCES `wear_cards` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_sizes_2`
    FOREIGN KEY (`size_type_id`)
    REFERENCES `size_types` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_wear_cards_sizes_3`
    FOREIGN KEY (`size_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

-- Преднастроенные типы размеров

INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (1,'Рост',1,'Height',1);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (2,'Размер одежды',1,'Size',2);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (4,'Размер обуви',1,'Size',4);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (5,'Размер зимней обуви',1,'Size',5);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (6,'Размер головного убора',1,'Size',6);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (7,'Размер перчаток',1,'Size',8);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (8,'Размер рукавиц',1,'Size',9);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (3,'Размер зимней одежды',0,'Size',3);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (9, 'Размер зимнего головного убора', 1, 'Size', 7);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (10, 'Размер противогаза', 1, 'Size', 10);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (11, 'Размер респиратора', 1, 'Size', 11);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (12, 'Размер носков', 1, 'Size', 12);

-- Преднастроенные размеры
SELECT IF(str_value = 'True', 1, 0) INTO @range FROM `base_parameters` WHERE name = 'EmployeeSizeRanges';
SET @range = IFNULL(@range, 0);

INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (1, '146', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (2, '146-152', 1, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (3, '152', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (4, '155-166', 1, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (5, '158', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (6, '158-164', 1, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (7, '164', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (8, '167-178', 1, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (9, '170', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (10, '170-176', 1, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (11, '176', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (12, '179-190', 1, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (13, '182', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (14, '182-188', 1, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (15, '188', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (16, '191-200', 1, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (17, '194', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (18, '194-200', 1, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (19, '200', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (20, '201-210', 1, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (30, '38', 2, 1, 1, '76');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (31, '40', 2, 1, 1, '80');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (32, '40-42', 2, @range, 1, '80-84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (33, '42', 2, 1, 1, '84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (34, '44', 2, 1, 1, '88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (35, '44-46', 2, @range, 1, '88-92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (36, '46', 2, 1, 1, '92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (37, '48', 2, 1, 1, '96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (38, '48-50', 2, @range, 1, '96-100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (39, '50', 2, 1, 1, '100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (40, '50-52', 2, @range, 1, '100-104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (41, '52', 2, 1, 1, '104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (42, '52-54', 2, @range, 1, '104-108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (43, '54', 2, 1, 1, '108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (44, '56', 2, 1, 1, '112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (45, '56-58', 2, @range, 1, '112-116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (46, '58', 2, 1, 1, '116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (47, '58-60', 2, @range, 1, '116-120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (48, '60', 2, 1, 1, '120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (49, '60-62', 2, @range, 1, '120-124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (50, '62', 2, 1, 1, '124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (51, '62-64', 2, @range, 1, '124-128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (52, '64', 2, 1, 1, '128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (53, '64-66', 2, @range, 1, '128-132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (54, '66', 2, 1, 1, '132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (55, '68', 2, 1, 1, '136');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (56, '68-70', 2, @range, 1, '136-140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (57, '70', 2, 1, 1, '140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (58, '72', 2, 1, 1, '144');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (59, '72-74', 2, @range, 1, '144-148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (60, '74', 2, 1, 1, '148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (61, '74-76', 2, @range, 1, '148-152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (62, '76', 2, 1, 1, '152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (63, '76-78', 2, @range, 1, '152-156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (64, '78', 2, 1, 1, '156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (65, '80', 2, 1, 1, '160');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (66, '80-82', 2, @range, 1, '160-164');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (67, '82', 2, 1, 1, '164');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (68, 'XXS', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (69, 'XS', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (70, 'S', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (71, 'M', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (72, 'L', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (73, 'XL', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (74, 'XXL', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (75, 'XXXL', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (76, '4XL', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (77, '5XL', 2, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (78, '42-44', 2, @range, 1, '84-88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (79, '46-48', 2, @range, 1, '92-96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (80, '54-56', 2, @range, 1, '108-112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (90, '38', 3, 1, 1, '76');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (91, '40', 3, 1, 1, '80');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (92, '40-42', 3, @range, 1, '80-84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (93, '42', 3, 1, 1, '84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (94, '44', 3, 1, 1, '88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (95, '44-46', 3, @range, 1, '88-92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (96, '46', 3, 1, 1, '92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (97, '48', 3, 1, 1, '96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (98, '48-50', 3, @range, 1, '96-100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (99, '50', 3, 1, 1, '100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (100, '50-52', 3, @range, 1, '100-104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (101, '52', 3, 1, 1, '104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (102, '52-54', 3, @range, 1, '104-108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (103, '54', 3, 1, 1, '108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (104, '56', 3, 1, 1, '112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (105, '56-58', 3, @range, 1, '112-116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (106, '58', 3, 1, 1, '116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (107, '58-60', 3, @range, 1, '116-120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (108, '60', 3, 1, 1, '120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (109, '60-62', 3, @range, 1, '120-124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (110, '62', 3, 1, 1, '124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (111, '62-64', 3, @range, 1, '124-128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (112, '64', 3, 1, 1, '128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (113, '64-66', 3, @range, 1, '128-132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (114, '66', 3, 1, 1, '132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (115, '68', 3, 1, 1, '136');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (116, '68-70', 3, @range, 1, '136-140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (117, '70', 3, 1, 1, '140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (118, '72', 3, 1, 1, '144');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (119, '72-74', 3, @range, 1, '144-148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (120, '74', 3, 1, 1, '148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (121, '74-76', 3, @range, 1, '148-152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (122, '76', 3, 1, 1, '152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (123, '76-78', 3, @range, 1, '152-156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (124, '78', 3, 1, 1, '156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (125, '80', 3, 1, 1, '160');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (126, '80-82', 3, @range, 1, '160-164');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (127, '82', 3, 1, 1, '164');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (128, 'XXS', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (129, 'XS', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (130, 'S', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (131, 'M', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (132, 'L', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (133, 'XL', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (134, 'XXL', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (135, 'XXXL', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (136, '4XL', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (137, '5XL', 3, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (138, '42-44', 3, @range, 1, '84-88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (139, '46-48', 3, @range, 1, '92-96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (140, '54-56', 3, @range, 1, '108-112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (150, '34', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (151, '34-35', 4, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (152, '35', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (153, '36', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (154, '36-37', 4, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (155, '37', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (156, '38', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (157, '38-39', 4, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (158, '39', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (159, '40', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (160, '40-41', 4, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (161, '41', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (162, '42', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (163, '42-43', 4, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (164, '43', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (165, '44', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (166, '44-45', 4, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (167, '45', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (168, '46', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (169, '46-47', 4, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (170, '47', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (171, '48', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (172, '49', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (173, '50', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (190, '34', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (191, '34-35', 5, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (192, '35', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (193, '36', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (194, '36-37', 5, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (195, '37', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (196, '38', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (197, '38-39', 5, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (198, '39', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (199, '40', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (200, '40-41', 5, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (201, '41', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (202, '42', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (203, '42-43', 5, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (204, '43', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (205, '44', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (206, '44-45', 5, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (207, '45', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (208, '46', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (209, '46-47', 5, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (210, '47', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (211, '48', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (212, '49', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (213, '50', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (230, '54', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (231, '55', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (232, '56', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (233, '57', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (234, '58', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (235, '58-60', 6, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (236, '59', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (237, '59-60', 6, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (238, '60', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (239, '61', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (240, '62', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (241, '63', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (242, '64', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (243, '65', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (270, '6', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (271, '6,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (272, '7', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (273, '7,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (274, '8', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (275, '8,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (276, '8,5-9', 7, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (277, '9', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (278, '9,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (279, '10', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (280, '10,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (281, '11', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (282, '11,5', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (283, '12', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (284, '13', 7, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (300, '1', 8, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (301, '2', 8, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (302, '3', 8, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (310, '54', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (311, '55', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (312, '56', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (313, '57', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (314, '58', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (315, '58-60', 9, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (316, '59', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (317, '59-60', 9, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (318, '60', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (319, '61', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (320, '62', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (321, '63', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (322, '64', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (323, '65', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (330, '1', 10, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (331, '2', 10, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (332, '3', 10, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (333, '4', 10, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (340, '1', 11, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (341, '2', 11, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (342, '3', 11, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (343, '4', 11, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (350, '23', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (351, '23-25', 12, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (352, '25', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (353, '27', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (354, '27-29', 12, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (355, '29', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (356, '31', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (357, '31-33', 12, @range, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (358, '33', 12, 1, 0, NULL);

DELETE FROM `base_parameters` WHERE `base_parameters`.`name` = 'EmployeeSizeRanges';

-- Добавляем соответствия в размерах

INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (2,1);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (2,3);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (4,5);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (6,7);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (8,9);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (8,10);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (12,13);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (16,17);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (18,19);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (30,68);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (31,69);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (32,31);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (32,33);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (33,70);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (34,71);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (35,34);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (35,36);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (36,71);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (37,72);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (38,37);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (38,39);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (39,72);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (40,39);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (40,41);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (41,73);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (42,41);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (42,43);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (43,74);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (44,74);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (45,44);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (45,46);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (46,75);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (47,46);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (47,48);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (48,76);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (49,48);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (49,50);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (50,76);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (51,50);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (51,52);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (52,76);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (53,52);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (53,54);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (54,77);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (55,77);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (56,55);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (56,57);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (57,77);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (59,58);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (59,60);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (61,60);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (61,62);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (63,62);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (63,64);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (66,65);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (66,67);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (78, 33);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (78, 34);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (79, 36);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (79, 37);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (80, 43);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (80, 44);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (90,128);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (91,129);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (92,91);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (92,93);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (93,130);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (94,131);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (95,94);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (95,96);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (96,131);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (97,132);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (98,97);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (98,99);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (99,132);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (100,99);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (100,101);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (101,133);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (102,101);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (102,103);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (103,134);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (104,134);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (105,104);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (105,106);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (106,135);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (107,106);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (107,108);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (108,136);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (109,108);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (109,110);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (110,136);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (111,110);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (111,112);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (112,136);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (113,112);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (113,114);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (114,137);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (115,137);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (116,115);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (116,117);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (117,137);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (119,118);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (119,120);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (121,120);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (121,122);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (123,122);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (123,124);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (126,125);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (126,127);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (138, 93);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (138, 94);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (139, 96);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (139, 97);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (140, 103);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (140, 104);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (151,150);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (151,152);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (154,153);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (154,155);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (157,156);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (157,158);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (160,159);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (160,161);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (163,162);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (163,164);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (166,165);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (166,167);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (169,168);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (169,170);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (191,190);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (191,192);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (194,193);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (194,195);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (197,196);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (197,198);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (200,199);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (200,201);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (203,202);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (203,204);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (206,205);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (206,207);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (209,208);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (209,210);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (235,234);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (235,236);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (235, 238);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (237,236);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (237,238);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (276,275);
INSERT INTO size_suitable (`size_id`,`size_suitable_id`) VALUES (276,277);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (315, 314);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (315, 316);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (315, 318);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (317, 316);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (317, 318);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (351, 350);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (351, 352);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (354, 353);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (354, 355);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (357, 356);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (357, 358);

-- Выносим данные из полей карточек сотрудника

INSERT INTO wear_cards_sizes (employee_id, size_type_id, size_id)
SELECT wear_cards.id, 1, sizes.id 
FROM wear_cards
JOIN sizes ON sizes.size_type_id = 1 AND sizes.name = wear_cards.wear_growth;

INSERT INTO wear_cards_sizes (employee_id, size_type_id, size_id)
SELECT wear_cards.id, 2, sizes.id 
FROM wear_cards
JOIN sizes ON sizes.size_type_id = 2 AND sizes.name = wear_cards.size_wear;

INSERT INTO wear_cards_sizes (employee_id, size_type_id, size_id)
SELECT wear_cards.id, 4, sizes.id 
FROM wear_cards
JOIN sizes ON sizes.size_type_id = 4 AND sizes.name = wear_cards.size_shoes;

INSERT INTO wear_cards_sizes (employee_id, size_type_id, size_id)
SELECT wear_cards.id, 5, sizes.id 
FROM wear_cards
JOIN sizes ON sizes.size_type_id = 5 AND sizes.name = wear_cards.size_winter_shoes;

INSERT INTO wear_cards_sizes (employee_id, size_type_id, size_id)
SELECT wear_cards.id, 6, sizes.id 
FROM wear_cards
JOIN sizes ON sizes.size_type_id = 6 AND sizes.name = wear_cards.size_headdress;

INSERT INTO wear_cards_sizes (employee_id, size_type_id, size_id)
SELECT wear_cards.id, 7, sizes.id 
FROM wear_cards
JOIN sizes ON sizes.size_type_id = 7 AND sizes.name = wear_cards.size_gloves;

INSERT INTO wear_cards_sizes (employee_id, size_type_id, size_id)
SELECT wear_cards.id, 8, sizes.id 
FROM wear_cards
JOIN sizes ON sizes.size_type_id = 8 AND sizes.name = wear_cards.size_mittens;

-- Добавляем новые колонки в документы

ALTER TABLE `nomenclature` 
DROP COLUMN `size_std`;

ALTER TABLE `wear_cards` 
DROP COLUMN `size_mittens`,
DROP COLUMN `size_gloves_std`,
DROP COLUMN `size_gloves`,
DROP COLUMN `size_headdress_std`,
DROP COLUMN `size_headdress`,
DROP COLUMN `size_winter_shoes_std`,
DROP COLUMN `size_winter_shoes`,
DROP COLUMN `size_shoes_std`,
DROP COLUMN `size_shoes`,
DROP COLUMN `size_wear_std`,
DROP COLUMN `size_wear`,
DROP COLUMN `wear_growth`;

ALTER TABLE `stock_income_detail` 
ADD COLUMN `size_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `growth`,
ADD COLUMN `height_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `size_id`,
ADD INDEX `fk_stock_income_detail_4_idx` (`size_id` ASC),
ADD INDEX `fk_stock_income_detail_5_idx` (`height_id` ASC);

ALTER TABLE `stock_expense_detail` 
ADD COLUMN `size_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `protection_tools_id`,
ADD COLUMN `height_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `size_id`,
ADD INDEX `fk_stock_expense_detail_5_idx` (`size_id` ASC),
ADD INDEX `fk_stock_expense_detail_6_idx` (`height_id` ASC);

ALTER TABLE `stock_write_off_detail` 
ADD COLUMN `size_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `growth`,
ADD COLUMN `height_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `size_id`,
ADD INDEX `fk_stock_write_off_detail_5_idx` (`size_id` ASC),
ADD INDEX `fk_stock_write_off_detail_6_idx` (`height_id` ASC);

ALTER TABLE `operation_issued_by_employee` 
ADD COLUMN `size_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `growth`,
ADD COLUMN `height_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `size_id`,
ADD INDEX `fk_operation_issued_by_employee_7_idx` (`size_id` ASC),
ADD INDEX `fk_operation_issued_by_employee_8_idx` (`height_id` ASC);

ALTER TABLE `issuance_sheet_items` 
ADD COLUMN `size_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `growth`,
ADD COLUMN `height_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `size_id`,
ADD INDEX `fk_issuance_sheet_items_9_idx` (`height_id` ASC),
ADD INDEX `fk_issuance_sheet_items_8_idx` (`size_id` ASC);

ALTER TABLE `operation_issued_in_subdivision` 
ADD COLUMN `size_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `growth`,
ADD COLUMN `height_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `size_id`,
ADD INDEX `fk_operation_issued_in_subdivision_6_idx` (`size_id` ASC),
ADD INDEX `fk_operation_issued_in_subdivision_7_idx` (`height_id` ASC);

ALTER TABLE `operation_warehouse` 
ADD COLUMN `size_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `growth`,
ADD COLUMN `height_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `size_id`,
ADD INDEX `fk_operation_warehouse_4_idx` (`size_id` ASC),
ADD INDEX `fk_operation_warehouse_5_idx` (`height_id` ASC);

ALTER TABLE `stock_collective_expense_detail` 
ADD COLUMN `size_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `growth`,
ADD COLUMN `height_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `size_id`,
ADD INDEX `fk_stock_collective_expense_detail_7_idx` (`size_id` ASC),
ADD INDEX `fk_stock_collective_expense_detail_8_idx` (`height_id` ASC);

ALTER TABLE `stock_income_detail` 
ADD CONSTRAINT `fk_stock_income_detail_4`
  FOREIGN KEY (`size_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_stock_income_detail_5`
  FOREIGN KEY (`height_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `stock_expense_detail` 
ADD CONSTRAINT `fk_stock_expense_detail_5`
  FOREIGN KEY (`size_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_stock_expense_detail_6`
  FOREIGN KEY (`height_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `stock_write_off_detail` 
ADD CONSTRAINT `fk_stock_write_off_detail_5`
  FOREIGN KEY (`size_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_stock_write_off_detail_6`
  FOREIGN KEY (`height_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `operation_issued_by_employee` 
ADD CONSTRAINT `fk_operation_issued_by_employee_7`
  FOREIGN KEY (`size_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_operation_issued_by_employee_8`
  FOREIGN KEY (`height_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `issuance_sheet_items` 
ADD CONSTRAINT `fk_issuance_sheet_items_8`
  FOREIGN KEY (`size_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_issuance_sheet_items_9`
  FOREIGN KEY (`height_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `operation_issued_in_subdivision` 
ADD CONSTRAINT `fk_operation_issued_in_subdivision_6`
  FOREIGN KEY (`size_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_operation_issued_in_subdivision_7`
  FOREIGN KEY (`height_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `operation_warehouse` 
ADD CONSTRAINT `fk_operation_warehouse_4`
  FOREIGN KEY (`size_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_operation_warehouse_5`
  FOREIGN KEY (`height_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

ALTER TABLE `stock_collective_expense_detail` 
ADD CONSTRAINT `fk_stock_collective_expense_detail_7`
  FOREIGN KEY (`size_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_stock_collective_expense_detail_8`
  FOREIGN KEY (`height_id`)
  REFERENCES `sizes` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

-- Добавляем типы размеров в типы номенклатуры

ALTER TABLE `item_types` 
ADD COLUMN `size_type_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `comment`,
ADD COLUMN `height_type_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `size_type_id`,
ADD INDEX `fk_item_types_2_idx` (`size_type_id` ASC),
ADD INDEX `fk_item_types_3_idx` (`height_type_id` ASC);

ALTER TABLE `item_types` 
ADD CONSTRAINT `fk_item_types_2`
  FOREIGN KEY (`size_type_id`)
  REFERENCES `size_types` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE,
ADD CONSTRAINT `fk_item_types_3`
  FOREIGN KEY (`height_type_id`)
  REFERENCES `size_types` (`id`)
  ON DELETE NO ACTION
  ON UPDATE CASCADE;

UPDATE `item_types` SET 
`size_type_id`=
	CASE wear_category
    WHEN 'Wear' THEN 2
    WHEN 'Shoes' THEN 4
    WHEN 'WinterShoes' THEN 5
    WHEN 'Headgear' THEN 6
    WHEN 'Gloves' THEN 7
    WHEN 'Mittens' THEN 8
    END;

UPDATE `item_types` SET 
height_type_id=
	CASE wear_category
    WHEN 'Wear' THEN 1
    END;

-- Переносим размеры со старых полей на новые
-- Ведомости

UPDATE issuance_sheet_items SET size_id = (SELECT DISTINCT sizes.id 
        FROM issuance_sheet_items items
        LEFT JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        LEFT JOIN protection_tools ON items.protection_tools_id = protection_tools.id
        JOIN item_types ON item_types.id = protection_tools.item_types_id OR item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.size_type_id = sizes.size_type_id
        WHERE issuance_sheet_items.id = items.id
        AND sizes.name = issuance_sheet_items.size 
)
WHERE size IS NOT NULL;

UPDATE issuance_sheet_items SET height_id = (SELECT DISTINCT sizes.id 
        FROM issuance_sheet_items items
        LEFT JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        LEFT JOIN protection_tools ON items.protection_tools_id = protection_tools.id
        JOIN item_types ON item_types.id = protection_tools.item_types_id OR item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.height_type_id = sizes.size_type_id
        WHERE issuance_sheet_items.id = items.id
        AND sizes.name = issuance_sheet_items.growth
)
WHERE growth IS NOT NULL;

-- Операции выдачи сотруднику
UPDATE operation_issued_by_employee SET size_id = (SELECT DISTINCT sizes.id 
        FROM operation_issued_by_employee items
        LEFT JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        LEFT JOIN protection_tools ON items.protection_tools_id = protection_tools.id
        JOIN item_types ON item_types.id = protection_tools.item_types_id OR item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.size_type_id = sizes.size_type_id
        WHERE operation_issued_by_employee.id = items.id
        AND sizes.name = operation_issued_by_employee.size
)
WHERE size IS NOT NULL;

UPDATE operation_issued_by_employee SET height_id = (SELECT DISTINCT sizes.id 
        FROM operation_issued_by_employee items
        LEFT JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        LEFT JOIN protection_tools ON items.protection_tools_id = protection_tools.id
        JOIN item_types ON item_types.id = protection_tools.item_types_id OR item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.height_type_id = sizes.size_type_id
        WHERE operation_issued_by_employee.id = items.id
        AND sizes.name = operation_issued_by_employee.growth
)
WHERE growth IS NOT NULL;

-- Операции выдачи на подразделения
UPDATE operation_issued_in_subdivision SET size_id = (SELECT sizes.id 
        FROM operation_issued_in_subdivision items
        JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        JOIN item_types ON item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.size_type_id = sizes.size_type_id
        WHERE operation_issued_in_subdivision.id = items.id
        AND sizes.name = operation_issued_in_subdivision.size
)
WHERE size IS NOT NULL;

UPDATE operation_issued_in_subdivision SET height_id = (SELECT DISTINCT sizes.id 
        FROM operation_issued_in_subdivision items
        JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        JOIN item_types ON item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.height_type_id = sizes.size_type_id
        WHERE operation_issued_in_subdivision.id = items.id
        AND sizes.name = operation_issued_in_subdivision.growth
)
WHERE growth IS NOT NULL;

-- Складские операции
UPDATE operation_warehouse SET size_id = (SELECT sizes.id 
        FROM operation_warehouse items
        JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        JOIN item_types ON item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.size_type_id = sizes.size_type_id
        WHERE operation_warehouse.id = items.id
        AND sizes.name = operation_warehouse.size
)
WHERE size IS NOT NULL;

UPDATE operation_warehouse SET height_id = (SELECT DISTINCT sizes.id 
        FROM operation_warehouse items
        JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        JOIN item_types ON item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.height_type_id = sizes.size_type_id
        WHERE operation_warehouse.id = items.id
        AND sizes.name = operation_warehouse.growth
)
WHERE growth IS NOT NULL;

-- Коллективная выдача
UPDATE stock_collective_expense_detail SET size_id = (SELECT DISTINCT sizes.id 
        FROM stock_collective_expense_detail items
        LEFT JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        LEFT JOIN protection_tools ON items.protection_tools_id = protection_tools.id
        JOIN item_types ON item_types.id = protection_tools.item_types_id OR item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.size_type_id = sizes.size_type_id
        WHERE stock_collective_expense_detail.id = items.id
        AND sizes.name = stock_collective_expense_detail.size
)
WHERE size IS NOT NULL;

UPDATE stock_collective_expense_detail SET height_id = (SELECT DISTINCT sizes.id 
        FROM stock_collective_expense_detail items
        LEFT JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        LEFT JOIN protection_tools ON items.protection_tools_id = protection_tools.id
        JOIN item_types ON item_types.id = protection_tools.item_types_id OR item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.height_type_id = sizes.size_type_id
        WHERE stock_collective_expense_detail.id = items.id
        AND sizes.name = stock_collective_expense_detail.growth
)
WHERE growth IS NOT NULL;

-- Документ выдачи 
UPDATE stock_expense_detail SET size_id = (SELECT DISTINCT sizes.id 
        FROM stock_expense_detail items
        LEFT JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        LEFT JOIN protection_tools ON items.protection_tools_id = protection_tools.id
        JOIN item_types ON item_types.id = protection_tools.item_types_id OR item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.size_type_id = sizes.size_type_id
        WHERE stock_expense_detail.id = items.id
        AND sizes.name = stock_expense_detail.size
)
WHERE size IS NOT NULL;

UPDATE stock_expense_detail SET height_id = (SELECT DISTINCT sizes.id 
        FROM stock_expense_detail items
        LEFT JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        LEFT JOIN protection_tools ON items.protection_tools_id = protection_tools.id
        JOIN item_types ON item_types.id = protection_tools.item_types_id OR item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.height_type_id = sizes.size_type_id
        WHERE stock_expense_detail.id = items.id
        AND sizes.name = stock_expense_detail.growth
)
WHERE growth IS NOT NULL;

-- Документ поступления
UPDATE stock_income_detail SET size_id = (SELECT sizes.id 
        FROM stock_income_detail items
        JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        JOIN item_types ON item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.size_type_id = sizes.size_type_id
        WHERE stock_income_detail.id = items.id
        AND sizes.name = stock_income_detail.size
)
WHERE size IS NOT NULL;

UPDATE stock_income_detail SET height_id = (SELECT DISTINCT sizes.id 
        FROM stock_income_detail items
        JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        JOIN item_types ON item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.height_type_id = sizes.size_type_id
        WHERE stock_income_detail.id = items.id
        AND sizes.name = stock_income_detail.growth
)
WHERE growth IS NOT NULL;

-- Документ списания
UPDATE stock_write_off_detail SET size_id = (SELECT sizes.id 
        FROM stock_write_off_detail items
        JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        JOIN item_types ON item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.size_type_id = sizes.size_type_id
        WHERE stock_write_off_detail.id = items.id
        AND sizes.name = stock_write_off_detail.size
)
WHERE size IS NOT NULL;

UPDATE stock_write_off_detail SET height_id = (SELECT DISTINCT sizes.id 
        FROM stock_write_off_detail items
        JOIN nomenclature ON items.nomenclature_id = nomenclature.id
        JOIN item_types ON item_types.id = nomenclature.type_id
        JOIN sizes ON item_types.height_type_id = sizes.size_type_id
        WHERE stock_write_off_detail.id = items.id
        AND sizes.name = stock_write_off_detail.growth
)
WHERE growth IS NOT NULL;

-- Удаляем старые поля из документов

ALTER TABLE `stock_income_detail` 
DROP COLUMN `growth`,
DROP COLUMN `size`;

ALTER TABLE `stock_expense_detail` 
DROP COLUMN `growth`,
DROP COLUMN `size`;

ALTER TABLE `stock_write_off_detail` 
DROP COLUMN `growth`,
DROP COLUMN `size`;

ALTER TABLE `operation_issued_by_employee` 
DROP COLUMN `growth`,
DROP COLUMN `size`,
DROP INDEX `index9` ,
DROP INDEX `index8` ;

ALTER TABLE `issuance_sheet_items` 
DROP COLUMN `growth`,
DROP COLUMN `size`;

ALTER TABLE `operation_issued_in_subdivision` 
DROP COLUMN `growth`,
DROP COLUMN `size`,
DROP INDEX `index9` ,
DROP INDEX `index8` ;

ALTER TABLE `operation_warehouse` 
DROP COLUMN `growth`,
DROP COLUMN `size`,
DROP INDEX `index5` ,
DROP INDEX `index4` ;

ALTER TABLE `stock_collective_expense_detail` 
DROP COLUMN `growth`,
DROP COLUMN `size`;

-- Удаляем выдачу списком

DROP TABLE IF EXISTS `stock_mass_expense_operation` ;

DROP TABLE IF EXISTS `stock_mass_expense_nomenclatures` ;

DROP TABLE IF EXISTS `stock_mass_expense_employee` ;

ALTER TABLE `issuance_sheet`
DROP FOREIGN KEY `fk_issuance_sheet_6`,
DROP COLUMN `stock_mass_expense_id`;

DROP TABLE IF EXISTS `stock_mass_expense` ;

-- Добавляем дежурные сиз в нормы
ALTER TABLE norms_item
    CHANGE COLUMN `period_type` `period_type` ENUM('Year', 'Month', 'Shift', 'Wearout', 'Duty') NOT NULL DEFAULT 'Year';
-- Увеличиваем размер колонки с названием СИЗ
ALTER TABLE protection_tools
	CHANGE COLUMN `name` `name` VARCHAR(800) NOT NULL ;
	
-- Увеличиваем размеры полей с параметрами
ALTER TABLE `base_parameters` 
CHANGE COLUMN `name` `name` VARCHAR(80) NOT NULL ,
CHANGE COLUMN `str_value` `str_value` VARCHAR(500) NULL DEFAULT NULL;

-- Меняем тип колонки а номенклатурным номером.
ALTER TABLE `nomenclature`
	CHANGE COLUMN `number` `number` VARCHAR(20) NULL DEFAULT NULL ;

-- Добавляем собственников на продукцию
ALTER TABLE `operation_warehouse`
	ADD COLUMN `owner_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `cost`,
ADD INDEX `fk_operation_warehouse_6_idx` (`owner_id` ASC);

CREATE TABLE `owners` (
	`id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
	`name` VARCHAR(180) NOT NULL,
	`description` TEXT NULL DEFAULT NULL,
	`priority` INT(11) NOT NULL DEFAULT 0,
	PRIMARY KEY (`id`))
	ENGINE = InnoDB
	DEFAULT CHARACTER SET = utf8mb4;

ALTER TABLE `operation_warehouse`
	ADD CONSTRAINT `fk_operation_warehouse_6`
		FOREIGN KEY (`owner_id`)
			REFERENCES `owners` (`id`)
			ON DELETE SET NULL
			ON UPDATE NO ACTION;

-- Обновляем хранимую процедуру
DROP function IF EXISTS `count_issue`;

DELIMITER $$

CREATE FUNCTION `count_issue`(
	`amount` INT UNSIGNED,
	`norm_period` INT UNSIGNED,
	`next_issue` DATE,
	`begin_date` DATE,
	`end_date` DATE,
	`begin_Issue_Period` INT,
	`end_Issue_Period` INT)
	RETURNS int(10) unsigned
	NO SQL
	DETERMINISTIC
	COMMENT 'Функция рассчитывает количество необходимое к выдачи.'
BEGIN
	DECLARE issue_count INT;

	IF norm_period <= 0 THEN RETURN 0; END IF;
	IF next_issue IS NULL THEN RETURN 0; END IF;

	SET issue_count = 0;

	WHILE next_issue <= end_date DO
			IF next_issue >= begin_date THEN
				SET issue_count = issue_count + amount;
			END IF;
			SET next_issue = DATE_ADD(next_issue, INTERVAL norm_period MONTH);
			IF begin_Issue_Period IS NOT NULL THEN
				IF begin_Issue_Period < end_Issue_Period THEN
					IF MONTH(next_issue) BETWEEN begin_Issue_Period AND end_Issue_Period THEN
						SET next_issue = DATE_ADD(CONCAT(YEAR(next_issue) , '-', end_Issue_Period, '-01'), INTERVAL 1 MONTH);
					END IF;
				ELSE
					IF MONTH(next_issue) BETWEEN begin_Issue_Period AND 12 THEN
						SET next_issue = DATE_ADD(CONCAT(YEAR(next_issue), '-', end_Issue_Period, '-01'), INTERVAL '1T1' YEAR_MONTH);
					ELSEIF MONTH(next_issue) BETWEEN 1 AND end_Issue_Period THEN
						SET next_issue = DATE_ADD(CONCAT(YEAR(next_issue), '-', end_Issue_Period, '-01'), INTERVAL 1 MONTH);
					END IF;
				END IF;
			END IF;
		END WHILE;
	RETURN issue_count;
END$$

DELIMITER ;
