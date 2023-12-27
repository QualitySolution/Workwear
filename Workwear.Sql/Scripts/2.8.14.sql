# Снимаем лишнее ограничение
alter table norms_item
	modify amount int unsigned default 1 not null;

alter table issuance_sheet_items
	modify amount int unsigned not null;

