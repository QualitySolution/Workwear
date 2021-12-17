-- Удаляем пустые строки из размеров и роста в операциях. Это приводит к неоднозначности в запросах
UPDATE `operation_warehouse` SET `size`= NULL WHERE size = "";
UPDATE `operation_warehouse` SET `growth`= NULL WHERE growth = "";
UPDATE `operation_issued_by_employee` SET `size`= NULL WHERE size = "";
UPDATE `operation_issued_by_employee` SET `growth`= NULL WHERE growth = ""; 
