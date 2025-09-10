-- Создание новой функции, которая корректно считает количество, необходимое к выдаче
DELIMITER $$
CREATE FUNCTION `quantity_issue`(
	`amount` INT UNSIGNED,
	`norm_period` INT UNSIGNED,
	`next_issue` DATE,
	`begin_date` DATE,
	`end_date` DATE,
	`begin_Issue_Period` DATE,
	`end_Issue_Period` DATE)
	RETURNS int(10) unsigned
    NO SQL
    DETERMINISTIC
    COMMENT 'Функция рассчитывает количество, необходимое к выдаче.'
BEGIN
    DECLARE issue_count INT;
    DECLARE next_issue_new DATE;
    DECLARE begin_Issue_Period_New DATE;
    DECLARE end_Issue_Period_New DATE;
    DECLARE start_Of_Year DATE;
    DECLARE end_Of_Year DATE;

    IF norm_period <= 0 THEN RETURN 0; END IF;
    IF next_issue IS NULL THEN RETURN 0; END IF;

    SET issue_count = 0;
    SET next_issue_new = CONCAT('2000', '-', MONTH(next_issue), '-', DAY(next_issue));
    SET begin_Issue_Period_New = CONCAT('2000', '-', MONTH(begin_Issue_Period), '-', DAY(begin_Issue_Period));
    SET end_Issue_Period_New = CONCAT('2000', '-', MONTH(end_Issue_Period), '-', DAY(end_Issue_Period));
    SET start_Of_Year = CONCAT('2000', '-', 1, '-',  1);
    SET end_Of_Year = CONCAT('2000', '-', 12 , '-', 31);

    WHILE next_issue <= end_date DO
            IF begin_Issue_Period IS NOT NULL THEN
                IF (next_issue_new  BETWEEN begin_Issue_Period_New AND end_Issue_Period_New)
                    OR ((next_issue_new BETWEEN begin_Issue_Period_New AND end_Of_Year OR next_issue_new BETWEEN start_Of_Year AND end_Issue_Period_New)
                        AND (MONTH(begin_Issue_Period) > MONTH(end_Issue_Period))) THEN
                    IF next_issue >= begin_date THEN
                        SET issue_count = issue_count + amount;
					END IF;
                    SET next_issue = DATE_ADD(next_issue, INTERVAL norm_period MONTH);
				END IF;
                IF (next_issue_new BETWEEN (DATE_ADD(end_Issue_Period_New, INTERVAL 1 DAY)) AND (DATE_ADD(begin_Issue_Period_New, INTERVAL -1 DAY)))
                    OR ((next_issue_new < begin_Issue_Period_New OR next_issue_new > end_Issue_Period_New) AND (MONTH(begin_Issue_Period) <= MONTH(end_Issue_Period))) THEN
                    SET next_issue = CONCAT(YEAR(next_issue), '-', MONTH(begin_Issue_Period), '-', DAY(begin_Issue_Period));
                    IF (next_issue_new > end_Issue_Period_New) AND (MONTH(begin_Issue_Period) <= MONTH(end_Issue_Period)) THEN
                        SET next_issue = DATE_ADD(next_issue, INTERVAL 1 YEAR);
					END IF;
				END IF;
			ELSE
                IF next_issue >= begin_date THEN
                    SET issue_count = issue_count + amount;
				END IF;
                SET next_issue = DATE_ADD(next_issue, INTERVAL norm_period MONTH);
			END IF;
            SET next_issue_new = CONCAT('2000', '-', MONTH(next_issue), '-', DAY(next_issue));
	END WHILE;
	RETURN issue_count;
END$$

DELIMITER ;
