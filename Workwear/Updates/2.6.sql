-- Удаляем больше не используемый по умолчанию параметр базы edition
DELETE FROM `base_parameters` WHERE `base_parameters`.`name` = 'edition' AND `base_parameters`.`str_value` = 'gpl'