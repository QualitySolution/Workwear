-- Удаление функции, которая считает количество, необходимое к выдаче
DROP FUNCTION IF EXISTS count_issue;

-- Удаление таблицы work_days, которая больше не используется
DROP TABLE work_days;

-- Замена периода Смена на Месяц в нормах
UPDATE norms_item SET period_type = 'Month' WHERE period_type = 'Shift';

-- Удаление из нормы типа периода Смена
ALTER TABLE norms_item CHANGE period_type period_type ENUM ('Year', 'Month', 'Wearout', 'Duty') NOT NULL DEFAULT 'Year';
