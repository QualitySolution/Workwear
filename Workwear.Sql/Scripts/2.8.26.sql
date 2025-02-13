alter table norms
	add column last_update timestamp default current_timestamp() not null on update current_timestamp() after id,
	ADD INDEX `norms_last_update_idx` (`last_update` DESC);

alter table norms_item
	add column last_update timestamp default current_timestamp() not null on update current_timestamp() after id,
	ADD INDEX `norms_item_last_update_idx` (`last_update` DESC);

alter table protection_tools
	add dermal_ppe tinyint(1) default 0 not null after item_types_id;

alter table protection_tools
	add dispenser tinyint(1) default 0 not null after dermal_ppe;
