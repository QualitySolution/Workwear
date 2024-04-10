create table if not exists `protection_tools_category_for_analytics`
(
	`id`   int(11) unsigned not null auto_increment primary key,
	`name` varchar(100)     not null,
	`comment` text null default null
);

alter table `protection_tools`
	add column
		`category_for_analytics_id` int unsigned null default null,
	add constraint `FK_protection_tools_category_for_analytics`
		foreign key (`category_for_analytics_id`)
			references `protection_tools_category_for_analytics` (`id`)
			on delete set null
			on update cascade 
