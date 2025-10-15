--
-- Добавляем внешние ключи для документа выдачи по дежурной норме в ведомость
--

ALTER TABLE `issuance_sheet`
	ADD `stock_expense_duty_norm_id` INT UNSIGNED NULL AFTER `stock_expense_id`;
ALTER TABLE `issuance_sheet`
	ADD CONSTRAINT `fk_stock_expense_duty_norm_id`
		FOREIGN KEY (`stock_expense_duty_norm_id`) REFERENCES `stock_expense_duty_norm` (`id`)
			ON UPDATE CASCADE ON DELETE NO ACTION;

ALTER TABLE `issuance_sheet_items`
	ADD `stock_expense_duty_norm_item_id` INTEGER UNSIGNED NULL AFTER `stock_expense_detail_id`;
ALTER TABLE `issuance_sheet_items`
	ADD CONSTRAINT `fk_stock_expense_duty_norm_item_id`
		FOREIGN KEY (`stock_expense_duty_norm_item_id`) REFERENCES `stock_expense_duty_norm_items` (`id`)
			ON UPDATE CASCADE ON DELETE CASCADE ;

-- Документ закупки
CREATE TABLE `shipment`
(
    `id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    `start_period` DATE NOT NULL,
    `end_period` DATE NOT NULL,
	`status` ENUM('Ordered','OnTheWay', 'AwaitPayment','Cancelled','Received') NOT NULL DEFAULT 'Ordered',
    `user_id` INT UNSIGNED NULL,
    `comment` TEXT NULL,
    `creation_date` DATETIME NULL
);
CREATE INDEX `index_shipment_start_period`
    ON `shipment` (`start_period`);
CREATE INDEX `index_shipment_end_period`
    ON `shipment` (`end_period`);

-- Строки документа закупки
CREATE TABLE `shipment_items`
(
    `id` INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    `shipment_id` INT UNSIGNED NOT NULL,
    `nomenclature_id` INT UNSIGNED NOT NULL,
    `quantity` INT UNSIGNED NOT NULL,
    `cost` DECIMAL(10, 2) UNSIGNED DEFAULT 0.00 NOT NULL,
    `size_id` INT UNSIGNED NULL,
    `height_id` INT UNSIGNED NULL,
    `comment` VARCHAR(120) NULL,
    CONSTRAINT `fk_shipment_items_doc`
        FOREIGN KEY (`shipment_id`) REFERENCES `shipment` (`id`)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT `fk_shipment_items_nomenclature`
        FOREIGN KEY (`nomenclature_id`) REFERENCES `nomenclature` (`id`)
            ON UPDATE CASCADE,
    CONSTRAINT `fk_shipment_items_size_id`
        FOREIGN KEY (`size_id`) REFERENCES `sizes` (`id`)
            ON UPDATE CASCADE,
    CONSTRAINT `fk_shipment_items_height_id`
        FOREIGN KEY (`height_id`) REFERENCES `sizes` (`id`)
            ON UPDATE CASCADE
);
CREATE INDEX `index_shipment_items_doc`
    ON `shipment_items` (`shipment_id`);

CREATE INDEX `index_shipment_items_height`
    ON `shipment_items` (`height_id`);

CREATE INDEX `index_shipment_items_nomenclature`
    ON `shipment_items` (`nomenclature_id`);

CREATE INDEX `index_shipment_items_size`
    ON `shipment_items` (`size_id`);
