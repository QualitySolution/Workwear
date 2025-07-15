-- Добавление параметра для отключения строки нормы
alter table norms_item
	add column is_disabled boolean default false not null;
