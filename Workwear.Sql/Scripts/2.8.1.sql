ALTER TABLE `objects`
    ADD INDEX `index_objects_code` (`code` ASC);

ALTER TABLE `nomenclature`
    ADD INDEX `index_nomenclature_number` (`number` ASC);

ALTER TABLE `wear_cards`
    ADD INDEX `index_wear_cards_personal_number` (`personnel_number` ASC),
    ADD INDEX `index_wear_cards_last_name` (`last_name` ASC),
    ADD INDEX `index_wear_cards_first_name` (`first_name` ASC),
    ADD INDEX `index_wear_cards_patronymic_name` (`patronymic_name` ASC),
    ADD INDEX `index_wear_cards_dismiss_date` (`dismiss_date` ASC),
    ADD INDEX `index_wear_cards_phone_number` (`phone_number` ASC);

ALTER TABLE `stock_income`
    ADD INDEX `index_stock_income_date` (`date` ASC);

ALTER TABLE `stock_expense`
    ADD INDEX `index_stock_expense_date` (`date` ASC),
    ADD INDEX `index_stock_expense_operation` (`operation` ASC);

ALTER TABLE `stock_write_off`
    ADD INDEX `index_stock_write_off_date` (`date` ASC);

ALTER TABLE `wear_cards_item`
    ADD INDEX `index_wear_cards_item_next_issue` (`next_issue` ASC);

ALTER TABLE `issuance_sheet`
    ADD INDEX `index_issuance_sheet_date` (`date` ASC);

ALTER TABLE `operation_warehouse`
    ADD INDEX `index_operation_warehouse_time` (`operation_time` ASC),
    ADD INDEX `index_operation_warehouse_wear_percent` (`wear_percent` ASC);

ALTER TABLE `stock_transfer`
    ADD INDEX `index_stock_inspection_date` (`date` ASC),
    DROP INDEX `id_UNIQUE` ;

ALTER TABLE `stock_collective_expense`
    ADD INDEX `index_stock_collective_expense_date` (`date` ASC);

ALTER TABLE `stock_completion`
    ADD INDEX `index_stock_completion_date` (`date` ASC);

ALTER TABLE `operation_barcodes`
    ADD UNIQUE INDEX `index_uniq` (`barcode_id` ASC, `employee_issue_operation_id` ASC, `warehouse_operation_id` ASC);

ALTER TABLE `stock_inspection`
    ADD INDEX `index_stock_inspection_date` (`date` ASC);