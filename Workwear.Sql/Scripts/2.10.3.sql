create table days_schedule (
	   id   	     	int unsigned auto_increment,
	   day_of_week 		int unsigned not null,
	   start 		 	time not null,
	   end 		    	time not null,
	   visit_interval   int unsigned not null,
	   		constraint days_schedule
		  	primary key (id),
);
