alter table postomat_documents
	add `terminal_location` text null;

alter table postomat_document_items
	add `cell_number` integer UNSIGNED null;
