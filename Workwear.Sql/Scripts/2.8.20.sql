alter table clothing_service_claim
	add preferred_terminal_id int(11) unsigned null after is_closed;

alter table clothing_service_claim
	add comment text null;

-- Изменение типа зависимости на каскад, чтобы можно было проще удалять стирки.
ALTER TABLE `clothing_service_states` DROP FOREIGN KEY `fk_clame_id`;
ALTER TABLE `clothing_service_states` ADD CONSTRAINT `fk_clame_id` 
	FOREIGN KEY (`claim_id`) 
	REFERENCES `clothing_service_claim`(`id`)
	ON DELETE CASCADE ON UPDATE CASCADE; 
