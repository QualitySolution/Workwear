-- Добавляем МВЗ в сотрудника

CREATE TABLE IF NOT EXISTS `wear_cards_cost_allocation` (
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`wear_card_id` INT UNSIGNED NOT NULL,
	`cost_center_id` INT UNSIGNED NOT NULL,
	`percent` DECIMAL(3,2) UNSIGNED NOT NULL DEFAULT 1.00,
	PRIMARY KEY (`id`),
	INDEX `wear_cards_cost_allocation_fk1_idx` (`cost_center_id` ASC),
	INDEX `wear_cards_cost_allocation_fk2_idx` (`wear_card_id` ASC),
	CONSTRAINT `wear_cards_cost_allocation_ibfk_1`
		FOREIGN KEY (`cost_center_id`)
		REFERENCES `cost_center` (`id`)
		ON DELETE RESTRICT
		ON UPDATE CASCADE,
	CONSTRAINT `wear_cards_cost_allocation_ibfk_2`
		FOREIGN KEY (`wear_card_id`)
		REFERENCES `wear_cards` (`id`)
		ON DELETE CASCADE
		ON UPDATE CASCADE)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

-- Приводим к единому соответствию схемы базы данных
-- Так как исправлял COLLATION уже после выхода в релиз миграции на 2.8
ALTER TABLE `issuance_sheet_items` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
ALTER TABLE `operation_issued_by_employee` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
ALTER TABLE `operation_issued_in_subdivision` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
ALTER TABLE `operation_warehouse` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
ALTER TABLE `sizes` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
ALTER TABLE `stock_collective_expense_detail` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
ALTER TABLE `stock_expense_detail` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
ALTER TABLE `stock_income_detail` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
ALTER TABLE `stock_write_off_detail` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
ALTER TABLE `wear_cards` CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

ALTER TABLE `operation_issued_in_subdivision`
    DROP FOREIGN KEY `fk_operation_issued_in_subdivision_4`;

ALTER TABLE `operation_issued_in_subdivision`
    ADD CONSTRAINT `fk_operation_issued_in_subdivision_4`
        FOREIGN KEY (`warehouse_operation_id`)
            REFERENCES `operation_warehouse` (`id`)
            ON DELETE RESTRICT
            ON UPDATE CASCADE;

ALTER TABLE `stock_income_detail`
    DROP FOREIGN KEY `fk_stock_income_detail_1`;

ALTER TABLE `stock_income_detail`
    ADD CONSTRAINT `fk_stock_income_detail_1`
        FOREIGN KEY (`employee_issue_operation_id`)
            REFERENCES `operation_issued_by_employee` (`id`)
            ON DELETE NO ACTION
            ON UPDATE CASCADE;