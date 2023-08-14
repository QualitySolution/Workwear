-- Удаляем глупый и бессмысленный индекс
ALTER TABLE `stock_transfer_detail`
    DROP INDEX `id_UNIQUE` ;