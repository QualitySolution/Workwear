-- Добавление причины выдачи в документ расхода
ALTER TABLE stock_expense
	ADD COLUMN cause_issue_id INT UNSIGNED NULL AFTER warehouse_id,
	ADD INDEX fk_stock_expense_cause_issue_idx (cause_issue_id ASC),
	ADD CONSTRAINT fk_stock_expense_cause_issue
		FOREIGN KEY (cause_issue_id) REFERENCES causes_issue (id)
			ON DELETE SET NULL ON UPDATE CASCADE;

-- Операции выдачи вне нормы
create table `operation_over_norm`
(
	`id`                              int unsigned                           not null auto_increment primary key,
	`operation_time`                  datetime                               not null,
	`last_update`                     timestamp                              not null default current_timestamp() not null on update current_timestamp(),
	`type`                            enum ('Simple', 'Substitute', 'Guest') not null,
	`nomenclature_id` 				  INT UNSIGNED							 NOT NULL,
	`size_id` 						  INT UNSIGNED							 NULL DEFAULT NULL,
	`height_id`						  INT UNSIGNED							 NULL DEFAULT NULL,
	`wear_percent`					  DECIMAL(3,2)							 UNSIGNED NOT NULL DEFAULT 0.00,
	`employee_id` 					  int unsigned 							 not null,
	`operation_warehouse_id`          int unsigned                           not null,
	`substituted_issue_operation_id`  int unsigned                           null     default null,
	`return_from_operation`           int unsigned                           null     default null,
	constraint `FK_over_norm_substituted_issued`
	foreign key (`substituted_issue_operation_id`) references `operation_issued_by_employee` (`id`)
	                                                                                                                       on update cascade
	                                                                                                                       on delete no action,
	constraint `FK_over_norm_return_from_operation`
	foreign key (`return_from_operation`) references `operation_over_norm` (`id`)
	                                                                                                                       on update cascade
	                                                                                                                       on delete cascade,
	constraint `FK_over_norm_op_warehouse`
	foreign key (`operation_warehouse_id`) references `operation_warehouse` (`id`)
	                                                                                                                       on update cascade
	                                                                                                                       ON DELETE RESTRICT,
	constraint `FK_over_norm_nomenclature`
	foreign key (`nomenclature_id`) references `nomenclature` (`id`)
	                                                                                                                       on update cascade
	                                                                                                                       ON DELETE RESTRICT,
	constraint `FK_over_norm_size`
	foreign key (`size_id`) references `sizes` (`id`)
	                                                                                                                       on update cascade
	                                                                                                                       on delete SET NULL,
	constraint `FK_over_norm_height`
	foreign key (`height_id`) references `sizes` (`id`)
	                                                                                                                       on update cascade
	                                                                                                                       on delete SET NULL,
	constraint `FK_over_norm_employee`
	foreign key (`employee_id`) references `employees` (`id`)
	                                                                                                                       on update cascade
	                                                                                                                       on delete RESTRICT
	)
	ENGINE = InnoDB
	DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

alter table operation_barcodes
	add column `over_norm_operation_id` int unsigned null,
	add constraint `FK_op_barcodes_op_over_norm_operation`
		foreign key (`over_norm_operation_id`) references `operation_over_norm` (`id`)
			on update cascade on delete cascade;

ALTER TABLE `barcodes`
	ADD COLUMN `label` varchar(50) null default null;

create table `stock_over_norms`
(
	`id`            int unsigned                           not null auto_increment primary key,
	`doc_number`    varchar(16)                            null,
	`date`          date                                   not null,
	`creation_date` timestamp                              default current_timestamp() not null on update current_timestamp(),
	`type`          enum ('Simple', 'Substitute', 'Guest') not null,
	`user_id`       int unsigned                           null default null,
	`warehouse_id`  int unsigned                           not null,
	`comment`       text                                   null,
	constraint `FK_over_norm_docs_user`
	foreign key (`user_id`) references `users` (`id`)
	                                                                                            on update cascade on delete set null,
	constraint `FK_over_norm_docs_warehouse`
	foreign key (`warehouse_id`) references `warehouse` (`id`)
	                                                                                            on update cascade on delete no action
	)
	ENGINE = InnoDB
	DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

create table `stock_over_norm_items`
(
	`id`           int unsigned not null auto_increment primary key,
	`document_id`  int unsigned not null,
	`over_norm_operation_id` int unsigned not null,
	constraint `FK_over_norm_document`
	foreign key (`document_id`) references `stock_over_norms` (`id`)
	on update cascade on delete cascade ,
	constraint `FK_op_over_norm`
	foreign key (`over_norm_operation_id`) references `operation_over_norm` (`id`)
	on update cascade on delete cascade
	)
	ENGINE = InnoDB
	DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Документ маркировки

-- -----------------------------------------------------
-- Table `stock_barcoding`
-- -----------------------------------------------------
CREATE TABLE `stock_barcoding`
(
	`id`            INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`doc_number`    VARCHAR(16)  NULL DEFAULT NULL,
	`date`          DATE         NOT NULL,
	`creation_date` DATETIME     NULL DEFAULT NULL,
	`user_id`       INT UNSIGNED NULL DEFAULT NULL,
	`comment`		text 		 null,
	`warehouse_id`  int unsigned not null,
	PRIMARY KEY (`id`),
	INDEX `index_stock_inspection_date` (`date` ASC),
	CONSTRAINT `stock_barcoding_fk_users`
	FOREIGN KEY (`user_id`)
	REFERENCES `users` (`id`)
	ON DELETE NO ACTION
	ON UPDATE NO ACTION,
	CONSTRAINT `stock_barcoding_fk_warehouse`
	FOREIGN KEY (`warehouse_id`)
	REFERENCES `warehouse` (`id`)
	ON DELETE NO ACTION
	ON UPDATE cascade
	)
	ENGINE = InnoDB
	DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;


-- -----------------------------------------------------
-- Table `stock_barcoding_items`
-- -----------------------------------------------------
CREATE TABLE `stock_barcoding_items`
(
	`id`                 		INT UNSIGNED NOT NULL AUTO_INCREMENT,
	`stock_barcoding_id` 		INT UNSIGNED NOT NULL,
	`operation_expense_id`      int unsigned not null,
	`operation_receipt_id`      int unsigned not null,
	PRIMARY KEY (`id`),
	CONSTRAINT `stock_barcoding_items_fk_doc`
	FOREIGN KEY (`stock_barcoding_id`)
	REFERENCES `stock_barcoding` (`id`)
	ON DELETE CASCADE
	ON UPDATE CASCADE,
	CONSTRAINT `stock_barcoding_items_fk_op_expense`
	FOREIGN KEY (`operation_expense_id`)
	REFERENCES `operation_warehouse` (`id`)
	ON DELETE CASCADE
	ON UPDATE CASCADE,
	CONSTRAINT `stock_barcoding_items_fk_op_receipt`
	FOREIGN KEY (`operation_receipt_id`)
	REFERENCES `operation_warehouse` (`id`)
	ON DELETE CASCADE
	ON UPDATE CASCADE
	)
	ENGINE = InnoDB
	DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- Возврат с обслуживания
ALTER TABLE stock_return_items
	ADD COLUMN claim_id int(10) unsigned NULL DEFAULT NULL AFTER duty_norm_issue_operation_id;

ALTER TABLE stock_return_items
	ADD CONSTRAINT stock_return_items_claim_id_fk FOREIGN KEY (claim_id) REFERENCES clothing_service_claim(id)
		ON UPDATE NO ACTION
		ON DELETE NO ACTION;

CREATE INDEX stock_return_items_claim_id_index
	ON stock_return_items(claim_id ASC);
