alter table stock_completion
	add doc_number varchar(16) null after id;

alter table stock_transfer
	add doc_number varchar(16) null after id;

alter table stock_transfer
	add organization_id int(10) unsigned null after doc_number;

alter table stock_transfer
	add CONSTRAINT `fk_stock_transfer_4`
		FOREIGN KEY (`organization_id`)
			REFERENCES `organizations` (`id`)
			ON DELETE SET NULL
			ON UPDATE CASCADE;

create index index_stock_transfer_organization
	on stock_transfer (organization_id);

drop index index_stock_inspection_date on stock_transfer;

create index index_stock_transfer_date
	on stock_transfer (date);


