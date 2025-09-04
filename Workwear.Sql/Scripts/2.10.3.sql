-- Рассписание работы склада
create table days_schedule (
	   id   	     	int unsigned auto_increment,
	   day_of_week 		int unsigned not null comment 'Восскресенье обозначается как 7',
	   start 		 	time not null,
	   end 		    	time not null,
	   visit_interval   int unsigned not null,
	   		constraint days_schedule
		  	primary key (id)
);

-- Создаем пропущенные индексы для снижения нагрузки на ЦП сервиса постоматов
ALTER TABLE `clothing_service_states` ADD INDEX(`operation_time`);
