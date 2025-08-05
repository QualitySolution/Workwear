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

-- Добавление поля для уведомлений о просроченных вещах
alter table postomat_document_items
	add column notification_sent boolean not null default false;
