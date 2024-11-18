alter table norms
	add column last_update timestamp default current_timestamp() not null on update current_timestamp();

alter table norms_item
	add column last_update timestamp default current_timestamp() not null on update current_timestamp();
