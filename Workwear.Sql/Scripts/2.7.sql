ALTER TABLE `objects` 
ADD COLUMN `parent_object_id` INT(10) UNSIGNED NULL DEFAULT NULL AFTER `warehouse_id`,
ADD INDEX `fk_objects_2_idx` (`parent_object_id` ASC);

ALTER TABLE `nomenclature` 
ADD COLUMN `archival` TINYINT(1) NOT NULL DEFAULT 0 AFTER `number`;

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
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4;

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
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4;

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
AUTO_INCREMENT = 1
DEFAULT CHARACTER SET = utf8mb4;

ALTER TABLE `wear_cards` 
DROP COLUMN `fill_date`,
ADD COLUMN `birth_date` DATE NULL DEFAULT NULL AFTER `sex`;

ALTER TABLE `stock_income` 
ADD COLUMN `creation_date` DATETIME NULL DEFAULT NULL AFTER `comment`;

ALTER TABLE `stock_expense` 
ADD COLUMN `creation_date` DATETIME NULL DEFAULT NULL AFTER `write_off_doc`;

ALTER TABLE `stock_write_off` 
ADD COLUMN `creation_date` DATETIME NULL DEFAULT NULL AFTER `comment`;

ALTER TABLE `wear_cards_item` 
ADD COLUMN `next_issue_annotation` VARCHAR(240) NULL DEFAULT NULL AFTER `amount`;

ALTER TABLE `stock_mass_expense` 
ADD COLUMN `creation_date` DATETIME NULL DEFAULT NULL AFTER `comment`;

ALTER TABLE `stock_transfer` 
ADD COLUMN `creation_date` DATETIME NULL DEFAULT NULL AFTER `comment`;

ALTER TABLE `stock_collective_expense` 
ADD COLUMN `creation_date` DATETIME NULL DEFAULT NULL AFTER `comment`;

ALTER TABLE `norm_conditions` 
ADD COLUMN `issuance_start` DATETIME NULL DEFAULT NULL AFTER `sex`,
ADD COLUMN `issuance_end` DATETIME NULL DEFAULT NULL AFTER `issuance_start`;

CREATE TABLE IF NOT EXISTS `stock_completion` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `date` DATE NOT NULL,
  `user_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `warehouse_receipt_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `comment` TEXT NULL DEFAULT NULL,
  `warehouse_expense_id` INT(10) UNSIGNED NULL DEFAULT NULL,
  `creation_date` DATETIME NULL DEFAULT NULL,
  PRIMARY KEY (`id`),
  INDEX `fk_stock_completion_user_id_idx` (`user_id` ASC),
  INDEX `fk_stock_completion_warehouse_receipt_idx` (`warehouse_receipt_id` ASC),
  INDEX `fk_stock_completion_warehouse_expense_idx` (`warehouse_expense_id` ASC),
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
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `stock_completion_source_item` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_completion_id` INT(10) UNSIGNED NOT NULL,
  `warehouse_operation_id` INT(10) UNSIGNED NOT NULL,
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
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

CREATE TABLE IF NOT EXISTS `stock_completion_result_item` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `stock_completion_id` INT(10) UNSIGNED NOT NULL,
  `warehouse_operation_id` INT(10) UNSIGNED NOT NULL,
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
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4;

ALTER TABLE `objects` 
ADD CONSTRAINT `fk_objects_2`
  FOREIGN KEY (`parent_object_id`)
  REFERENCES `objects` (`id`)
  ON DELETE SET NULL
  ON UPDATE NO ACTION;
