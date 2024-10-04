create table causes_write_off
(
	id int auto_increment primary key,
	name varchar(120) not null
);
insert into causes_write_off (name) values ('увольнение'), ('преждевременный износ'), ('изменение должности'), ('прочее');

