ALTER TABLE `clothing_service_claim`
	ADD `preferred_terminal_id` INT(11) UNSIGNED NULL AFTER `is_closed`;

ALTER TABLE `clothing_service_claim`
	ADD `comment` TEXT NULL;

-- Изменение типа зависимости на каскад, чтобы можно было проще удалять стирки.
ALTER TABLE `clothing_service_states` DROP FOREIGN KEY `fk_clame_id`;
ALTER TABLE `clothing_service_states` ADD CONSTRAINT `fk_clame_id` 
	FOREIGN KEY (`claim_id`) 
	REFERENCES `clothing_service_claim`(`id`)
	ON DELETE CASCADE ON UPDATE CASCADE; 
