﻿-- Документ закупки
create table shipment
(
    id int unsigned auto_increment primary key,
    start_period date not null,
    end_period date not null,
    user_id int unsigned null,
    comment text null,
    creation_date datetime null
);
create index index_shipment_start_period
    on shipment (start_period);
create index index_shipment_end_period
    on shipment (end_period);

-- Строки документа закупки
create table shipment_items
(
    id int unsigned auto_increment primary key,
    shipment_id int unsigned not null,
    nomenclature_id int unsigned not null,
    quantity int unsigned not null,
    cost decimal(10, 2) unsigned default 0.00 not null,
    size_id int unsigned null,
    height_id int unsigned null,
    comment varchar(120) null,
    constraint fk_shipment_items_doc
        foreign key (shipment_id) references shipment (id)
            on update cascade on delete cascade,
    constraint fk_shipment_items_nomenclature
        foreign key (nomenclature_id) references nomenclature (id)
            on update cascade,
    constraint fk_shipment_items_size_id
        foreign key (size_id) references sizes (id)
            on update cascade,
    constraint fk_shipment_items_height_id
        foreign key (height_id) references sizes (id)
            on update cascade
);
create index index_shipment_items_doc
    on shipment_items (shipment_id);

create index index_shipment_items_height
    on shipment_items (height_id);

create index index_shipment_items_nomenclature
    on shipment_items (nomenclature_id);

create index index_shipment_items_size
    on shipment_items (size_id);

alter table shipment
	add status text not null default 'Заказано' after end_period;
