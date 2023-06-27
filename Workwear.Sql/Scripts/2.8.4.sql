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
ENGINE = InnoDB;
DEFAULT CHARACTER SET = utf8mb4;
