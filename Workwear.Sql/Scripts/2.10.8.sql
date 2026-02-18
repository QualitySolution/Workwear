-- Добавление комментария в подразделения
ALTER TABLE subdivisions ADD COLUMN comment TEXT NULL DEFAULT NULL;

-- Замена периода Смена на Месяц в нормах
UPDATE norms_item SET period_type = 'Month' WHERE period_type = 'Shift';
