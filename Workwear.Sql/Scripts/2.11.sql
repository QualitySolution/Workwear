-- Удаление функции, которая считает количество, необходимое к выдаче
DROP FUNCTION IF EXISTS count_issue;

-- Удаление таблицы work_days, которая больше не используется
DROP TABLE work_days;

/*
 Настройку "Разрешать выдачу раньше срока" разделили по типам выдачи: персональная (осталась ColDayAheadOfShedule)
 Нужно отработать в мапинге и раскомментировать
 UPDATE base_parameters SET name = 'ColDayAheadOfShedulePersonal' WHERE name = 'ColDayAheadOfShedule';
*/
