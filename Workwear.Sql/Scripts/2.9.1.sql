--
-- Добавляем Дежурные нормы
--

CREATE TABLE `duty_norms`
(
	`id`                   INT UNSIGNED AUTO_INCREMENT
        PRIMARY KEY,
	`name`                 		VARCHAR(200) CHARSET utf8mb4 NULL,
	`responsible_leder_id` 		INT UNSIGNED 				 NULL,
	`responsible_employee_id` 	INT UNSIGNED 			     NULL,
	`subdivision_id` 		 	INT UNSIGNED 				 NULL, 
	`datefrom`             		DATETIME                     NULL,
	`dateto`               		DATETIME                     NULL,
	`comment`              		TEXT CHARSET utf8mb4         NULL,
	CONSTRAINT `duty_norms_employees_id_fk`
		FOREIGN KEY (`responsible_employee_id`) REFERENCES `employees` (`id`)
			ON DELETE SET NULL ON UPDATE CASCADE,
	CONSTRAINT `duty_norms_leaders_surname_fk`
		FOREIGN KEY (`responsible_leder_id`) REFERENCES `leaders` (`id`)
			ON DELETE SET NULL ON UPDATE CASCADE,
	CONSTRAINT `duty_norms_subdivisions_id_fk`
		FOREIGN KEY (`subdivision_id`) REFERENCES `subdivisions` (`id`)
			ON DELETE SET NULL ON UPDATE CASCADE
);

CREATE TABLE `duty_norm_items`
(
	`id`                  INT UNSIGNED AUTO_INCREMENT
        PRIMARY KEY,
	`duty_norm_id`        INT UNSIGNED                                              NOT NULL,
	`protection_tools_id` INT UNSIGNED                                              NOT NULL,
	`amount`              INT UNSIGNED                               DEFAULT 1      NOT NULL,
	`period_type`         ENUM ('Year', 'Month', 'Wearout') DEFAULT 'Year' NOT NULL,
	`period_count`        TINYINT UNSIGNED                           DEFAULT 1      NOT NULL,
	`next_issue`          DATE                                                      NULL,
	`norm_paragraph`      VARCHAR(200)                                              NULL COMMENT 'Пункт норм, основание выдачи',
	`comment`             TEXT                                                      NULL,
	CONSTRAINT `fk_duty_norms_item_norm`
		FOREIGN KEY (`duty_norm_id`) REFERENCES `duty_norms` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_duty_norms_item_protection_tools`
		FOREIGN KEY (`protection_tools_id`) REFERENCES `protection_tools` (`id`)
			ON UPDATE CASCADE
);

CREATE TABLE `operation_issued_by_duty_norm`
(
	`id`                     INT UNSIGNED AUTO_INCREMENT
        PRIMARY KEY,
	`operation_time`         DATETIME                                           NOT NULL,
	`last_update`            TIMESTAMP              DEFAULT CURRENT_TIMESTAMP() NOT NULL ON UPDATE CURRENT_TIMESTAMP(),
	`nomenclature_id`        INT UNSIGNED                                       NULL,
	`duty_norm_item_id`      INT UNSIGNED                                       NULL,
	`duty_norm_id`           INT UNSIGNED                                       NOT NULL,
	`size_id`                INT UNSIGNED                                       NULL,
	`height_id`              INT UNSIGNED                                       NULL,
	`wear_percent`           DECIMAL(3, 2) UNSIGNED DEFAULT 1.00                NOT NULL,
	`issued`                 INT                    DEFAULT 0                   NOT NULL,
	`returned`               INT                    DEFAULT 0                   NOT NULL,
	`auto_writeoff`          TINYINT(1)             DEFAULT 1                   NOT NULL,
	`auto_writeoff_date`     DATE                                               NULL,
	`protection_tools_id`    INT UNSIGNED                                       NULL,
	`start_of_use`           DATE                                               NULL,
	`expiry_by_norm`         DATE                                               NULL,
	`issued_operation_id`    INT UNSIGNED                                       NULL,
	`warehouse_operation_id` INT UNSIGNED                                       NULL,
	`override_before`        TINYINT(1)             DEFAULT 0                   NOT NULL,
	`comment`                TEXT                                               NULL,
	CONSTRAINT `fk_operation_issued_by_duty_norm_height`
		FOREIGN KEY (`height_id`) REFERENCES `sizes` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_operation_issued_by_duty_norm_issued_operation`
		FOREIGN KEY (`issued_operation_id`) REFERENCES `operation_issued_by_duty_norm` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `fk_operation_issued_by_duty_norm_nomenclature`
		FOREIGN KEY (`nomenclature_id`) REFERENCES `nomenclature` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_operation_issued_by_duty_norm_norm`
		FOREIGN KEY (`duty_norm_id`) REFERENCES `duty_norms` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE,
	CONSTRAINT `fk_operation_issued_by_duty_norm_operation_warehouse`
		FOREIGN KEY (`warehouse_operation_id`) REFERENCES `operation_warehouse` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_operation_issued_by_duty_norm_protection_tools`
		FOREIGN KEY (`protection_tools_id`) REFERENCES `protection_tools` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `fk_operation_issued_by_duty_norm_size`
		FOREIGN KEY (`size_id`) REFERENCES `sizes` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_operation_issued_by_employee_duty_norm_item`
		FOREIGN KEY (`duty_norm_item_id`) REFERENCES `duty_norm_items` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL
);

CREATE INDEX `operation_issued_by_duty_norm_last_update_idx`
	ON `operation_issued_by_duty_norm` (`last_update`);

CREATE INDEX `operation_issued_by_duty_norm_operation_time_idx`
	ON `operation_issued_by_duty_norm` (`operation_time`);

CREATE INDEX `operation_issued_by_duty_norm_wear_percent_idx`
	ON `operation_issued_by_duty_norm` (`wear_percent`);

CREATE TABLE `stock_expense_duty_norm`
(
	`id`                      INT UNSIGNED AUTO_INCREMENT
        PRIMARY KEY,
	`doc_number`              VARCHAR(16)  NULL,
	`creation_date`           DATETIME     NOT NULL  DEFAULT (CURRENT_DATE()),
	`date`                    DATE         NOT NULL,
	`duty_norm_id`            INT UNSIGNED NULL,
	`warehouse_id`            INT UNSIGNED NOT NULL,
	`responsible_employee_id` INT UNSIGNED NULL,
	`user_id`                 INT UNSIGNED NULL,
	`comment`                 TEXT         NULL,
	CONSTRAINT `fk_stock_expense_duty_norm_norm`
		FOREIGN KEY (`duty_norm_id`) REFERENCES `duty_norms` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_stock_expense_duty_norm_responsible_employee`
		FOREIGN KEY (`responsible_employee_id`) REFERENCES `employees` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_stock_expense_duty_norm_user`
		FOREIGN KEY (`user_id`) REFERENCES `users` (`id`)
			ON UPDATE CASCADE ON DELETE SET NULL,
	CONSTRAINT `fk_stock_expense_duty_norm_warehouse`
		FOREIGN KEY (`warehouse_id`) REFERENCES `warehouse` (`id`)
			ON UPDATE CASCADE
)
	CHARSET = utf8mb4;

CREATE INDEX `fk_stock_expense_duty_norm_employee_idx`
	ON `stock_expense_duty_norm` (`responsible_employee_id`);

CREATE INDEX `fk_stock_expense_duty_norm_user_idx`
	ON `stock_expense_duty_norm` (`user_id`);

CREATE INDEX `fk_stock_expense_duty_norm_warehouse_idx`
	ON `stock_expense_duty_norm` (`warehouse_id`);

CREATE INDEX `stock_expense_duty_norm_expense_date_idx`
	ON `stock_expense_duty_norm` (`date`);

CREATE TABLE `stock_expense_duty_norm_items`
(
	`id`                               INT UNSIGNED AUTO_INCREMENT
		PRIMARY KEY,
	`stock_expense_duty_norm_id`       INT UNSIGNED NOT NULL,
	`operation_issued_by_duty_norm_id` INT UNSIGNED NULL,
	`warehouse_operation_id`           INT UNSIGNED NOT NULL,
	CONSTRAINT `fk_stock_expense_duty_norm_items_operation_issued_by_duty_norm`
		FOREIGN KEY (`operation_issued_by_duty_norm_id`) REFERENCES `operation_issued_by_duty_norm` (`id`)
			ON UPDATE CASCADE,
	CONSTRAINT `fk_stock_expense_duty_norm_items_operation_warehouse`
		FOREIGN KEY (`warehouse_operation_id`) REFERENCES `operation_warehouse` (`id`),
	CONSTRAINT `fk_stock_expense_duty_norm_items_stock_expense_duty_norm`
		FOREIGN KEY (`stock_expense_duty_norm_id`) REFERENCES `stock_expense_duty_norm` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE
);

CREATE INDEX `fk_stock_expense_duty_norm_items_operation_idx`
	ON `stock_expense_duty_norm_items` (`operation_issued_by_duty_norm_id`);
CREATE INDEX `fk_stock_expense_duty_norm_items_warehouse_operation_idx`
	ON `stock_expense_duty_norm_items` (`warehouse_operation_id`);
CREATE INDEX `fk_stock_expense_duty_norm_items_stock_expense_duty_norm_idx`
	ON `stock_expense_duty_norm_items` (`stock_expense_duty_norm_id`);
