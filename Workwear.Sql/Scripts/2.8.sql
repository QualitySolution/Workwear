-- Создает новые таблицы размеров

CREATE TABLE IF NOT EXISTS `size_types` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  `use_in_employee` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Указывает отображается ли размер в карточке сотрудника',
  `category` ENUM('Size', 'Height') NOT NULL DEFAULT 'Size' COMMENT 'Вид размера, по сути определяет колонку для хранения в номеклатуре',
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
  `use_in_nomenclature` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Можно ли использовать в складской номеклатуре',
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
  `size_type_id` INT(10) UNSIGNED NOT NULL COMMENT 'Тип размера, не может быть устновлено несколько размеров одного типа одному сотруднику',
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
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (7,'Размер перчаток',1,'Size',7);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (8,'Размер рукавиц',1,'Size',8);
INSERT INTO `size_types` (`id`,`name`,`use_in_employee`,`category`,`position`) VALUES (3,'Размер зимней одежды',0,'Size',3);

-- Преднастроенные размеры

INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (1,'146',1,1,0);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (2,'146-152',1,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (3,'152',1,1,0);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (4,'155-166',1,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (5,'158',1,1,0);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (6,'158-164',1,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (7,'164',1,1,0);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (8,'167-178',1,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (9,'170',1,1,0);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (10,'170-176',1,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (11,'176',1,1,0);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (12,'179-190',1,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (13,'182',1,1,0);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (14,'182-188',1,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (15,'188',1,1,0);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (16,'191-200',1,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (17,'194',1,1,0);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (18,'194-200',1,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (19,'200',1,1,0);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (20,'201-210',1,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (30,'38',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (31,'40',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (32,'40-42',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (33,'42',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (34,'44',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (35,'44-46',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (36,'46',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (37,'48',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (38,'48-50',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (39,'50',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (40,'50-52',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (41,'52',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (42,'52-54',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (43,'54',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (44,'56',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (45,'56-58',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (46,'58',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (47,'58-60',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (48,'60',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (49,'60-62',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (50,'62',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (51,'62-64',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (52,'64',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (53,'64-66',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (54,'66',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (55,'68',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (56,'68-70',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (57,'70',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (58,'72',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (59,'72-74',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (60,'74',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (61,'74-76',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (62,'76',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (63,'76-78',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (64,'78',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (65,'80',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (66,'80-82',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (67,'82',2,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (68,'XXS',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (69,'XS',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (70,'S',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (71,'M',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (72,'L',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (73,'XL',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (74,'XXL',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (75,'XXXL',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (76,'4XL',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (77,'5XL',2,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (90,'38',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (91,'40',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (92,'40-42',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (93,'42',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (94,'44',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (95,'44-46',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (96,'46',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (97,'48',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (98,'48-50',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (99,'50',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (100,'50-52',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (101,'52',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (102,'52-54',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (103,'54',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (104,'56',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (105,'56-58',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (106,'58',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (107,'58-60',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (108,'60',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (109,'60-62',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (110,'62',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (111,'62-64',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (112,'64',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (113,'64-66',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (114,'66',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (115,'68',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (116,'68-70',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (117,'70',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (118,'72',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (119,'72-74',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (120,'74',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (121,'74-76',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (122,'76',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (123,'76-78',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (124,'78',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (125,'80',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (126,'80-82',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (127,'82',3,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (128,'XXS',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (129,'XS',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (130,'S',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (131,'M',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (132,'L',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (133,'XL',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (134,'XXL',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (135,'XXXL',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (136,'4XL',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (137,'5XL',3,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (150,'34',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (151,'34-35',4,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (152,'35',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (153,'36',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (154,'36-37',4,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (155,'37',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (156,'38',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (157,'38-39',4,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (158,'39',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (159,'40',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (160,'40-41',4,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (161,'41',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (162,'42',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (163,'42-43',4,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (164,'43',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (165,'44',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (166,'44-45',4,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (167,'45',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (168,'46',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (169,'46-47',4,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (170,'47',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (171,'48',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (172,'49',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (173,'50',4,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (190,'34',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (191,'34-35',5,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (192,'35',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (193,'36',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (194,'36-37',5,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (195,'37',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (196,'38',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (197,'38-39',5,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (198,'39',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (199,'40',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (200,'40-41',5,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (201,'41',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (202,'42',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (203,'42-43',5,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (204,'43',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (205,'44',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (206,'44-45',5,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (207,'45',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (208,'46',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (209,'46-47',5,0,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (210,'47',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (211,'48',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (212,'49',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (213,'50',5,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (230,'54',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (231,'55',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (232,'56',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (233,'57',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (234,'58',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (235,'58-60',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (236,'59',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (237,'59-60',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (238,'60',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (239,'61',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (240,'62',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (241,'63',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (242,'64',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (243,'65',6,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (270,'6',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (271,'6,5',5,7,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (272,'7',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (273,'7,5',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (274,'8',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (275,'8,5',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (276,'8,5-9',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (277,'9',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (278,'9,5',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (279,'10',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (280,'10,5',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (281,'11',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (282,'11,5',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (283,'12',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (284,'13',7,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (300,'1',8,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (301,'2',8,1,1);
INSERT INTO sizes (`id`,`name`,`size_type_id`,`use_in_employee`,`use_in_nomenclature`) VALUES (302,'3',8,1,1);

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
