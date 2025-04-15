ALTER TABLE stock_return_items
    ADD COLUMN claim_id int(10) unsigned NULL DEFAULT NULL AFTER duty_norm_issue_operation_id;
ALTER TABLE stock_return_items
    ADD CONSTRAINT stock_return_items_claim_id_fk FOREIGN KEY (claim_id) REFERENCES clothing_service_claim(id)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION;
CREATE INDEX stock_return_items_claim_id_index
    ON stock_return_items(claim_id ASC);
