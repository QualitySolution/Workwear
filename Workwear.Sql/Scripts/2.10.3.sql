-- Исключение выходных дней
DELIMITER $$
CREATE FUNCTION `count_working_days` (`start_date` DATE, `end_date` DATE)
    RETURNS INT
    DETERMINISTIC
    COMMENT 'Функция подсчитывает количество дней нахождения спецодежды на каждом этапе, исключая выходные дни'
BEGIN
    RETURN (WITH RECURSIVE date_range AS
                               (SELECT start_date as sd
                                UNION ALL
                                SELECT DATE_ADD(sd, INTERVAL 1 Day)
                                FROM date_range
                                WHERE DATE_ADD(sd, INTERVAL 1 Day) < end_date
                               )
            SELECT COUNT(*)
            FROM date_range
            WHERE WEEKDAY(sd) NOT IN (5, 6) AND start_date < end_date
    );
END $$;
DELIMITER ;

-- Добавление дополнительной информации в номенклатуру
ALTER TABLE nomenclature
	ADD COLUMN additional_info TEXT NULL DEFAULT NULL AFTER number;

-- ------------------------------------------------------
-- Запрет смены размера для потребности в личном кабинете
-- ------------------------------------------------------
alter table protection_tools
	add size_change int null  comment 'null - не ограничиваем, 0 - не даём всегда, остальное - число дней до выдачи, когда нужно ограничивать редактирования типа размера этого объекта' 
		after dispenser;

-- Новое расписание работы склада
create table days_schedule (
	   id   	     	int unsigned auto_increment,
	   date 		 	date null 			comment 'Если указана дата, то расписание действует только на эту дату',
	   day_of_week 		int unsigned null 	comment 'Если указано, то расписание действует на этот день недели (1-Пн, 2-Вт, ..., 7-Вс)',
	   start 			time null 			comment 'Время начала рабочего дня, если null, то день нерабочий',
	   end 		    	time null 			comment 'Время окончания рабочего дня, если null, то день нерабочий',
	   visit_interval  	int unsigned null 	comment 'Интервал между записями на приём в минутах, если null, то день нерабочий',
	   comment 	 		text null,
	   constraint days_schedule
		   primary key (id)
);

-- Добавление поля для уведомлений о просроченных вещах
alter table postomat_document_items
	add column notification_sent boolean not null default false;

-- Создаем пропущенные индексы для снижения нагрузки на ЦП сервиса постоматов
ALTER TABLE `clothing_service_states` ADD INDEX(`operation_time`);
