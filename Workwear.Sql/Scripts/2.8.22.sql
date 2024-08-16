alter table stock_completion
	add doc_number varchar(16) null after id;

alter table stock_transfer
	add doc_number varchar(16) null after id;
