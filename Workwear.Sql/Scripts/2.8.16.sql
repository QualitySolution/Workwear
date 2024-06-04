-- Добавляем поле  Email сотруднику
alter table wear_cards
	add email text null after lk_registered;
