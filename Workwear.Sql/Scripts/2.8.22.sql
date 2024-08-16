alter table stock_completion
	add doc_number varchar(16) null after id;

alter table stock_transfer
	add doc_number varchar(16) null after id;

alter table stock_transfer
	add organization_id int(10) unsigned null after doc_number;

alter table stock_transfer
	add constraint fk_stock_transfer_4
		foreign key (organization_id) references organizations (id);

create index index_stock_transfer_organization
	on stock_transfer (organization_id);

drop index index_stock_inspection_date on stock_transfer;

create index index_stock_transfer_date
	on stock_transfer (date);


