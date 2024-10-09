create table causes_write_off
(
	id int auto_increment primary key,
	name varchar(120) not null
);
insert into causes_write_off (name) values ('увольнение'), ('преждевременный износ'), ('изменение должности'), ('прочее');

alter table stock_write_off_detail
	add column cause_write_off_id int
		references causes_write_off(id)
	after akt_number;

alter table stock_write_off_detail
	rename column cause to comment;

alter table stock_write_off_detail
	drop constraint stock_write_off_detail_ibfk_1;
alter table stock_write_off_detail
	add constraint stock_write_off_detail_ibfk_1 foreign key (cause_write_off_id) references causes_write_off (id)
		on update cascade on delete set null;
