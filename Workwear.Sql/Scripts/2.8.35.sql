-- Для исправления падения новой версии оборотной стороны карточки сотрудника
ALTER TABLE issuance_sheet CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
