-- Добавление поля для архивации номенклатуры нормы
alter table protection_tools 
	add column archival bool not null default false;
