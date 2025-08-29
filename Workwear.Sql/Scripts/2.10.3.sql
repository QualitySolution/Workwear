DELIMITER $$
CREATE FUNCTION `count_working_days` (`start_date` DATE, `end_date` DATE)
    RETURNS INT
    DETERMINISTIC
    COMMENT 'Функция подсчитывает количество дней нахождения спецодежды на каждом этапе, исключая выходные дни'
BEGIN
    RETURN (WITH RECURSIVE date_range AS
                               (SELECT start_date as sd
                                UNION ALL
                                SELECT DATE_ADD(sd, INTERVAL 1 Day)
                                FROM date_range
                                WHERE DATE_ADD(sd, INTERVAL 1 Day) < end_date
                               )
            SELECT COUNT(*)
            FROM date_range
            WHERE WEEKDAY(sd) NOT IN (5, 6) AND start_date < end_date
    );
END $$;
DELIMITER ;
