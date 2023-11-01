--Добавление общих данных для документа списания
alter table stock_write_off	collate = utf8mb3_general_ci;
alter table stock_write_off	add organization_id int unsigned null after user_id;
alter table stock_write_off	add director_id int unsigned null after organization_id;
alter table stock_write_off	add chairman_id int unsigned null after director_id;
create index fk_stock_write_off_chairman_id_idx	on stock_write_off (chairman_id);
create index fk_stock_write_off_director_idx	on stock_write_off (director_id);
create index fk_stock_write_off_organization_idx	on stock_write_off (organization_id);
alter table stock_write_off
    add constraint fk_stock_write_off_chairman_id
		foreign key (chairman_id) references leaders (id)
			on update cascade on delete set null;
alter table stock_write_off
	add constraint fk_stock_write_off_organization_id
		foreign key (organization_id) references organizations (id)
			on update cascade on delete set null;
alter table stock_write_off
	add constraint stock_inspection_fk_director_id
		foreign key (director_id) references leaders (id)
			on update cascade on delete set null;

--Добавление комментария в строку списания
alter table stock_write_off_detail	add cause text null;

--Члены комиссии документа списания
create table stock_write_off_members
(
	id int unsigned auto_increment
		primary key,
	write_off_id int unsigned not null,
	member_id    int unsigned not null,
	constraint stock_write_off_members_fk1
		foreign key (write_off_id) references stock_write_off (id)
			on update cascade on delete cascade,
	constraint stock_write_off_members_fk2
		foreign key (member_id) references leaders (id)
			on update cascade
);
create index stock_write_off_members_fk1_idx	on stock_write_off_members (write_off_id);
create index stock_write_off_members_fk2_idx	on stock_write_off_members (member_id);

