ALTER TABLE `user_settings` 
DROP FOREIGN KEY `fk_user_settings_responsible_person_id`;

ALTER TABLE `user_settings` 
ADD CONSTRAINT `fk_user_settings_responsible_person_id`
  FOREIGN KEY (`default_responsible_person_id`)
  REFERENCES `leaders` (`id`)
  ON DELETE NO ACTION
  ON UPDATE NO ACTION;
