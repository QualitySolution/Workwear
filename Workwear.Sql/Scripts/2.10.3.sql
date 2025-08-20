-- -----------------------------------
-- Учёт RFID
-- -----------------------------------
alter table barcodes
	modify title varchar(24) null;

alter table barcodes
	add type enum ('EAN13', 'EPC96') default 'EAN13' null after last_update;

create index barcodes_type_idx
	on barcodes (type);

alter table barcodes
	drop key value_UNIQUE;

alter table barcodes
	add constraint value_UNIQUE
		unique (type, title);
