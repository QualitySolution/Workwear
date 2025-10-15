-- -----------------------------------
-- Учёт RFID
-- -----------------------------------
alter table barcodes
	modify title varchar(24) not null;

alter table barcodes
	add type enum ('EAN13', 'EPC96') default 'EAN13' not null after last_update;

create index barcodes_type_idx
	on barcodes (type);

alter table barcodes
	drop key value_UNIQUE;

alter table barcodes
	add constraint value_UNIQUE
		unique (type, title);

-- Создание новой функции, которая корректно считает количество, необходимое к выдаче
DELIMITER $$
CREATE FUNCTION `quantity_issue`(
	`amount` INT UNSIGNED,
	`norm_period` INT UNSIGNED,
	`next_issue` DATE,
	`begin_date` DATE,
	`end_date` DATE,
	`begin_Issue_Period` DATE,
	`end_Issue_Period` DATE)
	RETURNS int(10) unsigned
    NO SQL
    DETERMINISTIC
    COMMENT 'Функция рассчитывает количество, необходимое к выдаче.'
BEGIN
    DECLARE issue_count INT;
    DECLARE next_issue_new DATE;
    DECLARE begin_Issue_Period_New DATE;
    DECLARE end_Issue_Period_New DATE;
    DECLARE start_Of_Year DATE;
    DECLARE end_Of_Year DATE;

    IF norm_period <= 0 THEN RETURN 0; END IF;
    IF next_issue IS NULL THEN RETURN 0; END IF;

    SET issue_count = 0;
    SET next_issue_new = CONCAT('2000', '-', MONTH(next_issue), '-', DAY(next_issue));
    SET begin_Issue_Period_New = CONCAT('2000', '-', MONTH(begin_Issue_Period), '-', DAY(begin_Issue_Period));
    SET end_Issue_Period_New = CONCAT('2000', '-', MONTH(end_Issue_Period), '-', DAY(end_Issue_Period));
    SET start_Of_Year = CONCAT('2000', '-', 1, '-',  1);
    SET end_Of_Year = CONCAT('2000', '-', 12 , '-', 31);

    WHILE next_issue <= end_date DO
            IF begin_Issue_Period IS NOT NULL THEN
                IF (next_issue_new  BETWEEN begin_Issue_Period_New AND end_Issue_Period_New)
                    OR ((next_issue_new BETWEEN begin_Issue_Period_New AND end_Of_Year OR next_issue_new BETWEEN start_Of_Year AND end_Issue_Period_New)
                        AND (MONTH(begin_Issue_Period) > MONTH(end_Issue_Period))) THEN
                    IF next_issue >= begin_date THEN
                        SET issue_count = issue_count + amount;
					END IF;
                    SET next_issue = DATE_ADD(next_issue, INTERVAL norm_period MONTH);
				END IF;
                IF (next_issue_new BETWEEN (DATE_ADD(end_Issue_Period_New, INTERVAL 1 DAY)) AND (DATE_ADD(begin_Issue_Period_New, INTERVAL -1 DAY)))
                    OR ((next_issue_new < begin_Issue_Period_New OR next_issue_new > end_Issue_Period_New) AND (MONTH(begin_Issue_Period) <= MONTH(end_Issue_Period))) THEN
                    SET next_issue = CONCAT(YEAR(next_issue), '-', MONTH(begin_Issue_Period), '-', DAY(begin_Issue_Period));
                    IF (next_issue_new > end_Issue_Period_New) AND (MONTH(begin_Issue_Period) <= MONTH(end_Issue_Period)) THEN
                        SET next_issue = DATE_ADD(next_issue, INTERVAL 1 YEAR);
					END IF;
				END IF;
			ELSE
                IF next_issue >= begin_date THEN
                    SET issue_count = issue_count + amount;
				END IF;
                SET next_issue = DATE_ADD(next_issue, INTERVAL norm_period MONTH);
			END IF;
            SET next_issue_new = CONCAT('2000', '-', MONTH(next_issue), '-', DAY(next_issue));
	END WHILE;
	RETURN issue_count;
END$$

DELIMITER ;

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

-- Создаем пропущенные индексы для снижения нагрузки на ЦП сервиса постаматов
ALTER TABLE `clothing_service_states` ADD INDEX(`operation_time`);

-- ---------------------------------------------
-- Каталог, выбор пользователем предпочтительных номенклатур
-- ---------------------------------------------
alter table nomenclature
	add catalog_id char(24) null after archival;

alter table protection_tools_nomenclature
	add can_choose boolean default false not null;

create table employees_selected_nomenclatures
(
	id                  int unsigned auto_increment,
	employee_id         int unsigned not null,
	protection_tools_id int unsigned not null,
	nomenclature_id     int unsigned not null,
	constraint employees_selected_nomenclatures_pk
		primary key (id),
	constraint employees_selected_nomenclatures_employees_id_fk
		foreign key (employee_id) references employees (id)
			on update cascade on delete cascade,
	constraint employees_selected_nomenclatures_nomenclature_id_fk
		foreign key (nomenclature_id) references nomenclature (id)
			on update cascade on delete cascade,
	constraint employees_selected_nomenclatures_protection_tools_id_fk
		foreign key (protection_tools_id) references protection_tools (id)
			on update cascade on delete cascade
)
	comment 'Номенклатуры выбранные пользователем, как предпочтительные к выдаче';

-- Добавление операции выдачи по дежурной норме в ведомость
ALTER TABLE issuance_sheet_items 
	ADD COLUMN duty_norm_issue_operation_id int(10) unsigned NULL DEFAULT NULL AFTER issued_operation_id;

ALTER TABLE issuance_sheet_items
	ADD CONSTRAINT fk_issuance_sheet_items_duty_norm_issue_operation_id
		FOREIGN KEY (duty_norm_issue_operation_id) REFERENCES operation_issued_by_duty_norm(id)
			ON UPDATE CASCADE 
			ON DELETE NO ACTION;

CREATE INDEX fk_issuance_sheet_items_duty_norm_issue_operation_idx 
	ON issuance_sheet_items(duty_norm_issue_operation_id ASC);

-- Добавление операций выдачи по дежурной норме в операции со штрихкодами
ALTER TABLE operation_barcodes
	ADD COLUMN duty_norm_issue_operation_id int(10) unsigned NULL DEFAULT NULL AFTER employee_issue_operation_id;

ALTER TABLE operation_barcodes
	ADD CONSTRAINT fk_operation_barcodes_duty_norm_issue_operation_id
		FOREIGN KEY (duty_norm_issue_operation_id) REFERENCES operation_issued_by_duty_norm(id)
			ON UPDATE CASCADE
			ON DELETE NO ACTION;

CREATE INDEX fk_operation_barcodes_duty_norm_issue_operation_id_idx
	ON operation_barcodes(duty_norm_issue_operation_id ASC);

-- Подсветка в журнале сотрудников по подразделению
alter table subdivisions
	add employees_color varchar(7) null;
