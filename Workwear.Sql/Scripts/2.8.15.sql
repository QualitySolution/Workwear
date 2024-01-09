# Убрал unsigned
alter table norms_item
	modify amount int default 1 not null;
