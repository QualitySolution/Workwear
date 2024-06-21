alter table clothing_service_claim
	add preferred_terminal_id int(11) unsigned null after is_closed;

alter table clothing_service_claim
	add comment text null;
