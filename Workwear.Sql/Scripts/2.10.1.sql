-- Реализация оступления из поставки и поставки из прогноза
alter table shipment_items
	add diff_cause varchar(120) null after height_id,
	add ordered int unsigned not null after quantity;

alter table shipment
	modify status enum ('New', 'Present', 'Accepted', 'Ordered', 'Received') default 'New' not null,
	add full_ordered boolean default false not null after status;

UPDATE shipment SET full_ordered =
	(SELECT SUM(shipment_items.quantity > shipment_items.ordered) = 0
	 FROM shipment_items  WHERE shipment_items.shipment_id = shipment.id);

alter table stock_income
	add shipment_id int unsigned default null null after warehouse_id;
alter table stock_income
	add constraint fk_stock_income_shipment
		foreign key (shipment_id) references shipment (id);
