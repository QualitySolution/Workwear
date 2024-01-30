alter table postomat_document_items
	add employee_id int UNSIGNED NOT NULL after last_update;

alter table postomat_document_items
	add constraint fk_employee_id
		foreign key (employee_id) references wear_cards (id);

alter table clothing_service_claim
	add employee_id int UNSIGNED NOT NULL after barcode_id;
	
alter table clothing_service_claim
	add constraint fk_employee_id
		foreign key (employee_id) references wear_cards (id);
