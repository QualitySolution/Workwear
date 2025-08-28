-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema workwear
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Table `users`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `users` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  `login` VARCHAR(45) NOT NULL,
  `deactivated` TINYINT(1) NOT NULL DEFAULT 0,
  `email` VARCHAR(60) NULL DEFAULT NULL,
  `description` TEXT NULL DEFAULT NULL,
  `admin` TINYINT(1) NOT NULL DEFAULT FALSE,
  `can_delete` TINYINT(1) NOT NULL DEFAULT TRUE,
  `can_accounting_settings` TINYINT(1) NOT NULL DEFAULT TRUE,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `base_parameters`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `base_parameters` (
  `name` VARCHAR(80) NOT NULL,
  `str_value` VARCHAR(500) NULL DEFAULT NULL,
  PRIMARY KEY (`name`))
ENGINE = InnoDB;

CREATE TABLE `clothing_service_claim` (
  `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `barcode_id` int(10) unsigned NOT NULL,
  `employee_id` int UNSIGNED NOT NULL,
  `is_closed` tinyint(1) NOT NULL DEFAULT 0,
  `preferred_terminal_id` int(11) unsigned null,
  `need_for_repair` tinyint(1) NOT NULL,
  `defect` text DEFAULT NULL COMMENT 'Описание дефекта при сдаче, который нужно починить.',
  `comment` text null,
  PRIMARY KEY (`id`),
  KEY `barcode_id` (`barcode_id`),
  KEY `fk_clothing_service_claim_employee_id` (`employee_id`),
  constraint `fk_clothing_service_claim_employee_id` foreign key (employee_id) references employees (id),
  CONSTRAINT `fk_claim_barcode_id` FOREIGN KEY (`barcode_id`) REFERENCES `barcodes` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

CREATE TABLE `clothing_service_states` (
	`id` int(10) unsigned NOT NULL AUTO_INCREMENT,
	`claim_id` int(10) unsigned NOT NULL,
	`operation_time` datetime NOT NULL,
	`state` ENUM('WaitService','InReceiptTerminal','InTransit','DeliveryToLaundry','InRepair','InWashing','InDryCleaning','AwaitIssue','DeliveryToDispenseTerminal','InDispenseTerminal','Returned') NOT NULL,
	`user_id` int(10) unsigned DEFAULT NULL,
	`terminal_id` INT UNSIGNED NULL DEFAULT NULL COMMENT 'Номер постомата',
	`comment` text DEFAULT NULL,
	PRIMARY KEY (`id`),
	KEY `fk_clame_id` (`claim_id`),
	KEY `user_id` (`user_id`),
	CONSTRAINT `fk_clame_id` FOREIGN KEY (`claim_id`) REFERENCES `clothing_service_claim` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
	CONSTRAINT `fk_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- -----------------------------------------------------
-- Table `warehouse`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `warehouse` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `subdivisions`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `subdivisions` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code` VARCHAR(20) NULL DEFAULT NULL,
  `address` TEXT NULL DEFAULT NULL,
  `name` VARCHAR(240) NOT NULL,
  `warehouse_id` INT UNSIGNED NULL DEFAULT NULL,
  `parent_subdivision_id` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_subdivisions_1_idx` (`warehouse_id` ASC),
  INDEX `fk_subdivisions_2_idx` (`parent_subdivision_id` ASC),
  INDEX `index_subdivisions_code` (`code` ASC),
  CONSTRAINT `fk_subdivisions_1`
    FOREIGN KEY (`warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_subdivisions_2`
    FOREIGN KEY (`parent_subdivision_id`)
    REFERENCES `subdivisions` (`id`)
    ON DELETE SET NULL
    ON UPDATE NO ACTION)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4;



-- -----------------------------------------------------
-- Table `owners`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `owners` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(180) NOT NULL,
  `description` TEXT NULL DEFAULT NULL,
  `priority` INT(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;


-- -----------------------------------------------------
-- Table `departments`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `departments` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(254) NOT NULL,
  `code` VARCHAR(20) NULL DEFAULT NULL,
  `subdivision_id` INT UNSIGNED NULL DEFAULT NULL,
  `comments` TEXT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_departaments_1_idx` (`subdivision_id` ASC),
  CONSTRAINT `fk_departaments_1`
    FOREIGN KEY (`subdivision_id`)
    REFERENCES `subdivisions` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `professions`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `professions` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code` INT UNSIGNED NULL,
  `name` VARCHAR(200) NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `cost_center`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `cost_center` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `code` VARCHAR(14) NULL DEFAULT NULL,
  `name` VARCHAR(300) NOT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;

--
-- Table structure for table `postomat_document_items`
--

CREATE TABLE `postomat_document_items` (
   `id` int(10) unsigned NOT NULL AUTO_INCREMENT,
   `document_id` int(10) unsigned NOT NULL,
   `last_update` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
   `employee_id` int UNSIGNED NOT NULL,
   `nomenclature_id` int(10) unsigned NOT NULL,
   `barcode_id` int(10) unsigned DEFAULT NULL,
   `claim_id` INT UNSIGNED NULL DEFAULT NULL,
   `delta` int(11) NOT NULL,
   `loc_storage` int(11) unsigned NOT NULL,
   `loc_shelf` int(11) unsigned NOT NULL,
   `loc_cell` int(11) unsigned NOT NULL,
   `cell_number` varchar(10) null default null, 
   `dispense_time` DATETIME NULL DEFAULT NULL COMMENT 'Время выдачи постоматом',
   PRIMARY KEY (`id`),
   KEY `last_update` (`last_update`),
   KEY `fk_postomat_document_id` (`document_id`),
   KEY `fk_postomat_document_items_employee_id` (`employee_id`),
   KEY `fk_barcode_id` (`barcode_id`),
   KEY `fk_claim_id` (`claim_id`),
   KEY `fk_nomenclature_id` (`nomenclature_id`),
   constraint `fk_postomat_document_items_employee_id` foreign key (employee_id) references employees (id),
   CONSTRAINT `fk_barcode_id` FOREIGN KEY (`barcode_id`) REFERENCES `barcodes` (`id`),
   CONSTRAINT `fk_claim_id` FOREIGN KEY (`claim_id`) REFERENCES `clothing_service_claim`(`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
   CONSTRAINT `fk_nomenclature_id` FOREIGN KEY (`nomenclature_id`) REFERENCES `nomenclature` (`id`),
   CONSTRAINT `fk_postomat_document_id` FOREIGN KEY (`document_id`) REFERENCES `postomat_documents` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Table structure for table `postomat_documents`
--

CREATE TABLE `postomat_documents` (
  `id` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `last_update` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `create_time` datetime NOT NULL,
  `confirm_time` DATETIME NULL DEFAULT NULL COMMENT 'Время выполнения(done) документа на постомате.',
  `confirm_user` INT UNSIGNED NULL DEFAULT NULL COMMENT 'Пользователь системы постоматов проводивший документ.',
  `status` enum('New','Done','Deleted','') NOT NULL DEFAULT 'New',
  `type` enum('Income','Outgo','Correction','') NOT NULL,
  `terminal_id` int(11) unsigned NOT NULL,
  `comment` text DEFAULT NULL,
  `terminal_location` text DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `last_update` (`last_update`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Table structure for table `postomat_documents_export`
--

create table if not exists `postomat_documents_withdraw`
(
	`id`           int(11) unsigned NOT NULL AUTO_INCREMENT,
	`create_time`  datetime         NOT NULL,
	`comment` text DEFAULT NULL,
	`user_id`      int unsigned null,
	PRIMARY KEY (`id`),
	constraint `fk_postomat_documents_withdraw_user_id`
		foreign key (user_id) references users (id)
			on update cascade on delete set null
) ENGINE = InnoDB
	DEFAULT CHARSET = utf8mb4
	COLLATE = utf8mb4_general_ci;

--
-- Table structure for table `postomat_document_export_items`
--

create table if not exists `postomat_document_withdraw_items`
(
	`id`                 int(11) unsigned NOT NULL AUTO_INCREMENT,
	`terminal_id`  		 int(11) unsigned NOT NULL,
	`terminal_location`  text 				   DEFAULT NULL,
	`document_withdraw_id` int(11) unsigned NOT NULL,
	`employee_id`        int UNSIGNED     NOT NULL,
	`nomenclature_id`    int(10) unsigned NOT NULL,
	`barcode_id`         int(10) unsigned      DEFAULT NULL,
	PRIMARY KEY (`id`),
	KEY `fk_postomat_document_withdraw_items_documents_withdraw_id` (`document_withdraw_id`),
	KEY `fk_postomat_document_withdraw_items_employee_id` (`employee_id`),
	KEY `fk_postomat_document_withdraw_items_barcode_id` (`barcode_id`),
	KEY `fk_postomat_document_withdraw_items_nomenclature_id` (`nomenclature_id`),
	constraint `fk_postomat_document_withdraw_items_employee_id` foreign key (employee_id) references employees (id),
	CONSTRAINT `fk_postomat_document_withdraw_items_barcode_id` FOREIGN KEY (`barcode_id`) REFERENCES `barcodes` (`id`),
	CONSTRAINT `fk_postomat_document_withdraw_items_nomenclature_id` FOREIGN KEY (`nomenclature_id`) REFERENCES `nomenclature` (`id`),
	CONSTRAINT `fk_postomat_document_withdraw_items_postomat_documents_export_id` FOREIGN KEY (`document_withdraw_id`) REFERENCES `postomat_documents_withdraw` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
	) ENGINE = InnoDB
	DEFAULT CHARSET = utf8mb4
	COLLATE = utf8mb4_general_ci;


-- -----------------------------------------------------
-- Table `posts`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `posts` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(180) NOT NULL,
  `code` VARCHAR(20) NULL DEFAULT NULL,
  `subdivision_id` INT UNSIGNED NULL DEFAULT NULL,
  `department_id` INT UNSIGNED NULL DEFAULT NULL,
  `profession_id` INT UNSIGNED NULL DEFAULT NULL,
  `cost_center_id` INT UNSIGNED NULL DEFAULT NULL,
  `archival` TINYINT(1) NOT NULL DEFAULT 0,
  `comments` TEXT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_posts_subdivision_idx` (`subdivision_id` ASC),
  INDEX `fk_posts_department_idx` (`department_id` ASC),
  INDEX `fk_posts_professions_idx` (`profession_id` ASC),
  INDEX `fk_posts_1_idx` (`cost_center_id` ASC),
  CONSTRAINT `fk_posts_subdivision`
    FOREIGN KEY (`subdivision_id`)
    REFERENCES `subdivisions` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_posts_department`
    FOREIGN KEY (`department_id`)
    REFERENCES `departments` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_posts_professions`
    FOREIGN KEY (`profession_id`)
    REFERENCES `professions` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_posts_1`
    FOREIGN KEY (`cost_center_id`)
    REFERENCES `cost_center` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;


-- -----------------------------------------------------
-- Table `employees`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `employees` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `card_number` VARCHAR(15) NULL DEFAULT NULL,
  `personnel_number` VARCHAR(15) NULL DEFAULT NULL,
  `last_name` VARCHAR(20) NULL,
  `first_name` VARCHAR(20) NULL,
  `patronymic_name` VARCHAR(20) NULL,
  `card_key` VARCHAR(16) NULL DEFAULT NULL,
  `subdivision_id` INT UNSIGNED NULL DEFAULT NULL,
  `department_id` INT UNSIGNED NULL DEFAULT NULL,
  `hire_date` DATE NULL DEFAULT NULL,
  `change_of_position_date` DATE NULL DEFAULT NULL,
  `dismiss_date` DATE NULL DEFAULT NULL,
  `post_id` INT UNSIGNED NULL DEFAULT NULL,
  `leader_id` INT UNSIGNED NULL DEFAULT NULL,
  `sex` ENUM('F','M') NULL DEFAULT NULL,
  `birth_date` DATE NULL DEFAULT NULL,
  `user_id` INT UNSIGNED NULL DEFAULT NULL,
  `phone_number` VARCHAR(16) NULL DEFAULT NULL,
  `lk_registered` TINYINT(1) NOT NULL DEFAULT 0,
  `email` TEXT NULL DEFAULT NULL,
  `photo` MEDIUMBLOB NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `last_update` (`last_update` ASC),
  INDEX `fk_employees_subdivision_idx` (`subdivision_id` ASC),
  INDEX `fk_employees_post_idx` (`post_id` ASC),
  INDEX `fk_employees_leader_idx` (`leader_id` ASC),
  INDEX `fk_employees_user_idx` (`user_id` ASC),
  INDEX `fk_employees_department_idx` (`department_id` ASC),
  UNIQUE INDEX `card_key_UNIQUE` (`card_key` ASC),
  UNIQUE INDEX `card_number_UNIQUE` (`card_number` ASC),
  INDEX `index_employees_personal_number` (`personnel_number` ASC),
  INDEX `index_employees_last_name` (`last_name` ASC),
  INDEX `index_employees_first_name` (`first_name` ASC),
  INDEX `index_employees_patronymic_name` (`patronymic_name` ASC),
  INDEX `index_employees_dismiss_date` (`dismiss_date` ASC),
  INDEX `index_employees_phone_number` (`phone_number` ASC),
  CONSTRAINT `fk_employees_subdivision`
    FOREIGN KEY (`subdivision_id`)
    REFERENCES `subdivisions` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_employees_post`
    FOREIGN KEY (`post_id`)
    REFERENCES `posts` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_employees_leader`
    FOREIGN KEY (`leader_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_employees_user`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_employees_department`
    FOREIGN KEY (`department_id`)
    REFERENCES `departments` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_general_ci
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `leaders`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `leaders` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `surname` VARCHAR(50) NULL DEFAULT NULL,
  `name` VARCHAR(50) NULL DEFAULT NULL,
  `patronymic` VARCHAR(50) NULL DEFAULT NULL,
  `position` VARCHAR(150) NULL DEFAULT NULL,
  `employee_id` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_leaders_1_idx` (`employee_id` ASC),
  CONSTRAINT `fk_leaders_1`
    FOREIGN KEY (`employee_id`)
    REFERENCES `employees` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;


-- -----------------------------------------------------
-- Table `measurement_units`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `measurement_units` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(10) NOT NULL,
  `digits` TINYINT UNSIGNED NOT NULL DEFAULT 0,
  `okei` VARCHAR(3) NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `size_types`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `size_types` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  `use_in_employee` TINYINT(1) NOT NULL DEFAULT 1 COMMENT 'Указывает отображается ли размер в карточке сотрудника',
  `category` ENUM('Size', 'Height') NOT NULL DEFAULT 'Size' COMMENT 'Вид размера, по сути определяет колонку для хранения в номенклатуре',
  `position` INT NOT NULL DEFAULT 0 COMMENT 'Порядок сортировки антропометрических характеристик в карточке сотрудника',
  PRIMARY KEY (`id`))
ENGINE = InnoDB
AUTO_INCREMENT = 100
COMMENT = 'Внимание id до 100 пользователем создаваться не должны';


-- -----------------------------------------------------
-- Table `item_types`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `item_types` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `name` VARCHAR(240) NOT NULL,
  `wear_category` ENUM('Wear', 'Shoes', 'WinterShoes', 'Headgear', 'Gloves', 'Mittens', 'PPE') NULL DEFAULT NULL,
  `issue_type` ENUM('Personal', 'Collective') NOT NULL DEFAULT 'Personal',
  `units_id` INT UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  `size_type_id` INT UNSIGNED NULL DEFAULT NULL,
  `height_type_id` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `last_update` (`last_update` ASC),
  INDEX `fk_item_types_1_idx` (`units_id` ASC),
  INDEX `fk_item_types_2_idx` (`size_type_id` ASC),
  INDEX `fk_item_types_3_idx` (`height_type_id` ASC),
  CONSTRAINT `fk_item_types_1`
    FOREIGN KEY (`units_id`)
    REFERENCES `measurement_units` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_item_types_2`
    FOREIGN KEY (`size_type_id`)
    REFERENCES `size_types` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_item_types_3`
    FOREIGN KEY (`height_type_id`)
    REFERENCES `size_types` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;


-- -----------------------------------------------------
-- Table `nomenclature`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `nomenclature` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `name` VARCHAR(240) NOT NULL,
  `type_id` INT UNSIGNED NULL DEFAULT NULL,
  `sex` ENUM('Women','Men', 'Universal') NOT NULL DEFAULT 'Universal',
  `comment` TEXT NULL DEFAULT NULL,
  `number` VARCHAR(20) NULL DEFAULT NULL,
  `archival` TINYINT(1) NOT NULL DEFAULT 0,
  `rating` FLOAT NULL DEFAULT NULL,
  `rating_count` INT NULL DEFAULT NULL,
  `sale_cost` DECIMAL(10,2) UNSIGNED NULL DEFAULT NULL,
  `use_barcode` TINYINT(1) NOT NULL DEFAULT 0,
  `washable` BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'Можно отдать в стирку',
  PRIMARY KEY (`id`),
  INDEX `last_update` (`last_update` ASC),
  INDEX `fk_nomenclature_type_idx` (`type_id` ASC),
  INDEX `index_nomenclature_number` (`number` ASC),
  CONSTRAINT `fk_nomenclature_type`
    FOREIGN KEY (`type_id`)
    REFERENCES `item_types` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `history_changeset`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `history_changeset` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `user_login` VARCHAR(50) NULL DEFAULT NULL,
  `action_name` VARCHAR(100) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_history_changeset_1_idx` (`user_id` ASC),
  INDEX `history_changeset_login_idx` (`user_login` ASC),
  CONSTRAINT `fk_history_changeset_1`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;


-- -----------------------------------------------------
-- Table `history_changed_entities`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `history_changed_entities` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `changeset_id` INT(10) UNSIGNED NOT NULL,
  `datetime` DATETIME NOT NULL,
  `operation` ENUM('Create', 'Change', 'Delete') NOT NULL,
  `entity_class` VARCHAR(45) NOT NULL,
  `entity_id` INT(10) UNSIGNED NOT NULL,
  `entity_title` VARCHAR(200) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `ix_changeset_operation` (`operation` ASC),
  INDEX `fk_history_changed_entities_1_idx` (`changeset_id` ASC),
  INDEX `history_changed_entities_datetime_IDX` USING BTREE (`datetime`),
  INDEX `history_changed_entities_entity_class_IDX` USING BTREE (`entity_class`),
  INDEX `history_changed_entities_entity_id_IDX` USING BTREE (`entity_id`),
  CONSTRAINT `fk_history_changed_entities_1`
    FOREIGN KEY (`changeset_id`)
    REFERENCES `history_changeset` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;


-- -----------------------------------------------------
-- Table `history_changes`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `history_changes` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `changed_entity_id` INT(10) UNSIGNED NOT NULL,
  `type` ENUM('Added', 'Changed', 'Removed', 'Unchanged') NOT NULL DEFAULT 'Unchanged',
  `field_name` VARCHAR(80) NOT NULL,
  `old_value` TEXT NULL DEFAULT NULL,
  `old_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `new_value` TEXT NULL DEFAULT NULL,
  `new_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_change_entity_id_idx` (`changed_entity_id` ASC),
  INDEX `index_changesets_path` (`field_name` ASC),
  INDEX `history_changes_old_id_IDX` USING BTREE (`old_id`),
  INDEX `history_changes_new_id_IDX` USING BTREE (`new_id`),
  INDEX `history_changes_old_value_IDX` USING BTREE (`old_value`(100)),
  INDEX `history_changes_new_value_IDX` USING BTREE (`new_value`(100)),
  CONSTRAINT `fk_change_entity_id`
    FOREIGN KEY (`changed_entity_id`)
    REFERENCES `history_changed_entities` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1;


-- -----------------------------------------------------
-- Table `stock_income`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_income` (
	  id            int unsigned auto_increment
		  primary key,
	  doc_number    varchar(16)  null,
	  number        varchar(15)  null,
	  date          date         not null,
	  warehouse_id  int unsigned not null,
	  shipment_id   int unsigned null,
	  user_id       int unsigned null,
	  comment       text         null,
	  creation_date datetime     null,
	  INDEX `index_stock_income_date` (`date` ASC),
	  INDEX `fk_stock_income_user_idx` (`user_id` ASC),
	  INDEX `stock_income_warehouse_id_index` (`warehouse_id` ASC),
	  INDEX `stock_income_doc_number_index` (`doc_number`),
	  constraint fk_stock_income_user
		  foreign key (user_id) references users (id)
			  on update cascade on delete set null,
	  constraint fk_stock_income_warehouse
		  foreign key (warehouse_id) references warehouse (id)
			  on update cascade,
	  constraint fk_stock_income_shipment
		  foreign key (shipment_id) references shipment (id)
)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;

-- -----------------------------------------------------
-- Table `stock_income_items`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_income_items`
(
	id                     int unsigned auto_increment
		primary key,
	stock_income_id        int unsigned                         not null,
	nomenclature_id        int unsigned                         not null,
	quantity               int unsigned                         not null,
	cost                   decimal(10, 2) unsigned default 0.00 not null,
	certificate            varchar(40)                          null,
	warehouse_operation_id int unsigned                         not null,
	size_id                int unsigned                         null,
	height_id              int unsigned                         null,
	comment                varchar(120)                         null,
	INDEX `index_stock_income_items_doc` (`stock_income_id` ASC),
	INDEX `index_stock_income_items_size` (`size_id` ASC),
	INDEX `index_stock_income_items_height` (`height_id` ASC),
	INDEX `index_stock_income_items_nomenclature` (`nomenclature_id` ASC),
	INDEX `index_stock_income_items_warehouse_operation` (`warehouse_operation_id` ASC),
	constraint fk_stock_income_items_doc
		foreign key (stock_income_id) references stock_income (id)
			on update cascade on delete cascade,
	constraint fk_stock_income_items_height_id
		foreign key (height_id) references sizes (id)
			on update cascade,
	constraint fk_stock_income_items_nomenclature
		foreign key (nomenclature_id) references nomenclature (id)
			on update cascade,
	constraint fk_stock_income_items_operation_warehouse
		foreign key (warehouse_operation_id) references operation_warehouse (id),
	constraint fk_stock_income_items_size
		foreign key (size_id) references sizes (id)
			on update cascade
)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_general_ci;

-- -----------------------------------------------------
-- Table `stock_return`
-- -----------------------------------------------------
create table stock_return
(
	id            int unsigned auto_increment
		primary key,
	doc_number    varchar(16)  null,
	date          date         not null,
	warehouse_id  int unsigned not null,
	user_id       int unsigned null,
	comment       text         null,
	creation_date datetime     null,
	INDEX `index_stock_income_date` (`date` ASC),
	INDEX `stock_return_doc_number_index` (`doc_number` ASC),
	INDEX `stock_income_warehouse_id_index` (`warehouse_id` ASC),
	constraint stock_return_users_id_fk
		foreign key (user_id) references users (id)
			on update cascade on delete set null,
	constraint stock_return_warehouse_id_fk
		foreign key (warehouse_id) references warehouse (id)
			on update cascade on delete cascade
)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_general_ci;

-- -----------------------------------------------------
-- Table `stock_return_items`
-- -----------------------------------------------------
create table stock_return_items
(
	id                          int unsigned auto_increment
		primary key,
	stock_return_id             int unsigned not null,
	nomenclature_id             int unsigned not null,
	quantity                    int unsigned not null,
	employee_id					int unsigned null,
	employee_issue_operation_id int unsigned null,
	warehouse_operation_id      int unsigned not null,
	duty_norm_id				int unsigned null,
	duty_norm_issue_operation_id int unsigned null,
	size_id                     int unsigned null,
	height_id                   int unsigned null,
	comment_return              varchar(120) null,
	INDEX `index_stock_return_items_doc` (`stock_return_id` ASC),
	INDEX `index_stock_return_items_size` (`size_id` ASC),
	INDEX `index_stock_return_items_height` (`height_id` ASC),
	INDEX `index_stock_return_items_nomenclature` (`nomenclature_id` ASC),
	INDEX `index_stock_return_items_warehouse_operation` (`warehouse_operation_id` ASC),
	INDEX `stock_return_items_employee_id_index` (`employee_id` ASC),
	INDEX `stock_return_items_duty_norm_id_index` (`duty_norm_id` ASC),
	INDEX `stock_return_items_duty_norm_issue_operation_id_index` (`duty_norm_issue_operation_id` ASC),
	INDEX `stock_return_items_employee_issue_operation_id_index` (`employee_issue_operation_id` ASC),
	constraint fk_stock_return_items_doc
		foreign key (stock_return_id) references stock_return (id)
			on update cascade on delete cascade,
	constraint fk_stock_return_items_height
		foreign key (height_id) references sizes (id)
			on update cascade,
	constraint fk_stock_return_items_nomenclature
		foreign key (nomenclature_id) references nomenclature (id)
			on update cascade,
	constraint fk_stock_return_items_operation_issue
		foreign key (employee_issue_operation_id) references operation_issued_by_employee (id),
	constraint fk_stock_return_items_operation_warehouse
		foreign key (warehouse_operation_id) references operation_warehouse (id),
	constraint fk_stock_return_items_size
		foreign key (size_id) references sizes (id)
			on update cascade,
	constraint stock_return_items_employee_id_fk
		foreign key (employee_id) references employees(id)
			on update cascade 
			on delete restrict,
	constraint stock_return_items_duty_norm_id_fk
		foreign key (duty_norm_id) references duty_norms (id)
			on update cascade
			on delete restrict,
	constraint stock_return_items_duty_norm_issue_operation_id_fk
		foreign key (duty_norm_issue_operation_id) references operation_issued_by_duty_norm (id)
			on update cascade
			on delete restrict
)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_general_ci;

-- -----------------------------------------------------
-- Table `sizes`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `sizes` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(10) NOT NULL,
  `size_type_id` INT UNSIGNED NOT NULL,
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
DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_general_ci
COMMENT = 'до 1000 id пользователь не может редактировать данные.';


-- -----------------------------------------------------
-- Table `operation_warehouse`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `operation_warehouse` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `operation_time` DATETIME NOT NULL,
  `warehouse_receipt_id` INT UNSIGNED NULL,
  `warehouse_expense_id` INT UNSIGNED NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  `size_id` INT UNSIGNED NULL DEFAULT NULL,
  `height_id` INT UNSIGNED NULL DEFAULT NULL,
  `amount` INT UNSIGNED NOT NULL,
  `wear_percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 0.00,
  `cost` DECIMAL(10,2) UNSIGNED NULL DEFAULT NULL,
  `owner_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_operation_warehouse_2_idx` (`warehouse_receipt_id` ASC),
  INDEX `fk_operation_warehouse_3_idx` (`warehouse_expense_id` ASC),
  INDEX `fk_operation_warehouse_1_idx` (`nomenclature_id` ASC),
  INDEX `fk_operation_warehouse_4_idx` (`size_id` ASC),
  INDEX `fk_operation_warehouse_5_idx` (`height_id` ASC),
  INDEX `fk_operation_warehouse_6_idx` (`owner_id` ASC),
  INDEX `index_operation_warehouse_time` (`operation_time` ASC),
  INDEX `index_operation_warehouse_wear_percent` (`wear_percent` ASC),
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
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_operation_warehouse_4`
    FOREIGN KEY (`size_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_warehouse_5`
    FOREIGN KEY (`height_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_warehouse_6`
    FOREIGN KEY (`owner_id`)
    REFERENCES `owners` (`id`)
    ON DELETE SET NULL
    ON UPDATE NO ACTION)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_general_ci;


-- -----------------------------------------------------
-- Table `regulations`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `regulations` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(255) NOT NULL,
  `number` VARCHAR(10) NULL DEFAULT NULL,
  `doc_date` DATE NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB
AUTO_INCREMENT = 1000;


-- -----------------------------------------------------
-- Table `regulations_annex`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `regulations_annex` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `regulations_id` INT UNSIGNED NOT NULL,
  `number` TINYINT NOT NULL,
  `name` VARCHAR(255) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_regulations_appendix_1_idx` (`regulations_id` ASC),
  CONSTRAINT `fk_regulations_appendix_1`
    FOREIGN KEY (`regulations_id`)
    REFERENCES `regulations` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 10000;


-- -----------------------------------------------------
-- Table `norms`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `norms` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `regulations_id` INT UNSIGNED NULL DEFAULT NULL,
  `regulations_annex_id` INT UNSIGNED NULL DEFAULT NULL,
  `name` VARCHAR(200) NULL DEFAULT NULL,
  `ton_paragraph` VARCHAR(15) NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  `datefrom` DATETIME NULL DEFAULT NULL,
  `dateto` DATETIME NULL DEFAULT NULL,
  `archival` TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  INDEX `norms_last_update_idx` (`last_update` DESC),	
  INDEX `fk_norms_1_idx` (`regulations_id` ASC),
  INDEX `fk_norms_2_idx` (`regulations_annex_id` ASC),
  CONSTRAINT `fk_norms_1`
    FOREIGN KEY (`regulations_id`)
    REFERENCES `regulations` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_norms_2`
    FOREIGN KEY (`regulations_annex_id`)
    REFERENCES `regulations_annex` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB;

--
-- Table `protection_tools_category_for_analytics`
--
create table if not exists `protection_tools_category_for_analytics` (
	`id`   int(11) unsigned not null auto_increment primary key,
	`name` varchar(100)     not null,
	`comment` text null default null)
ENGINE = InnoDB;

-- -----------------------------------------------------
-- Table `protection_tools`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `protection_tools` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(800) NOT NULL,
  `item_types_id` INT UNSIGNED NOT NULL DEFAULT 1,
  `dermal_ppe` tinyint(1) default 0 not null,
  `dispenser` tinyint(1) default 0 not null,
  `assessed_cost` DECIMAL(10,2) UNSIGNED NULL DEFAULT NULL,
  supply_type enum ('Unisex', 'TwoSex') default 'Unisex' not null,
  supply_uni_id int(10) unsigned null,
  supply_male_id int(10) unsigned null,
  supply_female_id int(10) unsigned null,
  `comments` TEXT NULL DEFAULT NULL,
  `category_for_analytic_id` INT UNSIGNED NULL DEFAULT NULL,
  `archival` bool not null default false,
  PRIMARY KEY (`id`),
  INDEX `fk_protection_tools_1_idx` (`item_types_id` ASC),
  CONSTRAINT `fk_protection_tools_1`
    FOREIGN KEY (`item_types_id`)
    REFERENCES `item_types` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  constraint protection_tools_nomenclature_female_id_fk
	foreign key (supply_female_id) references nomenclature (id)
	on update cascade on delete set null,
  constraint protection_tools_nomenclature_male_id_fk
	foreign key (supply_male_id) references nomenclature (id)
	on update cascade on delete set null,
  constraint protection_tools_nomenclature_uni_id_fk
	foreign key (supply_uni_id) references nomenclature (id)
	on update cascade on delete set null,
  CONSTRAINT `FK_protection_tools_category_for_analytics`
	FOREIGN KEY (`category_for_analytic_id`)
	REFERENCES `protection_tools_category_for_analytics` (`id`)
	ON DELETE SET NULL
	ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `norm_conditions`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `norm_conditions` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(100) NOT NULL,
  `sex` ENUM('ForAll', 'OnlyMen', 'OnlyWomen') NOT NULL DEFAULT 'ForAll',
  `issuance_start` DATETIME NULL DEFAULT NULL,
  `issuance_end` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `norms_item`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `norms_item` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `norm_id` INT UNSIGNED NOT NULL,
  `protection_tools_id` INT UNSIGNED NOT NULL,
  `amount` INT UNSIGNED NOT NULL DEFAULT 1,
  `period_type` ENUM('Year', 'Month', 'Shift', 'Wearout', 'Duty') NOT NULL DEFAULT 'Year',
  `period_count` TINYINT UNSIGNED NOT NULL DEFAULT 1,
  `condition_id` INT UNSIGNED NULL DEFAULT NULL,
  `norm_paragraph` VARCHAR(200) NULL DEFAULT NULL COMMENT 'Пункт норм, основание выдачи',
  `comment` TEXT NULL DEFAULT NULL,
  `is_disabled` BOOLEAN NOT NULL DEFAULT FALSE,
  PRIMARY KEY (`id`),
  INDEX `norms_item_last_update_idx` (`last_update` DESC),
  INDEX `fk_norms_item_1_idx` (`norm_id` ASC),
  INDEX `fk_norms_item_2_idx` (`protection_tools_id` ASC),
  INDEX `fk_norms_item_3_idx` (`condition_id` ASC),
  CONSTRAINT `fk_norms_item_1`
    FOREIGN KEY (`norm_id`)
    REFERENCES `norms` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_norms_item_2`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_norms_item_3`
    FOREIGN KEY (`condition_id`)
    REFERENCES `norm_conditions` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `operation_issued_by_employee`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `operation_issued_by_employee` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `employee_id` INT UNSIGNED NOT NULL,
  `operation_time` DATETIME NOT NULL,
  `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `nomenclature_id` INT UNSIGNED NULL DEFAULT NULL,
  `size_id` INT UNSIGNED NULL DEFAULT NULL,
  `height_id` INT UNSIGNED NULL DEFAULT NULL,
  `wear_percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 1.00,
  `issued` INT NOT NULL DEFAULT 0,
  `returned` INT NOT NULL DEFAULT 0,
  `auto_writeoff` TINYINT(1) NOT NULL DEFAULT 1,
  `auto_writeoff_date` DATE NULL DEFAULT NULL,
  `protection_tools_id` INT UNSIGNED NULL DEFAULT NULL,
  `norm_item_id` INT UNSIGNED NULL DEFAULT NULL,
  `StartOfUse` DATE NULL DEFAULT NULL,
  `ExpiryByNorm` DATE NULL DEFAULT NULL,
  `issued_operation_id` INT UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `sign_key` VARCHAR(16) NULL DEFAULT NULL,
  `sign_timestamp` DATETIME NULL DEFAULT NULL,
  `manual_operation` TINYINT(1) NOT NULL DEFAULT 0,
  `override_before` TINYINT(1) NOT NULL DEFAULT (manual_operation),
  `fixed_operation` TINYINT(1) NOT NULL DEFAULT 0,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `operation_issued_by_employee_last_update_idx` (`last_update` DESC),
  INDEX `fk_operation_issued_by_employee_1_idx` (`employee_id` ASC),
  INDEX `fk_operation_issued_by_employee_2_idx` (`nomenclature_id` ASC),
  INDEX `fk_operation_issued_by_employee_3_idx` (`issued_operation_id` ASC),
  INDEX `operation_issued_by_employee_date` (`operation_time` ASC),
  INDEX `fk_operation_issued_by_employee_5_idx` (`norm_item_id` ASC),
  INDEX `fk_operation_issued_by_employee_4_idx` (`warehouse_operation_id` ASC),
  INDEX `index10` (`wear_percent` ASC),
  INDEX `fk_operation_issued_by_employee_protection_tools_idx` (`protection_tools_id` ASC),
  INDEX `fk_operation_issued_by_employee_7_idx` (`size_id` ASC),
  INDEX `fk_operation_issued_by_employee_8_idx` (`height_id` ASC),
  CONSTRAINT `fk_operation_issued_by_employee_1`
    FOREIGN KEY (`employee_id`)
    REFERENCES `employees` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_2`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_3`
    FOREIGN KEY (`issued_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_4`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_5`
    FOREIGN KEY (`norm_item_id`)
    REFERENCES `norms_item` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_protection_tools`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_7`
    FOREIGN KEY (`size_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_issued_by_employee_8`
    FOREIGN KEY (`height_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_general_ci;

-- -----------------------------------------------------
-- Table `stock_write_off`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_write_off` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `doc_number` VARCHAR(16) NULL DEFAULT NULL,
  `date` DATE NOT NULL,
  `user_id` INT UNSIGNED NULL,
  `organization_id` int unsigned null,
  `director_id` int unsigned null,
  `chairman_id` int unsigned null,
  `comment` TEXT NULL DEFAULT NULL,
  `creation_date` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_write_off_user_idx` (`user_id` ASC),
  INDEX `index_stock_write_off_date` (`date` ASC),
  index `fk_stock_write_off_chairman_id_idx` (`chairman_id` ASC),
  index `fk_stock_write_off_director_idx` (`director_id` ASC),
  index `fk_stock_write_off_organization_idx` (`organization_id` ASC),
  CONSTRAINT `fk_stock_write_off_user`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
	constraint fk_stock_write_off_chairman_id
	foreign key (chairman_id) references leaders (id)
	on update cascade on delete set null, 
	constraint fk_stock_write_off_organization_id
	foreign key (organization_id) references organizations (id)
	on update cascade on delete set null,
	constraint stock_inspection_fk_director_id
	foreign key (director_id) references leaders (id)
	on update cascade on delete set null
    )
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;

-- -----------------------------------------------------
-- Table `stock_write_off_members`
-- -----------------------------------------------------

create table stock_write_off_members(
	id INT UNSIGNED NOT NULL AUTO_INCREMENT,
	write_off_id int unsigned not null,
	member_id int unsigned not null,
	PRIMARY KEY (`id`),
	index stock_write_off_members_fk1_idx (write_off_id ASC),
	index stock_write_off_members_fk2_idx (member_id ASC),
	constraint stock_write_off_members_fk1
		foreign key (write_off_id) references stock_write_off (id)
			on update cascade on delete cascade,
	constraint stock_write_off_members_fk2
		foreign key (member_id) references leaders (id)
			on update cascade
);

-- -----------------------------------------------------
-- Table `stock_expense`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_expense` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `doc_number` VARCHAR(16) NULL DEFAULT NULL,
  `warehouse_id` INT(10) UNSIGNED NOT NULL,
  `employee_id` INT UNSIGNED NULL DEFAULT NULL,
  `date` DATE NOT NULL,
  `issue_date` date NULL DEFAULT `date`,
  `user_id` INT UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  `creation_date` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_expense_employee_idx` (`employee_id` ASC),
  INDEX `fk_stock_expense_user_idx` (`user_id` ASC),
  INDEX `fk_stock_expense_1_idx` (`warehouse_id` ASC),
  INDEX `index_stock_expense_date` (`date` ASC),
  CONSTRAINT `fk_stock_expense_1`
    FOREIGN KEY (`warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_employee`
    FOREIGN KEY (`employee_id`)
    REFERENCES `employees` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_user`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8;


-- -----------------------------------------------------
-- Table `stock_expense_detail`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_expense_detail` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_expense_id` INT UNSIGNED NOT NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  `quantity` INT UNSIGNED NOT NULL,
  `employee_issue_operation_id` INT UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT UNSIGNED NOT NULL,
  `protection_tools_id` INT UNSIGNED NULL DEFAULT NULL,
  `size_id` INT UNSIGNED NULL DEFAULT NULL,
  `height_id` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_expense_detail_stock_expense_idx` (`stock_expense_id` ASC),
  INDEX `fk_stock_expense_detail_nomenclature_idx` (`nomenclature_id` ASC),
  INDEX `fk_stock_expense_detail_1_idx` (`employee_issue_operation_id` ASC),
  INDEX `fk_stock_expense_detail_2_idx` (`warehouse_operation_id` ASC),
  INDEX `fk_stock_expense_detail_4_idx` (`protection_tools_id` ASC),
  INDEX `fk_stock_expense_detail_5_idx` (`size_id` ASC),
  INDEX `fk_stock_expense_detail_6_idx` (`height_id` ASC),
  CONSTRAINT `fk_stock_expense_detail_1`
    FOREIGN KEY (`employee_issue_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_detail_2`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_expense_detail_nomenclature`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_detail_stock_expense`
    FOREIGN KEY (`stock_expense_id`)
    REFERENCES `stock_expense` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_detail_4`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_expense_detail_5`
    FOREIGN KEY (`size_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_expense_detail_6`
    FOREIGN KEY (`height_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_general_ci;


-- -----------------------------------------------------
-- Table `stock_write_off_detail`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_write_off_detail` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_write_off_id` INT UNSIGNED NOT NULL,
  `nomenclature_id` INT UNSIGNED NULL DEFAULT NULL,
  `quantity` INT UNSIGNED NOT NULL,
  `employee_issue_operation_id` INT UNSIGNED NULL DEFAULT NULL,
  `warehouse_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT UNSIGNED NULL DEFAULT NULL,
  `duty_norm_issue_operation_id` INT UNSIGNED NULL DEFAULT NULL,	
  `size_id` INT UNSIGNED NULL DEFAULT NULL,
  `height_id` INT UNSIGNED NULL DEFAULT NULL,
  `cause_write_off_id` INT UNSIGNED NULL DEFAULT NULL,
  `cause` text null, 
  PRIMARY KEY (`id`),
  INDEX `fk_stock_write_off_detail_write_off_idx` (`stock_write_off_id` ASC),
  INDEX `fk_stock_write_off_detail_nomenclature_idx` (`nomenclature_id` ASC),
  INDEX `fk_stock_write_off_detail_1_idx` (`employee_issue_operation_id` ASC),
  INDEX `fk_stock_write_off_detail_2_idx` (`warehouse_operation_id` ASC),
  INDEX `fk_stock_write_off_detail_3_idx` (`warehouse_id` ASC),
  INDEX `fk_stock_write_off_detail_5_idx` (`size_id` ASC),
  INDEX `fk_stock_write_off_detail_6_idx` (`height_id` ASC),
  INDEX `fk_stock_write_off_detail_duty_norm_issue_operation_idx` (`duty_norm_issue_operation_id` ASC),
  CONSTRAINT `fk_stock_write_off_detail_1`
    FOREIGN KEY (`employee_issue_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_write_off_detail_2`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_write_off_detail_3`
    FOREIGN KEY (`warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_write_off_detail_nomenclature`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_write_off_detail_write_off`
    FOREIGN KEY (`stock_write_off_id`)
    REFERENCES `stock_write_off` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_write_off_detail_5`
    FOREIGN KEY (`size_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_write_off_detail_6`
    FOREIGN KEY (`height_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  constraint fk_stock_write_off_detail_cause_write_off 
	foreign key (cause_write_off_id) 
	references causes_write_off (id)
	on update cascade
	on delete set null,
CONSTRAINT fk_stock_write_off_detail_duty_norm_issue_operation
    FOREIGN KEY (`duty_norm_issue_operation_id`)
    REFERENCES `operation_issued_by_duty_norm` (id)
	ON DELETE NO ACTION
	ON UPDATE NO ACTION)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_general_ci;


-- -----------------------------------------------------
-- Table `read_news`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `read_news` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NULL DEFAULT NULL,
  `feed_id` VARCHAR(64) NOT NULL,
  `items` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_read_news_user_id_idx` (`user_id` ASC),
  CONSTRAINT `fk_read_news_user_id`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `norms_posts`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `norms_posts` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `norm_id` INT UNSIGNED NOT NULL,
  `post_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_norms_posts_1_idx` (`norm_id` ASC),
  INDEX `fk_norms_posts_2_idx` (`post_id` ASC),
  CONSTRAINT `fk_norms_posts_1`
    FOREIGN KEY (`norm_id`)
    REFERENCES `norms` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_norms_posts_2`
    FOREIGN KEY (`post_id`)
    REFERENCES `posts` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `employees_norms`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `employees_norms` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `employee_id` INT UNSIGNED NOT NULL,
  `norm_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_employees_norms_1_idx` (`employee_id` ASC),
  INDEX `fk_employees_norms_2_idx` (`norm_id` ASC),
  CONSTRAINT `fk_employees_norms_1`
    FOREIGN KEY (`employee_id`)
    REFERENCES `employees` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_employees_norms_2`
    FOREIGN KEY (`norm_id`)
    REFERENCES `norms` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `employees_cost_allocation`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `employees_cost_allocation` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `employee_id` INT(10) UNSIGNED NOT NULL,
  `cost_center_id` INT(10) UNSIGNED NOT NULL,
  `percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 1.00,
  PRIMARY KEY (`id`),
  INDEX `employees_cost_allocation_ibfk_1` (`cost_center_id` ASC),
  INDEX `employees_cost_allocation_ibfk_2` (`employee_id` ASC),
  CONSTRAINT `employees_cost_allocation_ibfk_1`
    FOREIGN KEY (`cost_center_id`)
    REFERENCES `cost_center` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `employees_cost_allocation_ibfk_2`
    FOREIGN KEY (`employee_id`)
    REFERENCES `employees` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;


-- -----------------------------------------------------
-- Table `employee_cards_item`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `employee_cards_item` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `employee_id` INT UNSIGNED NOT NULL,
  `protection_tools_id` INT UNSIGNED NOT NULL,
  `norm_item_id` INT UNSIGNED NULL,
  `created` DATE NULL,
  `next_issue` DATE NULL,
  `next_issue_annotation` VARCHAR(240) NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_employees_item_1_idx` (`employee_id` ASC),
  INDEX `fk_employee_cards_item_3_idx` (`norm_item_id` ASC),
  INDEX `fk_employee_cards_item_2_idx` (`protection_tools_id` ASC),
  INDEX `index_employee_cards_item_next_issue` (`next_issue` ASC),
  CONSTRAINT `fk_employees_item_1`
    FOREIGN KEY (`employee_id`)
    REFERENCES `employees` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_employee_cards_item_2`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_employee_cards_item_3`
    FOREIGN KEY (`norm_item_id`)
    REFERENCES `norms_item` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `organizations`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `organizations` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(300) NULL,
  `address` VARCHAR(300) NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `user_settings`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `user_settings` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` INT UNSIGNED NOT NULL,
  `toolbar_style` ENUM('Text', 'Icons', 'Both') NOT NULL DEFAULT 'Both',
  `toolbar_icons_size` ENUM('ExtraSmall', 'Small', 'Middle', 'Large') NOT NULL DEFAULT 'Middle',
  `toolbar_show` TINYINT(1) NOT NULL DEFAULT 1,
  `maximize_on_start` TINYINT(1) NOT NULL DEFAULT 1,
  `buyer_email` varchar(320) null,
  `default_added_amount` ENUM('All', 'One', 'Zero') NOT NULL DEFAULT 'All',
  `default_warehouse_id` INT UNSIGNED NULL,
  `default_organization_id` INT UNSIGNED NULL,
  `default_responsible_person_id` INT UNSIGNED NULL,
  `default_leader_id` INT UNSIGNED NULL,
  `default_claim_list_type` ENUM('NotAnswered','NotClosed','All') NOT NULL DEFAULT 'NotClosed',
  PRIMARY KEY (`id`),
  INDEX `fk_user_settings_1_idx` (`user_id` ASC),
  INDEX `fk_user_settings_warehouse_id_idx` (`default_warehouse_id` ASC),
  INDEX `fk_user_settings_organization_id_idx` (`default_organization_id` ASC),
  INDEX `fk_user_settings_leader_id_idx` (`default_leader_id` ASC),
  INDEX `fk_user_settings_responsible_person_id_idx` (`default_responsible_person_id` ASC),
  CONSTRAINT `fk_user_settings_1`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_user_settings_warehouse_id`
    FOREIGN KEY (`default_warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_user_settings_organization_id`
    FOREIGN KEY (`default_organization_id`)
    REFERENCES `organizations` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_user_settings_responsible_person_id`
    FOREIGN KEY (`default_responsible_person_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_user_settings_leader_id`
    FOREIGN KEY (`default_leader_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `vacation_type`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `vacation_type` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(45) NOT NULL,
  `exclude_from_wearing` TINYINT(1) NOT NULL DEFAULT 0,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `employees_vacations`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `employees_vacations` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `employee_id` INT UNSIGNED NOT NULL,
  `vacation_type_id` INT UNSIGNED NOT NULL,
  `begin_date` DATE NOT NULL,
  `end_date` DATE NOT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_employees_vacations_1_idx` (`employee_id` ASC),
  INDEX `fk_employees_vacations_2_idx` (`vacation_type_id` ASC),
  CONSTRAINT `fk_employees_vacations_1`
    FOREIGN KEY (`employee_id`)
    REFERENCES `employees` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_employees_vacations_2`
    FOREIGN KEY (`vacation_type_id`)
    REFERENCES `vacation_type` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB;

-- -----------------------------------------------------
-- Table `causes_write_off`
-- -----------------------------------------------------
create table causes_write_off
(
	id int UNSIGNED auto_increment primary key,
	name varchar(120) not null
);
insert into causes_write_off (name) values ('Увольнение'), ('Преждевременный износ'), ('Изменение должности'), ('Прочее');

-- -----------------------------------------------------
-- Table `stock_collective_expense`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_collective_expense` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `doc_number` VARCHAR(16) NULL DEFAULT NULL,
  `warehouse_id` INT(10) UNSIGNED NOT NULL,
  `date` DATE NOT NULL,
  `user_id` INT UNSIGNED NULL DEFAULT NULL,
  `transfer_agent_id` INT(10) UNSIGNED NULL,
  `issuance_request_id` INT UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  `creation_date` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_expense_user_idx` (`user_id` ASC),
  INDEX `fk_stock_expense_1_idx` (`warehouse_id` ASC),
  INDEX `index_stock_collective_expense_date` (`date` ASC),
  INDEX `fk_transfer_agent_id_idx` (`transfer_agent_id` ASC),
  INDEX `issuance_request_id_idx` (`issuance_request_id` ASC),
  CONSTRAINT `fk_stock_collective_expense_1`
    FOREIGN KEY (`warehouse_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_2`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_3`
    FOREIGN KEY (`transfer_agent_id`)
    REFERENCES `employees` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_request_id` 
  	FOREIGN KEY (`issuance_request_id`)
	REFERENCES issuance_requests (`id`)
	ON DELETE NO ACTION
	ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4;


-- -----------------------------------------------------
-- Table `issuance_sheet`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `issuance_sheet` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `doc_number` VARCHAR(16) NULL DEFAULT NULL,
  `date` DATE NOT NULL,
  `organization_id` INT UNSIGNED NULL DEFAULT NULL,
  `subdivision_id` INT UNSIGNED NULL DEFAULT NULL,
  `responsible_person_id` INT UNSIGNED NULL DEFAULT NULL,
  `head_of_division_person_id` INT UNSIGNED NULL DEFAULT NULL,
  `stock_expense_id` INT UNSIGNED NULL DEFAULT NULL,
  `stock_collective_expense_id` INT UNSIGNED NULL DEFAULT NULL,
  `transfer_agent_id` INT(10) UNSIGNED NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_issuance_sheet_1_idx` (`organization_id` ASC),
  INDEX `fk_issuance_sheet_3_idx` (`responsible_person_id` ASC),
  INDEX `fk_issuance_sheet_4_idx` (`head_of_division_person_id` ASC),
  INDEX `fk_issuance_sheet_5_idx` (`stock_expense_id` ASC),
  INDEX `fk_issuance_sheet_2_idx` (`subdivision_id` ASC),
  INDEX `fk_issuance_sheet_7_idx` (`stock_collective_expense_id` ASC),
  INDEX `index_issuance_sheet_date` (`date` ASC),
  INDEX `fk_issuance_sheet_8_idx` (`transfer_agent_id` ASC),
  CONSTRAINT `fk_issuance_sheet_1`
    FOREIGN KEY (`organization_id`)
    REFERENCES `organizations` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_2`
    FOREIGN KEY (`subdivision_id`)
    REFERENCES `subdivisions` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_3`
    FOREIGN KEY (`responsible_person_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_4`
    FOREIGN KEY (`head_of_division_person_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_5`
    FOREIGN KEY (`stock_expense_id`)
    REFERENCES `stock_expense` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_7`
    FOREIGN KEY (`stock_collective_expense_id`)
    REFERENCES `stock_collective_expense` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_8`
    FOREIGN KEY (`transfer_agent_id`)
    REFERENCES `employees` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_collective_expense_detail`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_collective_expense_detail` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_collective_expense_id` INT UNSIGNED NOT NULL,
  `employee_id` INT UNSIGNED NOT NULL,
  `protection_tools_id` INT UNSIGNED NULL DEFAULT NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  `quantity` INT UNSIGNED NOT NULL,
  `employee_issue_operation_id` INT UNSIGNED NULL DEFAULT NULL,
  `warehouse_operation_id` INT UNSIGNED NOT NULL,
  `size_id` INT UNSIGNED NULL DEFAULT NULL,
  `height_id` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_expense_detail_nomenclature_idx` (`nomenclature_id` ASC),
  INDEX `fk_stock_expense_detail_2_idx` (`warehouse_operation_id` ASC),
  INDEX `fk_stock_expense_detail_4_idx` (`protection_tools_id` ASC),
  INDEX `fk_stock_collective_expense_detail_4_idx` (`stock_collective_expense_id` ASC),
  INDEX `fk_stock_collective_expense_detail_6_idx` (`employee_id` ASC),
  INDEX `fk_stock_collective_expense_detail_7_idx` (`size_id` ASC),
  INDEX `fk_stock_collective_expense_detail_8_idx` (`height_id` ASC),
  INDEX `fk_stock_expense_detail_1_idx` (`employee_issue_operation_id` ASC),
  CONSTRAINT `fk_stock_collective_expense_detail_1`
    FOREIGN KEY (`employee_issue_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_detail_2`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_detail_3`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_detail_4`
    FOREIGN KEY (`stock_collective_expense_id`)
    REFERENCES `stock_collective_expense` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_detail_5`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_collective_expense_detail_6`
    FOREIGN KEY (`employee_id`)
    REFERENCES `employees` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_detail_7`
    FOREIGN KEY (`size_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_collective_expense_detail_8`
    FOREIGN KEY (`height_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_general_ci;


-- -----------------------------------------------------
-- Table `issuance_sheet_items`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `issuance_sheet_items` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `issuance_sheet_id` INT UNSIGNED NOT NULL,
  `employee_id` INT UNSIGNED NOT NULL,
  `nomenclature_id` INT UNSIGNED NULL DEFAULT NULL,
  `protection_tools_id` INT UNSIGNED NULL DEFAULT NULL,
  `stock_expense_detail_id` INT UNSIGNED NULL DEFAULT NULL,
  `stock_collective_expense_item_id` INT UNSIGNED NULL DEFAULT NULL,
  `issued_operation_id` INT UNSIGNED NULL,
  `amount` INT UNSIGNED NOT NULL,
  `start_of_use` DATE NULL DEFAULT NULL,
  `lifetime` DECIMAL(5,2) UNSIGNED NULL DEFAULT NULL,
  `size_id` INT UNSIGNED NULL DEFAULT NULL,
  `height_id` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_issuance_sheet_items_1_idx` (`issuance_sheet_id` ASC),
  INDEX `fk_issuance_sheet_items_2_idx` (`employee_id` ASC),
  INDEX `fk_issuance_sheet_items_3_idx` (`nomenclature_id` ASC),
  INDEX `fk_issuance_sheet_items_4_idx` (`issued_operation_id` ASC),
  INDEX `fk_issuance_sheet_items_5_idx` (`stock_expense_detail_id` ASC),
  INDEX `fk_issuance_sheet_items_6_idx` (`protection_tools_id` ASC),
  INDEX `fk_issuance_sheet_items_7_idx` (`stock_collective_expense_item_id` ASC),
  INDEX `fk_issuance_sheet_items_9_idx` (`height_id` ASC),
  INDEX `fk_issuance_sheet_items_8_idx` (`size_id` ASC),
  CONSTRAINT `fk_issuance_sheet_items_1`
    FOREIGN KEY (`issuance_sheet_id`)
    REFERENCES `issuance_sheet` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_2`
    FOREIGN KEY (`employee_id`)
    REFERENCES `employees` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_3`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_4`
    FOREIGN KEY (`issued_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_5`
    FOREIGN KEY (`stock_expense_detail_id`)
    REFERENCES `stock_expense_detail` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_6`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_7`
    FOREIGN KEY (`stock_collective_expense_item_id`)
    REFERENCES `stock_collective_expense_detail` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_8`
    FOREIGN KEY (`size_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE,
  CONSTRAINT `fk_issuance_sheet_items_9`
    FOREIGN KEY (`height_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4 COLLATE=utf8mb4_general_ci;


-- -----------------------------------------------------
-- Table `stock_transfer`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_transfer` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `doc_number` VARCHAR(16) NULL DEFAULT NULL,
  `organization_id` INT(10) UNSIGNED NULL,
  `warehouse_from_id` INT UNSIGNED NOT NULL,
  `warehouse_to_id` INT UNSIGNED NOT NULL,
  `date` DATETIME NOT NULL,
  `user_id` INT(10) UNSIGNED NULL,
  `comment` TEXT NULL,
  `creation_date` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_transfer_1_idx` (`warehouse_from_id` ASC),
  INDEX `fk_stock_transfer_2_idx` (`warehouse_to_id` ASC),
  INDEX `fk_stock_transfer_3_idx` (`user_id` ASC),
  INDEX `index_stock_transfer_organization` (`organization_id` ASC),
  INDEX `index_stock_transfer_date` (`date` ASC),
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
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_transfer_4`
	FOREIGN KEY (`organization_id`)
	REFERENCES `organizations` (`id`)
	ON DELETE SET NULL
	ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_transfer_detail`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_transfer_detail` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_transfer_id` INT UNSIGNED NOT NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  `quantity` INT UNSIGNED NULL,
  `warehouse_operation_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_transfer_detail_1_idx` (`stock_transfer_id` ASC),
  INDEX `fk_stock_transfer_detail_2_idx` (`nomenclature_id` ASC),
  INDEX `fk_stock_transfer_detail_3_idx` (`warehouse_operation_id` ASC),
  CONSTRAINT `fk_stock_transfer_detail_1`
    FOREIGN KEY (`stock_transfer_id`)
    REFERENCES `stock_transfer` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_detail_2`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_transfer_detail_3`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;

-- -----------------------------------------------------
-- Table `protection_tools_nomenclature`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `protection_tools_nomenclature` (
  `protection_tools_id` INT UNSIGNED NOT NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`protection_tools_id`, `nomenclature_id`),
  INDEX `fk_protection_tools_nomenclature_2_idx` (`nomenclature_id` ASC),
  CONSTRAINT `fk_protection_tools_nomenclature_1`
    FOREIGN KEY (`protection_tools_id`)
    REFERENCES `protection_tools` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_protection_tools_nomenclature_2`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `message_templates`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `message_templates` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `name` VARCHAR(100) NOT NULL,
  `message_title` VARCHAR(200) NOT NULL,
  `message_text` text NOT NULL,
  `link_title` varchar(100) NULL,
  `link` varchar(100) NULL,
  PRIMARY KEY (`id`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_completion`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_completion` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `doc_number` VARCHAR(16) NULL DEFAULT NULL,
  `date` DATE NOT NULL,
  `user_id` INT UNSIGNED NULL DEFAULT NULL,
  `warehouse_receipt_id` INT UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  `warehouse_expense_id` INT UNSIGNED NULL DEFAULT NULL,
  `creation_date` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_completion_user_id_idx` (`user_id` ASC),
  INDEX `fk_stock_completion_warehouse_receipt_idx` (`warehouse_receipt_id` ASC),
  INDEX `fk_stock_completion_warehouse_expense_idx` (`warehouse_expense_id` ASC),
  INDEX `index_stock_completion_date` (`date` ASC),
  CONSTRAINT `fk_stock_completion_user_id`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_completion_warehouse_receipt`
    FOREIGN KEY (`warehouse_receipt_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_stock_completion_warehouse_expense`
    FOREIGN KEY (`warehouse_expense_id`)
    REFERENCES `warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_completion_source_item`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_completion_source_item` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_completion_id` INT UNSIGNED NOT NULL,
  `warehouse_operation_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_completion_id_idx` (`stock_completion_id` ASC),
  INDEX `fk_stock_completion_detail_operation_idx` (`warehouse_operation_id` ASC),
  CONSTRAINT `fk_source_stock_completion_id`
    FOREIGN KEY (`stock_completion_id`)
    REFERENCES `stock_completion` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_source_stock_completion_detail_operation`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_completion_result_item`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_completion_result_item` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_completion_id` INT UNSIGNED NOT NULL,
  `warehouse_operation_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_completion_id_idx` (`stock_completion_id` ASC),
  INDEX `fk_stock_completion_detail_operation_idx` (`warehouse_operation_id` ASC),
  CONSTRAINT `fk_result_stock_completion_id`
    FOREIGN KEY (`stock_completion_id`)
    REFERENCES `stock_completion` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_result_stock_completion_result_operation`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `size_suitable`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `size_suitable` (
  `size_id` INT UNSIGNED NOT NULL,
  `size_suitable_id` INT UNSIGNED NOT NULL,
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
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `employees_sizes`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `employees_sizes` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `employee_id` INT UNSIGNED NOT NULL COMMENT 'Сотрудник для которого установлен размер',
  `size_type_id` INT UNSIGNED NOT NULL COMMENT 'Тип размера, не может быть установлено несколько размеров одного типа одному сотруднику',
  `size_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_employees_sizes_1_idx` (`employee_id` ASC),
  INDEX `fk_employees_sizes_2_idx` (`size_type_id` ASC),
  INDEX `fk_employees_sizes_3_idx` (`size_id` ASC),
  UNIQUE INDEX `employees_sizes_unique` USING BTREE (`employee_id`, `size_type_id`),
  CONSTRAINT `fk_employees_sizes_1`
    FOREIGN KEY (`employee_id`)
    REFERENCES `employees` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_employees_sizes_2`
    FOREIGN KEY (`size_type_id`)
    REFERENCES `size_types` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_employees_sizes_3`
    FOREIGN KEY (`size_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `barcodes`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `barcodes` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `creation_date` DATE NOT NULL DEFAULT (CURRENT_DATE()),
  `last_update` TIMESTAMP on update CURRENT_TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `title` VARCHAR(13) NULL DEFAULT NULL,
  `nomenclature_id` INT UNSIGNED NOT NULL,
  `size_id` INT UNSIGNED NULL DEFAULT NULL,
  `height_id` INT UNSIGNED NULL DEFAULT NULL,
  `comment` text null,
  PRIMARY KEY (`id`),
  INDEX `last_update` (`last_update` ASC),
  UNIQUE INDEX `value_UNIQUE` (`title` ASC),
  INDEX `fk_barcodes_1_idx` (`nomenclature_id` ASC),
  INDEX `fk_barcodes_2_idx` (`size_id` ASC),
  INDEX `fk_barcodes_3_idx` (`height_id` ASC),
  CONSTRAINT `fk_barcodes_1`
    FOREIGN KEY (`nomenclature_id`)
    REFERENCES `nomenclature` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_barcodes_2`
    FOREIGN KEY (`size_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE,
  CONSTRAINT `fk_barcodes_3`
    FOREIGN KEY (`height_id`)
    REFERENCES `sizes` (`id`)
    ON DELETE SET NULL
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `operation_barcodes`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `operation_barcodes` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `barcode_id` INT UNSIGNED NOT NULL,
  `employee_issue_operation_id` INT UNSIGNED NULL,
  `warehouse_operation_id` INT UNSIGNED NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_operation_barcodes_1_idx` (`barcode_id` ASC),
  INDEX `fk_operation_barcodes_2_idx` (`employee_issue_operation_id` ASC),
  INDEX `fk_operation_barcodes_3_idx` (`warehouse_operation_id` ASC),
  UNIQUE INDEX `index_uniq` (`barcode_id` ASC, `employee_issue_operation_id` ASC, `warehouse_operation_id` ASC),
  CONSTRAINT `fk_operation_barcodes_1`
    FOREIGN KEY (`barcode_id`)
    REFERENCES `barcodes` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_barcodes_2`
    FOREIGN KEY (`employee_issue_operation_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `fk_operation_barcodes_3`
    FOREIGN KEY (`warehouse_operation_id`)
    REFERENCES `operation_warehouse` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_inspection`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_inspection` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `doc_number` VARCHAR(16) NULL DEFAULT NULL,
  `date` DATE NOT NULL,
  `creation_date` DATETIME NULL DEFAULT NULL,
  `user_id` INT UNSIGNED NULL DEFAULT NULL,
  `organization_id` INT UNSIGNED NULL DEFAULT NULL,
  `director_id` INT UNSIGNED NULL DEFAULT NULL,
  `chairman_id` INT UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `stock_inspection_fk_1_idx` (`user_id` ASC),
  INDEX `stock_inspection_fk_2_idx` (`director_id` ASC),
  INDEX `stock_inspection_fk_3_idx` (`chairman_id` ASC),
  INDEX `fk_stock_inspection_1_idx` (`organization_id` ASC),
  INDEX `index_stock_inspection_date` (`date` ASC),
  CONSTRAINT `stock_inspection_fk_1`
    FOREIGN KEY (`user_id`)
    REFERENCES `users` (`id`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `stock_inspection_fk_2`
    FOREIGN KEY (`director_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `stock_inspection_fk_3`
    FOREIGN KEY (`chairman_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `fk_stock_inspection_1`
    FOREIGN KEY (`organization_id`)
    REFERENCES `organizations` (`id`)
    ON DELETE NO ACTION
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_inspection_items`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_inspection_items` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_inspection_id` INT UNSIGNED NOT NULL,
  `operation_issue_id` INT UNSIGNED NOT NULL,
  `new_operation_issue_id` INT UNSIGNED NOT NULL,
  `cause` TEXT NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `stock_inspection_detail_fk_1_idx` (`stock_inspection_id` ASC),
  INDEX `stock_inspection_detail_fk_2_idx` (`operation_issue_id` ASC),
  INDEX `stock_inspection_detail_fk_3_idx` (`new_operation_issue_id` ASC),
  CONSTRAINT `stock_inspection_detail_fk_1`
    FOREIGN KEY (`stock_inspection_id`)
    REFERENCES `stock_inspection` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `stock_inspection_detail_fk_2`
    FOREIGN KEY (`operation_issue_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE,
  CONSTRAINT `stock_inspection_detail_fk_3`
    FOREIGN KEY (`new_operation_issue_id`)
    REFERENCES `operation_issued_by_employee` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `stock_inspection_members`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `stock_inspection_members` (
  `id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_inspection_id` INT UNSIGNED NOT NULL,
  `member_id` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `stock_inspection_members_fk1_idx` (`stock_inspection_id` ASC),
  INDEX `stock_inspection_members_fk2_idx` (`member_id` ASC),
  CONSTRAINT `stock_inspection_members_fk1`
    FOREIGN KEY (`stock_inspection_id`)
    REFERENCES `stock_inspection` (`id`)
    ON DELETE CASCADE
    ON UPDATE CASCADE,
  CONSTRAINT `stock_inspection_members_fk2`
    FOREIGN KEY (`member_id`)
    REFERENCES `leaders` (`id`)
    ON DELETE RESTRICT
    ON UPDATE CASCADE)
ENGINE = InnoDB;

-- Table `employee_groups`

create table employee_groups
(
	id int unsigned not null auto_increment,
	name varchar(128) null,
	comment text null,
	PRIMARY KEY (`id`)
);

create index employee_groups_name_index
	on employee_groups (name);

-- Table `employee_group_items`

create table employee_group_items
(
	id int unsigned not null auto_increment,
	employee_group_id int unsigned not null,
	employee_id int unsigned not null,
	comment text null,
	PRIMARY KEY (`id`),
	constraint employee_groups_items_unique
		unique (employee_id, employee_group_id),
	constraint foreign_key_employee_groups_items_employees
		foreign key (employee_id) references employees (id)
			on update cascade on delete cascade,
	constraint foreign_key_employee_groups_items_employee_groups
		foreign key (employee_group_id) references employee_groups (id)
			on update cascade on delete cascade
);

create index employee_groups_items_employee_groups_id_index
	on employee_group_items (employee_group_id);

create index employee_groups_items_employees_id_index
	on employee_group_items (employee_id);

-- -----------------------------------------------------
-- Дежурные нормы
-- -----------------------------------------------------

create table duty_norms
(
	id                   int unsigned auto_increment
		primary key,
	name                 varchar(200) charset utf8mb4 null,
	responsible_leder_id int unsigned 				  null,
	responsible_employee_id int unsigned 			  null,
	subdivision_id 		 int unsigned 				  null,
	datefrom             datetime                     null,
	dateto               datetime                     null,
	comment              text charset utf8mb4         null,
	constraint duty_norms_employees_id_fk
		foreign key (responsible_employee_id) references employees (id)
			on delete set null on update cascade,
	constraint duty_norms_leaders_surname_fk
		foreign key (responsible_leder_id) references leaders (id)
			on delete set null on update cascade,
	constraint duty_norms_subdivisions_id_fk
		foreign key (subdivision_id) references subdivisions (id)
			on delete set null on update cascade
);

-- -----------------------------------------------------
-- строка дежурной нормы
-- -----------------------------------------------------
create table duty_norm_items
(
	id                  int unsigned auto_increment
		primary key,
	duty_norm_id        int unsigned                                              not null,
	protection_tools_id int unsigned                                              not null,
	amount              int unsigned                               default 1      not null,
	period_type         enum ('Year', 'Month', 'Wearout') default 'Year' not null,
	period_count        tinyint unsigned                           default 1      not null,
	next_issue          date                                                      null,
	norm_paragraph      varchar(200)                                              null comment 'Пункт норм, основание выдачи',
	comment             text                                                      null,
	constraint fk_duty_norms_item_norm
		foreign key (duty_norm_id) references duty_norms (id)
			on update cascade,
	constraint fk_duty_norms_item_protection_tools
		foreign key (protection_tools_id) references protection_tools (id)
			on update cascade
);

-- -----------------------------------------------------
-- Операции выдачи по дежурной норме
-- -----------------------------------------------------
create table operation_issued_by_duty_norm
(
	id                     int unsigned auto_increment
		primary key,
	operation_time         datetime                                           not null,
	last_update            timestamp              default current_timestamp() not null on update current_timestamp(),
	nomenclature_id        int unsigned                                       null,
	duty_norm_item_id      int unsigned                                       null,
	duty_norm_id           int unsigned                                       not null,
	size_id                int unsigned                                       null,
	height_id              int unsigned                                       null,
	wear_percent           decimal(3, 2) unsigned default 1.00                not null,
	issued                 int                    default 0                   not null,
	returned               int                    default 0                   not null,
	auto_writeoff          tinyint(1)             default 1                   not null,
	auto_writeoff_date     date                                               null,
	protection_tools_id    int unsigned                                       null,
	start_of_use           date                                               null,
	expiry_by_norm         date                                               null,
	issued_operation_id    int unsigned                                       null,
	warehouse_operation_id int unsigned                                       null,
	override_before        tinyint(1)             default 0                   not null,
	comment                text                                               null,
	constraint fk_operation_issued_by_duty_norm_height
		foreign key (height_id) references sizes (id)
			on update cascade,
	constraint fk_operation_issued_by_duty_norm_issued_operation
		foreign key (issued_operation_id) references operation_issued_by_duty_norm (id)
			on update cascade on delete cascade,
	constraint fk_operation_issued_by_duty_norm_nomenclature
		foreign key (nomenclature_id) references nomenclature (id)
			on update cascade,
	constraint fk_operation_issued_by_duty_norm_norm
		foreign key (duty_norm_id) references duty_norms (id)
			on update cascade on delete cascade,
	constraint fk_operation_issued_by_duty_norm_operation_warehouse
		foreign key (warehouse_operation_id) references operation_warehouse (id)
			on update cascade,
	constraint fk_operation_issued_by_duty_norm_protection_tools
		foreign key (protection_tools_id) references protection_tools (id)
			on update cascade on delete set null,
	constraint fk_operation_issued_by_duty_norm_size
		foreign key (size_id) references sizes (id)
			on update cascade,
	constraint fk_operation_issued_by_employee_duty_norm_item
		foreign key (duty_norm_item_id) references duty_norm_items (id)
			on update cascade on delete set null
);

create index operation_issued_by_duty_norm_last_update_idx
	on operation_issued_by_duty_norm (last_update);

create index operation_issued_by_duty_norm_operation_time_idx
	on operation_issued_by_duty_norm (operation_time);

create index operation_issued_by_duty_norm_wear_percent_idx
	on operation_issued_by_duty_norm (wear_percent);

-- -----------------------------------------------------
-- Выдача по дежурной норме
-- -----------------------------------------------------
create table stock_expense_duty_norm
(
	id                      int unsigned auto_increment
		primary key,
	doc_number              varchar(16)  null,
	creation_date           datetime     not null  default (CURRENT_DATE()),
	date                    date         not null,
	duty_norm_id            int unsigned null,
	warehouse_id            int unsigned not null,
	responsible_employee_id int unsigned null,
	user_id                 int unsigned null,
	comment                 text         null,
	constraint fk_stock_expense_duty_norm_norm
		foreign key (duty_norm_id) references duty_norms (id)
			on update cascade,
	constraint fk_stock_expense_duty_norm_responsible_employee
		foreign key (responsible_employee_id) references employees (id)
			on update cascade,
	constraint fk_stock_expense_duty_norm_user
		foreign key (user_id) references users (id)
			on update cascade on delete set null,
	constraint fk_stock_expense_duty_norm_warehouse
		foreign key (warehouse_id) references warehouse (id)
			on update cascade
)
	charset = utf8mb4;

create index fk_stock_expense_duty_norm_employee_idx
	on stock_expense_duty_norm (responsible_employee_id);

create index fk_stock_expense_duty_norm_user_idx
	on stock_expense_duty_norm (user_id);

create index fk_stock_expense_duty_norm_warehouse_idx
	on stock_expense_duty_norm (warehouse_id);

create index stock_expense_duty_norm_expense_date_idx
	on stock_expense_duty_norm (date);

-- -----------------------------------------------------
-- Строка выдачи по дежурной норме
-- -----------------------------------------------------
create table stock_expense_duty_norm_items
(
	id                               int unsigned auto_increment
		primary key,
	stock_expense_duty_norm_id       int unsigned not null,
	operation_issued_by_duty_norm_id int unsigned null,
	warehouse_operation_id           int unsigned not null,
	constraint fk_stock_expense_duty_norm_items_operation_issued_by_duty_norm
		foreign key (operation_issued_by_duty_norm_id) references operation_issued_by_duty_norm (id)
			on update cascade,
	constraint fk_stock_expense_duty_norm_items_operation_warehouse
		foreign key (warehouse_operation_id) references operation_warehouse (id),
	constraint fk_stock_expense_duty_norm_items_stock_expense_duty_norm
		foreign key (stock_expense_duty_norm_id) references stock_expense_duty_norm (id)
			on update cascade on delete cascade
);

create index fk_stock_expense_duty_norm_items_operation_idx
	on stock_expense_duty_norm_items (operation_issued_by_duty_norm_id);
create index fk_stock_expense_duty_norm_items_warehouse_operation_idx
	on stock_expense_duty_norm_items (warehouse_operation_id);
create index fk_stock_expense_duty_norm_items_stock_expense_duty_norm_idx
	on stock_expense_duty_norm_items (stock_expense_duty_norm_id);

-- -----------------------------------------------------
-- Table `shipment`
-- -----------------------------------------------------
create table shipment
(
	id int unsigned auto_increment primary key,
	start_period date null,
	end_period date null,
	status enum ('Draft', 'New', 'Present', 'Accepted', 'Ordered', 'Received') default 'Draft' not null,
	full_ordered boolean default false not null,
	full_received boolean default false not null,
	has_receive boolean default false not null,
	submitted datetime null,
	user_id int unsigned null,
	comment text null,
	creation_date datetime null
);
create index index_shipment_start_period
	on shipment (start_period);
create index index_shipment_end_period
	on shipment (end_period);

-- -----------------------------------------------------
-- Table `shipment_items`
-- -----------------------------------------------------
create table shipment_items
(
	id int unsigned auto_increment primary key,
	shipment_id int unsigned not null,
	nomenclature_id int unsigned not null,
	quantity int unsigned not null,
	ordered int unsigned not null,
	received int unsigned not null,
	cost decimal(10, 2) unsigned default 0.00 not null,
	size_id int unsigned null,
	height_id int unsigned null,
	diff_cause varchar(120) null,
	comment varchar(120) null,
	constraint fk_shipment_items_doc
		foreign key (shipment_id) references shipment (id)
			on update cascade on delete cascade,
	constraint fk_shipment_items_nomenclature
		foreign key (nomenclature_id) references nomenclature (id)
			on update cascade,
	constraint fk_shipment_items_size_id
		foreign key (size_id) references sizes (id)
			on update cascade,
	constraint fk_shipment_items_height_id
		foreign key (height_id) references sizes (id)
			on update cascade
);
create index index_shipment_items_doc
	on shipment_items (shipment_id);

create index index_shipment_items_height
	on shipment_items (height_id);

create index index_shipment_items_nomenclature
	on shipment_items (nomenclature_id);

create index index_shipment_items_size
	on shipment_items (size_id);

-- -----------------------------------------------------
-- Записи  на посещение
-- -----------------------------------------------------
create table visits
(
	id              int unsigned auto_increment,
	create_date     datetime              not null,
	visit_date      datetime              not null,
	employee_id     int unsigned          not null,
	employee_create boolean default TRUE  not null,
	done            boolean default FALSE not null,
	cancelled       boolean default FALSE not null,
	comment         text                  null,
	constraint visits_pk
		primary key (id),
	constraint visits_employees_id_fk
		foreign key (employee_id) references employees (id)
			on update cascade on delete cascade
);

create index visits_create_date_index
	on visits (create_date);
create index visits_visit_date_index
	on visits (visit_date);


create table visits_documents
(
	id         int unsigned auto_increment
		primary key,
	visit_id   int unsigned not null,
	expence_id int unsigned null,
	writeof_id int unsigned null,
	return_id  int unsigned null,
	constraint visits_documents_stock_expense_id_fk
		foreign key (expence_id) references stock_expense (id)
			on update cascade on delete cascade,
	constraint visits_documents_stock_return_id_fk
		foreign key (return_id) references stock_return (id)
			on update cascade on delete cascade,
	constraint visits_documents_stock_write_off_organization_id_fk
		foreign key (writeof_id) references stock_write_off (id)
			on update cascade on delete cascade,
	constraint visits_documents_visits_id_fk
		foreign key (visit_id) references visits (id)
			on update cascade on delete cascade
);

-- -----------------------------------------------------
-- Учёт дней недели
-- -----------------------------------------------------
create table work_days
(
	id          int unsigned auto_increment,
	date 		date 	not null,
	is_work_day boolean default true not null,
	comment 	text	null,
		constraint work_days_pk
		primary key (id)
);

-- -----------------------------------------------------
-- Оказываемые услуги
-- -----------------------------------------------------
create table clothing_service_services
(
	id   	int unsigned auto_increment,
	name 	varchar(60)       not null,
	cost 	decimal default 0 not null,
	code    varchar(13)       null,
	comment text       		  null,
	constraint clothing_service_services_pk
		primary key (id)
)
	auto_increment = 101;

create table clothing_service_services_nomenclature
(
	id   			int unsigned auto_increment,
	nomenclature_id int unsigned not null,
	service_id     	int unsigned not null,
	constraint clothing_service_services_pk
		primary key (id),
	constraint fk_services_nomenclature_nomenclature_id
		foreign key (nomenclature_id) references nomenclature (id)
			on update cascade on delete cascade,
	constraint fk_services_nomenclature_service_id
		foreign key (service_id) references clothing_service_services (id)
			on update cascade on delete cascade
);

create table clothing_service_services_claim
(
	id         int unsigned auto_increment,
	service_id int unsigned null,
	claim_id   int unsigned,
	constraint clothing_service_services_claim_pk
		primary key (id),
	constraint clothing_service_services_claim_service_id_claim_id_uindex
		unique (service_id, claim_id),
	constraint clothing_service_services_claim_clothing_service_claim_id_fk
		foreign key (claim_id) references clothing_service_claim (id)
			on update cascade on delete cascade,
	constraint clothing_service_services_claim_clothing_service_services_id_fk
		foreign key (service_id) references clothing_service_services (id)
			on update cascade on delete cascade
);
	
-- -----------------------------------------------------
-- Заявка на выдачу
-- -----------------------------------------------------

-- Заявка на выдачу
CREATE TABLE issuance_requests(
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`receipt_date` DATE NOT NULL,
	`status` ENUM('New', 'Issued', 'PartiallyIssued') DEFAULT 'New' NOT NULL,
	`comment` TEXT NULL,
	`user_id` INT UNSIGNED NULL,
	`creation_date` DATETIME NULL DEFAULT NULL,
	PRIMARY KEY (`id`),
	CONSTRAINT `fk_issuance_request_user_id` FOREIGN KEY (`user_id`) REFERENCES users (`id`)
	    ON DELETE NO ACTION
		ON UPDATE CASCADE,
	INDEX `issuance_request_user_id_idx` (`user_id` ASC)
);

-- Сотрудники в заявках на выдачу
CREATE TABLE employees_issuance_request(
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`employee_id` INT UNSIGNED NOT NULL,
	`issuance_request_id` INT UNSIGNED NOT NULL,
	PRIMARY KEY (`id`),
	CONSTRAINT `fk_employee_id` FOREIGN KEY (`employee_id`) REFERENCES employees (`id`)
		ON DELETE CASCADE
		ON UPDATE CASCADE,
	CONSTRAINT `fk_issuance_request_id` FOREIGN KEY  (`issuance_request_id`) REFERENCES issuance_requests (`id`)
		ON DELETE CASCADE
		ON UPDATE CASCADE,
	INDEX `employee_id_idx` (`employee_id` ASC),
	INDEX `issuance_request_id_idx` (`issuance_request_id` ASC)
);
-- -----------------------------------------------------
-- Добавление внешних ключей для документа выдачи по дежурной норме в ведомость
-- -----------------------------------------------------
ALTER TABLE issuance_sheet
	ADD stock_expense_duty_norm_id int unsigned null after stock_expense_id;
ALTER TABLE issuance_sheet
	ADD CONSTRAINT fk_stock_expense_duty_norm_id
		FOREIGN KEY (stock_expense_duty_norm_id) REFERENCES stock_expense_duty_norm (id)
			ON UPDATE CASCADE ON DELETE NO ACTION;

ALTER TABLE issuance_sheet_items
	ADD stock_expense_duty_norm_item_id integer unsigned null after stock_expense_detail_id;
ALTER TABLE issuance_sheet_items
	ADD CONSTRAINT fk_stock_expense_duty_norm_item_id
		FOREIGN KEY (stock_expense_duty_norm_item_id) REFERENCES stock_expense_duty_norm_items (id)
			ON UPDATE CASCADE ON DELETE CASCADE ;

-- -----------------------------------------------------
-- function count_issue
-- -----------------------------------------------------

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

SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;

-- -----------------------------------------------------
-- Data for table `base_parameters`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `base_parameters` (`name`, `str_value`) VALUES ('product_name', 'workwear');
INSERT INTO `base_parameters` (`name`, `str_value`) VALUES ('version', '2.10.2');

COMMIT;


-- -----------------------------------------------------
-- Data for table `warehouse`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `warehouse` (`id`, `name`) VALUES (DEFAULT, 'Основной склад');

COMMIT;


-- -----------------------------------------------------
-- Data for table `measurement_units`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `measurement_units` (`id`, `name`, `digits`, `okei`) VALUES (DEFAULT, 'шт.', 0, '796');
INSERT INTO `measurement_units` (`id`, `name`, `digits`, `okei`) VALUES (DEFAULT, 'пара', 0, '715');
INSERT INTO `measurement_units` (`id`, `name`, `digits`, `okei`) VALUES (DEFAULT, 'компл.', 0, '839');
INSERT INTO `measurement_units` (`id`, `name`, `digits`, `okei`) VALUES (DEFAULT, 'набор', 0, '704');

COMMIT;


-- -----------------------------------------------------
-- Data for table `size_types`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (1, 'Рост', 1, 'Height', 1);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (2, 'Размер одежды', 1, 'Size', 2);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (4, 'Размер обуви', 1, 'Size', 4);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (5, 'Размер зимней обуви', 1, 'Size', 5);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (6, 'Размер головного убора', 1, 'Size', 6);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (7, 'Размер перчаток', 1, 'Size', 8);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (8, 'Размер рукавиц', 1, 'Size', 9);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (3, 'Размер зимней одежды', 1, 'Size', 3);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (9, 'Размер зимнего головного убора', 1, 'Size', 7);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (10, 'Размер противогаза', 1, 'Size', 10);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (11, 'Размер респиратора', 1, 'Size', 11);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES (12, 'Размер носков', 1, 'Size', 12);
INSERT INTO `size_types` (`id`, `name`, `use_in_employee`, `category`, `position`) VALUES(13,'Размер футболки',1,'size',13);

	COMMIT;


-- -----------------------------------------------------
-- Data for table `item_types`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'Одежда', 'Wear', 'Personal', 1, NULL, 2, 1);
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'Обувь', 'Shoes', 'Personal', 2, NULL, 4, NULL);
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'Зимняя обувь', 'WinterShoes', 'Personal', 2, NULL, 5, NULL);
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'Головные уборы', 'Headgear', 'Personal', 1, NULL, 6, NULL);
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'Перчатки', 'Gloves', 'Personal', 2, NULL, 7, NULL);
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'Варежки', 'Mittens', 'Personal', 2, NULL, 8, NULL);
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'СИЗ', 'PPE', 'Personal', 1, NULL, NULL, NULL);
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'Зимняя одежда', 'Wear', 'Personal', 1, NULL, 2, 1);
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'Противогазы', 'PPE', 'Personal', 1, NULL, 10, NULL);
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'Респираторы', 'PPE', 'Personal', 1, NULL, 11, NULL);
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'Носки','Wear', 'Personal', 2, NULL, 12, NULL);
INSERT INTO `item_types` (`id`, `name`, `wear_category`, `issue_type`, `units_id`, `comment`, `size_type_id`, `height_type_id`) VALUES (DEFAULT, 'Футболки', 'Wear', 'Personal', 1, NULL, 13, NULL);

COMMIT;


-- -----------------------------------------------------
-- Data for table `sizes`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (1, '146', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (2, '146-152', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (3, '152', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (4, '155-166', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (5, '158', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (6, '158-164', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (7, '164', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (8, '167-178', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (9, '170', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (10, '170-176', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (11, '176', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (12, '179-190', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (13, '182', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (14, '182-188', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (15, '188', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (16, '191-200', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (17, '194', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (18, '194-200', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (19, '200', 1, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (20, '201-210', 1, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (30, '38', 2, 1, 1, '76');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (31, '40', 2, 1, 1, '80');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (32, '40-42', 2, 0, 1, '80-84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (33, '42', 2, 1, 1, '84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (34, '44', 2, 1, 1, '88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (35, '44-46', 2, 0, 1, '88-92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (36, '46', 2, 1, 1, '92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (37, '48', 2, 1, 1, '96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (38, '48-50', 2, 0, 1, '96-100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (39, '50', 2, 1, 1, '100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (40, '50-52', 2, 0, 1, '100-104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (41, '52', 2, 1, 1, '104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (42, '52-54', 2, 0, 1, '104-108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (43, '54', 2, 1, 1, '108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (44, '56', 2, 1, 1, '112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (45, '56-58', 2, 0, 1, '112-116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (46, '58', 2, 1, 1, '116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (47, '58-60', 2, 0, 1, '116-120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (48, '60', 2, 1, 1, '120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (49, '60-62', 2, 0, 1, '120-124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (50, '62', 2, 1, 1, '124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (51, '62-64', 2, 0, 1, '124-128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (52, '64', 2, 1, 1, '128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (53, '64-66', 2, 0, 1, '128-132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (54, '66', 2, 1, 1, '132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (55, '68', 2, 1, 1, '136');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (56, '68-70', 2, 0, 1, '136-140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (57, '70', 2, 1, 1, '140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (58, '72', 2, 1, 1, '144');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (59, '72-74', 2, 0, 1, '144-148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (60, '74', 2, 1, 1, '148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (61, '74-76', 2, 0, 1, '148-152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (62, '76', 2, 1, 1, '152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (63, '76-78', 2, 0, 1, '152-156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (64, '78', 2, 1, 1, '156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (65, '80', 2, 1, 1, '160');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (66, '80-82', 2, 0, 1, '160-164');
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
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (90, '38', 3, 1, 1, '76');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (91, '40', 3, 1, 1, '80');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (92, '40-42', 3, 0, 1, '80-84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (93, '42', 3, 1, 1, '84');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (94, '44', 3, 1, 1, '88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (95, '44-46', 3, 0, 1, '88-92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (96, '46', 3, 1, 1, '92');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (97, '48', 3, 1, 1, '96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (98, '48-50', 3, 0, 1, '96-100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (99, '50', 3, 1, 1, '100');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (100, '50-52', 3, 0, 1, '100-104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (101, '52', 3, 1, 1, '104');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (102, '52-54', 3, 0, 1, '104-108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (103, '54', 3, 1, 1, '108');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (104, '56', 3, 1, 1, '112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (105, '56-58', 3, 0, 1, '112-116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (106, '58', 3, 1, 1, '116');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (107, '58-60', 3, 0, 1, '116-120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (108, '60', 3, 1, 1, '120');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (109, '60-62', 3, 0, 1, '120-124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (110, '62', 3, 1, 1, '124');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (111, '62-64', 3, 0, 1, '124-128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (112, '64', 3, 1, 1, '128');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (113, '64-66', 3, 0, 1, '128-132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (114, '66', 3, 1, 1, '132');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (115, '68', 3, 1, 1, '136');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (116, '68-70', 3, 0, 1, '136-140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (117, '70', 3, 1, 1, '140');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (118, '72', 3, 1, 1, '144');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (119, '72-74', 3, 0, 1, '144-148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (120, '74', 3, 1, 1, '148');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (121, '74-76', 3, 0, 1, '148-152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (122, '76', 3, 1, 1, '152');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (123, '76-78', 3, 0, 1, '152-156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (124, '78', 3, 1, 1, '156');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (125, '80', 3, 1, 1, '160');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (126, '80-82', 3, 0, 1, '160-164');
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
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (150, '34', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (151, '34-35', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (152, '35', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (153, '36', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (154, '36-37', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (155, '37', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (156, '38', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (157, '38-39', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (158, '39', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (159, '40', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (160, '40-41', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (161, '41', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (162, '42', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (163, '42-43', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (164, '43', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (165, '44', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (166, '44-45', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (167, '45', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (168, '46', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (169, '46-47', 4, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (170, '47', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (171, '48', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (172, '49', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (173, '50', 4, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (190, '34', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (191, '34-35', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (192, '35', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (193, '36', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (194, '36-37', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (195, '37', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (196, '38', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (197, '38-39', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (198, '39', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (199, '40', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (200, '40-41', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (201, '41', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (202, '42', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (203, '42-43', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (204, '43', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (205, '44', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (206, '44-45', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (207, '45', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (208, '46', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (209, '46-47', 5, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (210, '47', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (211, '48', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (212, '49', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (213, '50', 5, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (230, '54', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (231, '55', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (232, '56', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (233, '57', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (234, '58', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (235, '58-60', 6, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (236, '59', 6, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (237, '59-60', 6, 0, 1, NULL);
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
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (276, '8,5-9', 7, 0, 1, NULL);
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
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (315, '58-60', 9, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (316, '59', 9, 1, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (317, '59-60', 9, 0, 1, NULL);
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
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (351, '23-25', 12, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (352, '25', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (353, '27', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (354, '27-29', 12, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (355, '29', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (356, '31', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (357, '31-33', 12, 0, 1, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (358, '33', 12, 1, 0, NULL);
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (78, '42-44', 2, 0, 1, '84-88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (79, '46-48', 2, 0, 1, '92-96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (80, '54-56', 2, 0, 1, '108-112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (138, '42-44', 3, 0, 1, '84-88');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (139, '46-48', 3, 0, 1, '92-96');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (140, '54-56', 3, 0, 1, '108-112');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`) VALUES (370, '3XS', 13,1,1,'40');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (371, '2XS', 13,1,1,'42');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (372, 'XS', 13,1,1,'44');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (373, 'S', 13,1,1,'46');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (374, 'M', 13,1,1,'48');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (375, 'L', 13,1,1,'50');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (376, 'XL', 13,1,1,'52');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (377, '2XL', 13,1,1,'54');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (378, '3XL', 13,1,1,'56');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (379, '4XL', 13,1,1,'58');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (380, '5XL', 13,1,1,'60');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (381, '6XL', 13,1,1,'62');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (382, '7XL', 13,1,1,'64');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (383, '8XL', 13,1,1,'66');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (384, '9XL', 13,1,1,'68');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (385, '10XL', 13,1,1,'70');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (386, '11XL', 13,1,1,'72');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (387, '12XL', 13,1,1,'74');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (388, '13XL', 13,1,1,'76');
INSERT INTO `sizes` (`id`, `name`, `size_type_id`, `use_in_employee`, `use_in_nomenclature`, `alternative_name`)  VALUES (389, '14XL', 13,1,1,'78');

COMMIT;


-- -----------------------------------------------------
-- Data for table `organizations`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `organizations` (`id`, `name`, `address`) VALUES (DEFAULT, 'Моя организация', NULL);

COMMIT;


-- -----------------------------------------------------
-- Data for table `vacation_type`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `vacation_type` (`id`, `name`, `exclude_from_wearing`, `comment`) VALUES (1, 'Основной', 0, NULL);

COMMIT;


-- -----------------------------------------------------
-- Data for table `size_suitable`
-- -----------------------------------------------------
START TRANSACTION;
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (2, 1);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (2, 3);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (4, 5);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (6, 7);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (8, 9);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (8, 10);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (12, 13);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (16, 17);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (18, 19);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (30, 68);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (31, 69);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (32, 31);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (32, 33);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (33, 70);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (34, 71);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (35, 34);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (35, 36);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (36, 71);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (37, 72);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (38, 37);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (38, 39);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (39, 72);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (40, 39);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (40, 41);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (41, 73);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (42, 41);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (42, 43);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (43, 74);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (44, 74);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (45, 44);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (45, 46);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (46, 75);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (47, 46);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (47, 48);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (48, 76);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (49, 48);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (49, 50);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (50, 76);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (51, 50);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (51, 52);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (52, 76);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (53, 52);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (53, 54);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (54, 77);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (55, 77);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (56, 55);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (56, 57);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (57, 77);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (59, 58);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (59, 60);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (61, 60);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (61, 62);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (63, 62);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (63, 64);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (66, 65);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (66, 67);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (90, 128);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (91, 129);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (92, 91);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (92, 93);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (93, 130);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (94, 131);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (95, 94);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (95, 96);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (96, 131);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (97, 132);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (98, 97);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (98, 99);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (99, 132);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (100, 99);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (100, 101);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (101, 133);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (102, 101);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (102, 103);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (103, 134);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (104, 134);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (105, 104);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (105, 106);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (106, 135);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (107, 106);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (107, 108);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (108, 136);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (109, 108);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (109, 110);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (110, 136);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (111, 110);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (111, 112);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (112, 136);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (113, 112);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (113, 114);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (114, 137);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (115, 137);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (116, 115);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (116, 117);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (117, 137);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (119, 118);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (119, 120);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (121, 120);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (121, 122);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (123, 122);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (123, 124);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (126, 125);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (126, 127);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (151, 150);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (151, 152);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (154, 153);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (154, 155);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (157, 156);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (157, 158);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (160, 159);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (160, 161);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (163, 162);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (163, 164);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (166, 165);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (166, 167);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (169, 168);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (169, 170);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (191, 190);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (191, 192);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (194, 193);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (194, 195);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (197, 196);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (197, 198);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (200, 199);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (200, 201);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (203, 202);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (203, 204);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (206, 205);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (206, 207);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (209, 208);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (209, 210);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (235, 234);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (235, 236);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (235, 238);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (237, 236);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (237, 238);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (276, 275);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (276, 277);
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
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (78, 33);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (78, 34);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (79, 36);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (79, 37);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (80, 43);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (80, 44);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (138, 93);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (138, 94);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (139, 96);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (139, 97);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (140, 103);
INSERT INTO `size_suitable` (`size_id`, `size_suitable_id`) VALUES (140, 104);

COMMIT;


