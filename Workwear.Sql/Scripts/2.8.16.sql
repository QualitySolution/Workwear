-- Добавляем поле  Email сотруднику
ALTER TABLE wear_cards
	ADD email TEXT NULL AFTER lk_registered;
