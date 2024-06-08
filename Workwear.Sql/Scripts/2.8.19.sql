alter table `postomat_document_items`
	modify column `cell_number` varchar(10) null default null;

alter table norms
	add archival TINYINT(1) not null DEFAULT 0  after dateto;
