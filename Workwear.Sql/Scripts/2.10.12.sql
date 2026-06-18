-- Добавление руководителя подразделения в таблицу подразделений
ALTER TABLE subdivisions
	ADD COLUMN head_of_division_id INT UNSIGNED NULL DEFAULT NULL AFTER comment,
	ADD INDEX fk_subdivisions_head_of_division_idx (head_of_division_id ASC),
	ADD CONSTRAINT fk_subdivisions_head_of_division
		FOREIGN KEY (head_of_division_id) REFERENCES employees (id)
			ON DELETE SET NULL ON UPDATE CASCADE;

-- Добавление руководителя отдела в таблицу отделов
ALTER TABLE departments
	ADD COLUMN head_of_department_id INT UNSIGNED NULL DEFAULT NULL AFTER comments,
	ADD INDEX fk_departments_head_of_department_idx (head_of_department_id ASC),
	ADD CONSTRAINT fk_departments_head_of_department
		FOREIGN KEY (head_of_department_id) REFERENCES employees (id)
			ON DELETE SET NULL ON UPDATE CASCADE;

-- Добавление дневной стоимости аренды в таблицу номенклатуры
ALTER TABLE nomenclature
	ADD COLUMN rent_cost_day DECIMAL(7,2) UNSIGNED NULL DEFAULT NULL AFTER sale_cost;

-- Причины выдачи
create table causes_issue
(
	id int UNSIGNED auto_increment primary key,
	name varchar(120) not null
)
	DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
insert into causes_issue (name) values ('Вновь принятый'), ('По окончании срока носки'), ('Замена размера'), ('Досрочное списание'), ('Перевод');

-- Добавление свойств и класса защиты в номенклатуру
ALTER TABLE nomenclature
	ADD COLUMN protection_properties VARCHAR(120) NULL DEFAULT NULL AFTER additional_info,
	ADD COLUMN protection_class VARCHAR(16) NULL DEFAULT NULL AFTER protection_properties;

-- Добавление веса номенклатуры
ALTER TABLE nomenclature
	ADD COLUMN weight INT NOT NULL DEFAULT 0 AFTER protection_class;

-- Добавление номера комплекта
ALTER TABLE operation_barcodes
	ADD COLUMN kit_number INT UNSIGNED NULL AFTER warehouse_operation_id;

