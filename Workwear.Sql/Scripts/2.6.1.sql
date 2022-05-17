ALTER TABLE `stock_write_off_detail`
    DROP FOREIGN KEY `fk_stock_write_off_detail_nomenclature`;

ALTER TABLE `stock_write_off_detail`
    CHANGE COLUMN `nomenclature_id` `nomenclature_id` INT(10) UNSIGNED NULL DEFAULT NULL ;

ALTER TABLE `stock_write_off_detail`
    ADD CONSTRAINT `fk_stock_write_off_detail_nomenclature`
        FOREIGN KEY (`nomenclature_id`)
            REFERENCES `nomenclature` (`id`)
            ON DELETE RESTRICT
            ON UPDATE CASCADE;
