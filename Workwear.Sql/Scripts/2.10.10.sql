-- Добавление размеров номенклатур 
create table nomenclature_sizes
(
	id              int unsigned auto_increment,
	nomenclature_id int unsigned not null,
	size_id         int unsigned null,
	height_id       int unsigned null,
	comment         text         null,
	constraint nomenclature_sizes_pk
		primary key (id),
	constraint nomenclature_sizes_nomenclature_id_fk
		foreign key (nomenclature_id) references nomenclature (id)
			on update cascade on delete cascade,
	constraint nomenclature_sizes_sizes_id_fk
		foreign key (size_id) references sizes (id)
			on update cascade on delete set null,
	constraint nomenclature_sizes_sizes_id_fk_2
		foreign key (height_id) references sizes (id)
			on update cascade on delete set null
)
	comment 'Сечетания размера и роста доступные для номенклатуры';

