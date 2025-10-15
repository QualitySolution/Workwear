-- Удаляем механизм выдачи со списанием
ALTER TABLE `stock_expense` DROP FOREIGN KEY `fk_stock_expense_2`;
ALTER TABLE `stock_expense` DROP `write_off_doc`;

ALTER TABLE `operation_issued_by_employee` DROP FOREIGN KEY `fk_operation_issued_by_employee_6`;
ALTER TABLE `operation_issued_by_employee` DROP `operation_write_off_id`;

ALTER TABLE `stock_write_off_detail` DROP `akt_number`;

-- Удаляем номер бухгалтерского документа
ALTER TABLE `operation_issued_by_employee` DROP `buh_document`;

-- Удаляем аналоги из номенклатуры нормы
DROP TABLE `protection_tools_replacement`;

-- Удаляем выдачу на подразделения
-- stock_income
DELETE FROM `stock_income` WHERE `operation` = 'Object';

ALTER TABLE `stock_income`
	MODIFY `operation` ENUM ('Enter', 'Return') NOT NULL;

ALTER TABLE `stock_income`
DROP FOREIGN KEY `fk_stock_income_object`;

ALTER TABLE `stock_income`
DROP COLUMN `object_id`;

ALTER TABLE `stock_income_detail`
DROP FOREIGN KEY `fk_stock_income_detail_3`;

ALTER TABLE `stock_income_detail`
DROP COLUMN `subdivision_issue_operation_id`;

-- stock_expense
DELETE FROM `stock_expense` WHERE `operation` = 'Object';

ALTER TABLE `stock_expense`
DROP FOREIGN KEY `fk_stock_expense_object_id`;

ALTER TABLE `stock_expense`
DROP COLUMN `operation`,
DROP COLUMN `object_id`;
     
ALTER TABLE `stock_expense_detail`
DROP FOREIGN KEY `fk_stock_expense_detail_3`,
DROP FOREIGN KEY `fk_stock_expense_detail_placement`;

ALTER TABLE `stock_expense_detail`
DROP COLUMN `object_place_id`,
DROP COLUMN `subdivision_issue_operation_id`;
     
-- stock_write_off
DELETE FROM `stock_write_off_detail` WHERE `subdivision_issue_operation_id` IS NOT NULL;    
     
ALTER TABLE `stock_write_off_detail`
DROP FOREIGN KEY `fk_stock_write_off_detail_4`;

ALTER TABLE `stock_write_off_detail`
DROP COLUMN `subdivision_issue_operation_id`;
     
-- operation_issued_in_subdivision
DROP TABLE `operation_issued_in_subdivision`;

-- object_places
DROP TABLE `object_places`;

-- удаляем тип номенклатур имущества

ALTER TABLE `item_types` 
	DROP COLUMN `category`,
	DROP COLUMN `norm_life`;

-- переименование таблиц: objects в subdivisions, wear_cards в employees и таблички связи с wear_cards , 
-- norms_professions в norms_posts, переименование ключей, индексов
-- вставка данных из старых таблиц в новые

--  norms_posts
CREATE TABLE `norms_posts`
(
	`id`      INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`norm_id` INT UNSIGNED NOT NULL,
	`post_id` INT UNSIGNED NOT NULL,
	CONSTRAINT `fk_norms_posts_1`
		FOREIGN KEY (`norm_id`) REFERENCES `norms` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `fk_norms_posts_2`
		FOREIGN KEY (`post_id`) REFERENCES `posts` (`id`)
			ON DELETE RESTRICT
			ON UPDATE CASCADE
);
INSERT INTO `norms_posts`
SELECT * FROM `norms_professions`;

CREATE INDEX `fk_norms_posts_1_idx`
	ON `norms_posts` (`norm_id`);

CREATE INDEX `fk_norms_posts_2_idx`
	ON `norms_posts` (`post_id`);

-- subdivisions
CREATE TABLE `subdivisions`
(
	`id`                    INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`code`                  VARCHAR(20)  NULL DEFAULT NULL,
	`address`               TEXT         NULL DEFAULT NULL,
	`name`                  VARCHAR(240) NOT NULL,
	`warehouse_id`          INT UNSIGNED NULL DEFAULT NULL,
	`parent_subdivision_id` INT UNSIGNED NULL DEFAULT NULL,
	CONSTRAINT `fk_subdivisions_1`
		FOREIGN KEY (`warehouse_id`) REFERENCES `warehouse` (`id`)
			ON DELETE NO ACTION
			ON UPDATE NO ACTION 
)
	COLLATE = utf8mb4_general_ci;

INSERT INTO `subdivisions`
SELECT * FROM `objects`;

ALTER TABLE `subdivisions`
	ADD CONSTRAINT `fk_subdivisions_2`
		FOREIGN KEY (`parent_subdivision_id`) REFERENCES `subdivisions` (`id`)
			ON DELETE SET NULL
			ON UPDATE NO ACTION;

CREATE INDEX `fk_subdivisions_1_idx`
	ON `subdivisions` (`warehouse_id`);

CREATE INDEX `fk_subdivisions_2_idx`
	ON `subdivisions` (`parent_subdivision_id`);

CREATE INDEX `index_subdivisions_code`
	ON `subdivisions` (`code`);

-- employees
CREATE TABLE `employees`
(
	`id`                      INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`last_update`             TIMESTAMP  DEFAULT CURRENT_TIMESTAMP() NOT NULL ON UPDATE CURRENT_TIMESTAMP(),
	`card_number`             VARCHAR(15)                            NULL DEFAULT NULL,
	`personnel_number`        VARCHAR(15)                            NULL DEFAULT NULL,
	`last_name`               VARCHAR(20)                            NULL,
	`first_name`              VARCHAR(20)                            NULL,
	`patronymic_name`         VARCHAR(20)                            NULL,
	`card_key`                VARCHAR(16)                            NULL DEFAULT NULL,
	`subdivision_id`          INT UNSIGNED                           NULL DEFAULT NULL,
	`department_id`           INT UNSIGNED                           NULL DEFAULT NULL,
	`hire_date`               DATE                                   NULL DEFAULT NULL,
	`change_of_position_date` DATE                                   NULL DEFAULT NULL,
	`dismiss_date`            DATE                                   NULL DEFAULT NULL,
	`post_id`                 INT UNSIGNED                           NULL DEFAULT NULL,
	`leader_id`               INT UNSIGNED                           NULL DEFAULT NULL,
	`sex`                     ENUM ('F', 'M')                        NULL DEFAULT NULL,
	`birth_date`              DATE                                   NULL DEFAULT NULL,
	`user_id`                 INT UNSIGNED                           NULL DEFAULT NULL,
	`phone_number`            VARCHAR(16)                            NULL DEFAULT NULL,
	`lk_registered`           TINYINT(1) DEFAULT 0                   NOT NULL,
	`email`                   TEXT                                   NULL DEFAULT NULL,
	`photo`                   MEDIUMBLOB                             NULL DEFAULT NULL,
	`comment`                 TEXT                                   NULL DEFAULT NULL,
	CONSTRAINT `card_key_UNIQUE`
		UNIQUE (`card_key`),
	CONSTRAINT `card_number_UNIQUE`
		UNIQUE (`card_number`),
	CONSTRAINT `fk_employees_department`
		FOREIGN KEY (`department_id`) REFERENCES `departments` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `fk_employees_leader`
		FOREIGN KEY (`leader_id`) REFERENCES `leaders` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `fk_employees_post`
		FOREIGN KEY (`post_id`) REFERENCES `posts` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `fk_employees_subdivision`
		FOREIGN KEY (`subdivision_id`) REFERENCES `subdivisions` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `fk_employees_user`
		FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL
);

INSERT INTO `employees`
SELECT * FROM `wear_cards`;

CREATE INDEX `fk_employees_department_idx`
	ON `employees` (`department_id`);

CREATE INDEX `fk_employees_leader_idx`
	ON `employees` (`leader_id`);

CREATE INDEX `fk_employees_post_idx`
	ON `employees` (`post_id`);

CREATE INDEX `fk_employees_subdivision_idx`
	ON `employees` (`subdivision_id`);

CREATE INDEX `fk_employees_user_idx`
	ON `employees` (`user_id`);

CREATE INDEX `index_employees_dismiss_date`
	ON `employees` (`dismiss_date`);

CREATE INDEX `index_employees_first_name`
	ON `employees` (`first_name`);

CREATE INDEX `index_employees_last_name`
	ON `employees` (`last_name`);

CREATE INDEX `index_employees_patronymic_name`
	ON `employees` (`patronymic_name`);

CREATE INDEX `index_employees_personal_number`
	ON `employees` (`personnel_number`);

CREATE INDEX `index_employees_phone_number`
	ON `employees` (`phone_number`);

CREATE INDEX `last_update`
	ON `employees` (`last_update`);

-- employee_cards_item
CREATE TABLE `employee_cards_item`
(
	`id`                    INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`employee_id`           INT UNSIGNED NOT NULL,
	`protection_tools_id`   INT UNSIGNED NOT NULL,
	`norm_item_id`          INT UNSIGNED NULL,
	`created`               DATE         NULL,
	`next_issue`            DATE         NULL,
	`next_issue_annotation` VARCHAR(240) NULL DEFAULT NULL,
	CONSTRAINT `fk_employee_cards_item_2`
		FOREIGN KEY (`protection_tools_id`) REFERENCES `protection_tools` (`id`)
			ON DELETE RESTRICT
			ON UPDATE CASCADE,
	CONSTRAINT `fk_employee_cards_item_3`
		FOREIGN KEY (`norm_item_id`) REFERENCES `norms_item` (`id`)
			ON DELETE RESTRICT
			ON UPDATE CASCADE,
	CONSTRAINT `fk_employees_item_1`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON DELETE RESTRICT
			ON UPDATE CASCADE
);

INSERT INTO `employee_cards_item`
SELECT * FROM `wear_cards_item`;

CREATE INDEX `fk_employee_cards_item_2_idx`
	ON `employee_cards_item` (`protection_tools_id`);

CREATE INDEX `fk_employee_cards_item_3_idx`
	ON `employee_cards_item` (`norm_item_id`);

CREATE INDEX `fk_employees_item_1_idx`
	ON `employee_cards_item` (`employee_id`);

CREATE INDEX `index_employee_cards_item_next_issue`
	ON `employee_cards_item` (`next_issue`);

-- employees_cost_allocation
CREATE TABLE `employees_cost_allocation`
(
	`id`             INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`employee_id`    INT UNSIGNED                        NOT NULL,
	`cost_center_id` INT UNSIGNED                        NOT NULL,
	`percent`        DECIMAL(3, 2) UNSIGNED DEFAULT 1.00 NOT NULL,
	CONSTRAINT `employees_cost_allocation_ibfk_1`
		FOREIGN KEY (`cost_center_id`) REFERENCES `cost_center` (`id`)
			ON DELETE RESTRICT
			ON UPDATE CASCADE,
	CONSTRAINT `employees_cost_allocation_ibfk_2`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE
);

INSERT INTO `employees_cost_allocation`
SELECT * FROM `wear_cards_cost_allocation`;

-- employees_norms
CREATE TABLE `employees_norms`
(
	`id`          INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`employee_id` INT UNSIGNED NOT NULL,
	`norm_id`     INT UNSIGNED NOT NULL,
	CONSTRAINT `fk_employees_norms_1`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `fk_employees_norms_2`
		FOREIGN KEY (`norm_id`) REFERENCES `norms` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE
);

INSERT INTO `employees_norms`
SELECT * FROM `wear_cards_norms`;

CREATE INDEX `fk_employees_norms_1_idx`
	ON `employees_norms` (`employee_id`);

CREATE INDEX `fk_employees_norms_2_idx`
	ON `employees_norms` (`norm_id`);

-- employees_sizes
CREATE TABLE `employees_sizes`
(
	`id`           INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`employee_id`  INT UNSIGNED NOT NULL COMMENT 'Сотрудник для которого установлен размер',
	`size_type_id` INT UNSIGNED NOT NULL COMMENT 'Тип размера, не может быть установлено несколько размеров одного типа одному сотруднику',
	`size_id`      INT UNSIGNED NOT NULL,
	CONSTRAINT `employees_sizes_unique`
		UNIQUE USING BTREE(`employee_id`, `size_type_id`),
	CONSTRAINT `fk_employees_sizes_1`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `fk_employees_sizes_2`
		FOREIGN KEY (`size_type_id`) REFERENCES `size_types` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `fk_employees_sizes_3`
		FOREIGN KEY (`size_id`) REFERENCES `sizes` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE
);

INSERT INTO `employees_sizes`
SELECT * FROM `wear_cards_sizes`;

CREATE INDEX `fk_employees_sizes_1_idx`
	ON `employees_sizes` (`employee_id`);

CREATE INDEX `fk_employees_sizes_2_idx`
	ON `employees_sizes` (`size_type_id`);

CREATE INDEX `fk_employees_sizes_3_idx`
	ON `employees_sizes` (`size_id`);

-- employees_vacations
CREATE TABLE `employees_vacations`
(
	`id`               INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`employee_id`      INT UNSIGNED NOT NULL,
	`vacation_type_id` INT UNSIGNED NOT NULL,
	`begin_date`       DATE         NOT NULL,
	`end_date`         DATE         NOT NULL,
	`comment`          TEXT         NULL DEFAULT NULL,
	CONSTRAINT `fk_employees_vacations_1`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `fk_employees_vacations_2`
		FOREIGN KEY (`vacation_type_id`) REFERENCES `vacation_type` (`id`)
			ON DELETE RESTRICT
			ON UPDATE CASCADE
);

INSERT INTO `employees_vacations`
SELECT * FROM `wear_cards_vacations`;

CREATE INDEX `fk_employees_vacations_1_idx`
	ON `employees_vacations` (`employee_id`);

CREATE INDEX `fk_employees_vacations_2_idx`
	ON `employees_vacations` (`vacation_type_id`);

-- Другие таблицы

ALTER TABLE `stock_expense`
	CHANGE COLUMN `wear_card_id` `employee_id` INT UNSIGNED NULL;

CREATE INDEX `fk_stock_expense_employee_idx`
	ON `stock_expense` (`employee_id`);
ALTER TABLE `stock_income`
	CHANGE COLUMN `wear_card_id` `employee_id` INT UNSIGNED NULL;

CREATE INDEX `fk_stock_income_employee_idx`
	ON `stock_income` (`employee_id`);

ALTER TABLE `employee_group_items`
	ADD CONSTRAINT `employee_groups_items_unique`
		UNIQUE (`employee_id`, `employee_group_id`);

ALTER TABLE `clothing_service_claim`
	DROP FOREIGN KEY `fk_clothing_service_claim_employee_id`;

ALTER TABLE `departments`
	DROP FOREIGN KEY `fk_departaments_1`;

# В ScriptsConfiguration реализовано удаление
# DROP FOREIGN KEY foreign_key_employee_groups_items_employees;
# Приводим к ожидаемому названию
ALTER TABLE `employee_group_items`
	ADD CONSTRAINT `employee_groups_items_employee_fk`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE `employee_group_items`
	ADD CONSTRAINT `employee_groups_items_employee_groups_fk`
		FOREIGN KEY (`employee_group_id`) REFERENCES `employee_groups` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE `issuance_sheet`
	DROP FOREIGN KEY `fk_issuance_sheet_2`;

ALTER TABLE `issuance_sheet`
	DROP FOREIGN KEY `fk_issuance_sheet_8`;

ALTER TABLE `issuance_sheet_items`
	DROP FOREIGN KEY `fk_issuance_sheet_items_2`;

ALTER TABLE `leaders`
	DROP FOREIGN KEY `fk_leaders_1`;

ALTER TABLE `operation_issued_by_employee`
	DROP FOREIGN KEY `fk_operation_issued_by_employee_1`;

ALTER TABLE `postomat_document_items`
	DROP FOREIGN KEY `fk_postomat_document_items_employee_id`;

ALTER TABLE `postomat_document_withdraw_items`
	DROP FOREIGN KEY `fk_postomat_document_withdraw_items_employee_id`;

ALTER TABLE `posts`
	DROP FOREIGN KEY `fk_posts_subdivision`;

ALTER TABLE `stock_collective_expense`
	DROP FOREIGN KEY `fk_stock_collective_expense_3`;

ALTER TABLE `stock_collective_expense_detail`
	DROP FOREIGN KEY `fk_stock_collective_expense_detail_6`;

ALTER TABLE `stock_expense`
	DROP FOREIGN KEY `fk_stock_expense_wear_card`;

ALTER TABLE `stock_income`
	DROP FOREIGN KEY `fk_stock_income_wear_card`;

ALTER TABLE `departments`
	ADD CONSTRAINT `fk_departaments_1`
		FOREIGN KEY (`subdivision_id`) REFERENCES `subdivisions` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL;

ALTER TABLE `clothing_service_claim`
	ADD CONSTRAINT `fk_clothing_service_claim_employee_id`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`);

ALTER TABLE `employee_group_items`
	ADD CONSTRAINT `foreign_key_employee_groups_items_employees`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE;

ALTER TABLE `issuance_sheet`
	ADD CONSTRAINT `fk_issuance_sheet_2`
		FOREIGN KEY (`subdivision_id`) REFERENCES `subdivisions` (`id`)
			ON DELETE NO ACTION
			ON UPDATE CASCADE;

ALTER TABLE `issuance_sheet`
	ADD CONSTRAINT `fk_issuance_sheet_8`
		FOREIGN KEY (`transfer_agent_id`) REFERENCES `employees` (`id`)
			ON DELETE NO ACTION
			ON UPDATE CASCADE;

ALTER TABLE `issuance_sheet_items`
	ADD CONSTRAINT `fk_issuance_sheet_items_2`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON DELETE NO ACTION
			ON UPDATE CASCADE;

ALTER TABLE `leaders`
	ADD CONSTRAINT `fk_leaders_1`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON DELETE NO ACTION
			ON UPDATE CASCADE;

ALTER TABLE `operation_issued_by_employee`
	ADD CONSTRAINT `fk_operation_issued_by_employee_1`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON DELETE RESTRICT
			ON UPDATE CASCADE;

ALTER TABLE `postomat_document_items`
	ADD CONSTRAINT `fk_postomat_document_items_employee_id`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`);

ALTER TABLE `postomat_document_withdraw_items`
	ADD CONSTRAINT `fk_postomat_document_withdraw_items_employee_id`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`);

ALTER TABLE `posts`
	ADD CONSTRAINT `fk_posts_subdivision`
		FOREIGN KEY (`subdivision_id`) REFERENCES `subdivisions` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL;

ALTER TABLE `stock_collective_expense`
	ADD CONSTRAINT `fk_stock_collective_expense_3`
		FOREIGN KEY (`transfer_agent_id`) REFERENCES `employees` (`id`)
			ON DELETE RESTRICT
			ON UPDATE CASCADE;

ALTER TABLE `stock_collective_expense_detail`
	ADD CONSTRAINT `fk_stock_collective_expense_detail_6`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON DELETE NO ACTION
			ON UPDATE CASCADE;

ALTER TABLE `stock_expense`
	ADD CONSTRAINT `fk_stock_expense_employee`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON DELETE RESTRICT
			ON UPDATE CASCADE;

ALTER TABLE `stock_income`
	ADD CONSTRAINT `fk_stock_income_employee`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON DELETE RESTRICT
			ON UPDATE CASCADE;

-- Удаляем

ALTER TABLE `employee_group_items`
	DROP KEY `wear_card_groups_items_unique`;

DROP INDEX `fk_stock_expense_wear_card_idx` ON `stock_expense`;

DROP INDEX `fk_stock_income_wear_card_idx` ON `stock_income`;

DROP TABLE `norms_professions`;

DROP TABLE `wear_cards_cost_allocation`;

DROP TABLE `wear_cards_item`;

DROP TABLE `wear_cards_norms`;

DROP TABLE `wear_cards_sizes`;

DROP TABLE `wear_cards_vacations`;

DROP TABLE `wear_cards`;

DROP TABLE `objects`;

-- Разделяем документ поступления и возврата

-- Документ возврата
CREATE TABLE `stock_return`
(
	`id`            INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`doc_number`    VARCHAR(16)  NULL,
	`date`          DATE         NOT NULL,
	`warehouse_id`  INT UNSIGNED NOT NULL,
	`employee_id`   INT UNSIGNED NOT NULL,
	`user_id`       INT UNSIGNED NULL,
	`comment`       TEXT         NULL,
	`creation_date` DATETIME     NULL,
	CONSTRAINT `stock_return_employees_id_fk`
		FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `stock_return_users_id_fk`
		FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `stock_return_warehouse_id_fk`
		FOREIGN KEY (`warehouse_id`) REFERENCES `warehouse` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE INDEX `stock_return_date_index`
	ON `stock_return` (`date`);

CREATE INDEX `stock_return_doc_number_index`
	ON `stock_return` (`doc_number`);

CREATE INDEX `stock_return_employee_id_index`
	ON `stock_return` (`employee_id`);

CREATE INDEX `stock_income_warehouse_id_index`
	ON `stock_return` (`warehouse_id`);

CREATE TABLE `stock_return_items`
(
	`id`                          INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`stock_return_id`             INT UNSIGNED NOT NULL,
	`nomenclature_id`             INT UNSIGNED NOT NULL,
	`quantity`                    INT UNSIGNED NOT NULL,
	`employee_issue_operation_id` INT UNSIGNED NOT NULL,
	`warehouse_operation_id`      INT UNSIGNED NOT NULL,
	`size_id`                     INT UNSIGNED NULL,
	`height_id`                   INT UNSIGNED NULL,
	`comment_return`              VARCHAR(120) NULL,
	CONSTRAINT `fk_stock_return_items_doc`
		FOREIGN KEY (`stock_return_id`) REFERENCES `stock_return` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `fk_stock_return_items_height`
		FOREIGN KEY (`height_id`) REFERENCES `sizes` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_stock_return_items_nomenclature`
		FOREIGN KEY (`nomenclature_id`) REFERENCES `nomenclature` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_stock_return_items_operation_issue`
		FOREIGN KEY (`employee_issue_operation_id`) REFERENCES `operation_issued_by_employee` (`id`),
	CONSTRAINT `fk_stock_return_items_operation_warehouse`
		FOREIGN KEY (`warehouse_operation_id`) REFERENCES `operation_warehouse` (`id`),
	CONSTRAINT `fk_stock_return_items_size`
		FOREIGN KEY (`size_id`) REFERENCES `sizes` (`id`)
			ON UPDATE CASCADE
);

CREATE INDEX `index_stock_return_items_doc`
	ON `stock_return_items` (`stock_return_id`);

CREATE INDEX `index_stock_return_items_height`
	ON `stock_return_items` (`height_id`);

CREATE INDEX `index_stock_return_items_nomenclature`
	ON `stock_return_items` (`nomenclature_id`);

CREATE INDEX `index_stock_return_items_size`
	ON `stock_return_items` (`size_id`);

CREATE INDEX `index_stock_return_items_warehouse_operation`
	ON `stock_return_items` (`warehouse_operation_id`);

-- Перенос данных
INSERT INTO `stock_return` (`id`, `doc_number`, `date`, `warehouse_id`, `employee_id`, `user_id`, `comment`, `creation_date`)
SELECT `id`, `doc_number`, `date`, `warehouse_id`, `employee_id`, `user_id`, `comment`, `creation_date`
FROM `stock_income`
WHERE `operation` = 'Return';

INSERT INTO `stock_return_items` (`stock_return_id`, `nomenclature_id`, `quantity`, `employee_issue_operation_id`, `warehouse_operation_id`, `size_id`, `height_id`, `comment_return`)
SELECT
	`stock_income_detail`.`stock_income_id` AS `stock_return_id`,
	`stock_income_detail`.`nomenclature_id`,
	`stock_income_detail`.`quantity`,
	`stock_income_detail`.`employee_issue_operation_id`,
	`stock_income_detail`.`warehouse_operation_id`,
	`stock_income_detail`.`size_id`,
	`stock_income_detail`.`height_id`,
	`stock_income_detail`.`comment_return`
FROM `stock_income_detail`
		 LEFT JOIN `stock_income` ON `stock_income_detail`.`stock_income_id` = `stock_income`.`id`
WHERE `stock_income`.`operation` = 'Return';

DELETE FROM `stock_income_detail` WHERE `stock_income_detail`.`stock_income_id` IN
									  (SELECT `id` FROM `stock_income` WHERE `operation` = 'Return');

DELETE FROM `stock_income` WHERE `stock_income`.`operation` = 'Return';

-- Переименование stock_income_detail
CREATE TABLE `stock_income_items`
(
	`id`                     INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`stock_income_id`        INT UNSIGNED                         NOT NULL,
	`nomenclature_id`        INT UNSIGNED                         NOT NULL,
	`quantity`               INT UNSIGNED                         NOT NULL,
	`cost`                   DECIMAL(10, 2) UNSIGNED DEFAULT 0.00 NOT NULL,
	`certificate`            VARCHAR(40)                          NULL,
	`warehouse_operation_id` INT UNSIGNED                         NOT NULL,
	`size_id`                INT UNSIGNED                         NULL,
	`height_id`              INT UNSIGNED                         NULL,
	`comment`                VARCHAR(120)                         NULL,
	CONSTRAINT `fk_stock_income_items_doc`
		FOREIGN KEY (`stock_income_id`) REFERENCES `stock_income` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `fk_stock_income_items_height_id`
		FOREIGN KEY (`height_id`) REFERENCES `sizes` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_stock_income_items_nomenclature`
		FOREIGN KEY (`nomenclature_id`) REFERENCES `nomenclature` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_stock_income_items_operation_warehouse`
		FOREIGN KEY (`warehouse_operation_id`) REFERENCES `operation_warehouse` (`id`),
	CONSTRAINT `fk_stock_income_items_size`
		FOREIGN KEY (`size_id`) REFERENCES `sizes` (`id`)
			ON UPDATE CASCADE
);

CREATE INDEX `index_stock_income_items_doc`
	ON `stock_income_items` (`stock_income_id`);

CREATE INDEX `index_stock_income_items_height`
	ON `stock_income_items` (`height_id`);

CREATE INDEX `index_stock_income_items_nomenclature`
	ON `stock_income_items` (`nomenclature_id`);

CREATE INDEX `index_stock_income_items_size`
	ON `stock_income_items` (`size_id`);

CREATE INDEX `index_stock_income_items_warehouse_operation`
	ON `stock_income_items` (`warehouse_operation_id`);

-- Перенос данных
INSERT INTO `stock_income_items` (`stock_income_id`, `nomenclature_id`, `quantity`, `cost`, `certificate`, `warehouse_operation_id`, `size_id`, `height_id`) 
SELECT
	`stock_income_detail`.`stock_income_id`,
	`stock_income_detail`.`nomenclature_id`,
	`stock_income_detail`.`quantity`,
	`stock_income_detail`.`cost`,
	`stock_income_detail`.`certificate`,
	`stock_income_detail`.`warehouse_operation_id`,
	`stock_income_detail`.`size_id`,
	`stock_income_detail`.`height_id`
FROM `stock_income_detail`
		 LEFT JOIN `stock_income` ON `stock_income_detail`.`stock_income_id` = `stock_income`.`id`
WHERE `stock_income`.`operation` = 'Enter';

-- Переименования
CREATE INDEX `stock_income_warehouse_id_index`
	ON `stock_income` (`warehouse_id`);
# В ScriptsConfiguration реализовано удаление
#DROP INDEX fk_stock_income_1_idx ON stock_income;
#ALTER TABLE stock_income DROP FOREIGN KEY fk_stock_income_1;

ALTER TABLE `stock_income`
	ADD CONSTRAINT `fk_stock_income_warehouse`
		FOREIGN KEY (`warehouse_id`) REFERENCES `warehouse` (`id`)
			ON UPDATE CASCADE;

CREATE INDEX `stock_income_doc_number_index`
	ON `stock_income` (`doc_number`);

-- Удаления
ALTER TABLE `stock_income`
    DROP COLUMN `operation`;

ALTER TABLE `stock_income`
	DROP FOREIGN KEY `fk_stock_income_employee`;
ALTER TABLE `stock_income`
	DROP COLUMN `employee_id`;

DROP TABLE `stock_income_detail`;
