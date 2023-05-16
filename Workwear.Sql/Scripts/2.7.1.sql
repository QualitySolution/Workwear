-- Удаляем плохой индекс уникальности
ALTER TABLE `wear_cards` DROP INDEX `personnel_number_UNIQUE`;

-- Исправляем возможное появление отсутствующего склада
UPDATE `stock_income` SET `warehouse_id`=(
    SELECT operation_warehouse.warehouse_receipt_id FROM stock_income_detail
    JOIN operation_warehouse ON stock_income_detail.warehouse_operation_id = operation_warehouse.id
    WHERE stock_income_detail.stock_income_id = stock_income.id
    LIMIT 1)
WHERE warehouse_id IS NULL;

-- Если запрос выше не смог установить значение склада, значит документ без строк. Удаляем такие документы
DELETE FROM `stock_income` WHERE `warehouse_id` IS NULL;

-- Корректируем колонку чтобы значение null было не возможно.
ALTER TABLE `stock_income`
    DROP FOREIGN KEY `fk_stock_income_1`;

ALTER TABLE `stock_income`
    CHANGE COLUMN `warehouse_id` `warehouse_id` INT(10) UNSIGNED NOT NULL ;

ALTER TABLE `stock_income`
    ADD CONSTRAINT `fk_stock_income_1`
        FOREIGN KEY (`warehouse_id`)
            REFERENCES `warehouse` (`id`)
            ON DELETE NO ACTION
            ON UPDATE CASCADE;