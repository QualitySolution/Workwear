alter table postomat_documents
	add `terminal_location` text null;

alter table postomat_document_items
	add `cell_number` int(11) UNSIGNED null;
