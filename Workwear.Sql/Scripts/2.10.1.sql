-- Права на вход в настройки учета

ALTER TABLE users
	ADD COLUMN can_accounting_settings TINYINT(1) NOT NULL DEFAULT 1 AFTER can_delete;

-- Реализация поступления из поставки и поставки из прогноза
alter table shipment_items
	add diff_cause varchar(120) null after height_id,
	add ordered int unsigned not null after quantity,
	add received int unsigned not null after ordered;

alter table shipment
	modify status enum ('Draft', 'New', 'Present', 'Accepted', 'Ordered', 'Received') default 'Draft' not null,
	modify start_period date null,
	modify end_period date null,
	add full_ordered boolean default false not null after status,
	add full_received boolean default false not null after full_ordered,
	add has_receive boolean default false not null after full_received,
	add submitted datetime null after has_receive;

UPDATE shipment SET full_ordered = COALESCE((
												SELECT SUM(shipment_items.quantity > shipment_items.ordered) = 0
												FROM shipment_items
												WHERE shipment_items.shipment_id = shipment.id
											), FALSE);

alter table stock_income
	add shipment_id int unsigned default null null after warehouse_id;
alter table stock_income
	add constraint fk_stock_income_shipment
		foreign key (shipment_id) references shipment (id);

alter table user_settings
	add buyer_email varchar(320) null after maximize_on_start;
