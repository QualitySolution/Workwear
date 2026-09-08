-- Разрешаем приём на обслуживание спецодежды, числящейся не на сотруднике.
ALTER TABLE clothing_service_claim
	MODIFY employee_id int UNSIGNED NULL;
