alter table protection_tools
	add washing_ppe tinyint(1) default 0 not null after item_types_id;

alter table protection_tools
	add dispenser tinyint(1) default 0 not null after washing_ppe;
