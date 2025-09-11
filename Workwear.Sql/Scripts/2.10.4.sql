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


-- Заявка на выдачу
CREATE TABLE issuance_requests(
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`receipt_date` DATE NOT NULL,
	`status` ENUM('New', 'Issued', 'PartiallyIssued') DEFAULT 'New' NOT NULL,
	`comment` TEXT NULL,
	`user_id` INT UNSIGNED NULL,
	`creation_date` DATETIME NULL DEFAULT NULL,
	PRIMARY KEY (`id`),
	CONSTRAINT `fk_issuance_request_user_id` FOREIGN KEY (`user_id`) REFERENCES users (`id`)
    	ON DELETE NO ACTION 
		ON UPDATE CASCADE,
	INDEX `issuance_request_user_id_idx` (`user_id` ASC) 
);

-- Сотрудники в заявках на выдачу
CREATE TABLE employees_issuance_request(
	`id` INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`employee_id` INT UNSIGNED NOT NULL,
	`issuance_request_id` INT UNSIGNED NOT NULL,
	PRIMARY KEY (`id`),
	CONSTRAINT `fk_employee_id` FOREIGN KEY (`employee_id`) REFERENCES employees (`id`)
    	ON DELETE CASCADE 
        ON UPDATE CASCADE,
	CONSTRAINT `fk_issuance_request_id` FOREIGN KEY  (`issuance_request_id`) REFERENCES issuance_requests (`id`)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
	INDEX `employee_id_idx` (`employee_id` ASC),
	INDEX `issuance_request_id_idx` (`issuance_request_id` ASC)
);

-- Добавление ссылки на заявку в коллективную выдачу
ALTER TABLE stock_collective_expense
	ADD COLUMN `issuance_request_id` INT UNSIGNED NULL DEFAULT NULL AFTER `transfer_agent_id`,
    ADD CONSTRAINT `fk_collective_expense_issuance_request_id` FOREIGN KEY (`issuance_request_id`) 
        REFERENCES issuance_requests (`id`)
		ON DELETE NO ACTION 
		ON UPDATE CASCADE,
	ADD INDEX `issuance_request_id_idx` (`issuance_request_id` ASC);
