-- Добавление параметра для отключения номенклатуры нормы в норме выдачи
alter table norms_item
	add column is_hidden boolean default false not null;
