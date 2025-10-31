-- Обновляем хранимую процедуру
DROP function IF EXISTS `count_issue`;

DELIMITER $$

CREATE FUNCTION `count_issue`(
    `amount` INT UNSIGNED, 
    `norm_period` INT UNSIGNED, 
    `next_issue` DATE, 
    `begin_date` DATE, 
    `end_date` DATE,
	`begin_Issue_Period` INT,
	`end_Issue_Period` INT) 
    RETURNS int(10) unsigned
    NO SQL
    DETERMINISTIC
    COMMENT 'Функция рассчитывает количество необходимое к выдачи.'
SQL SECURITY INVOKER	
BEGIN
DECLARE issue_count INT;

IF norm_period <= 0 THEN RETURN 0; END IF;
IF next_issue IS NULL THEN RETURN 0; END IF;

SET issue_count = 0;

WHILE next_issue <= end_date DO
    IF next_issue >= begin_date THEN 
    	SET issue_count = issue_count + amount;
    END IF;
  SET next_issue = DATE_ADD(next_issue, INTERVAL norm_period MONTH);
	IF begin_Issue_Period IS NOT NULL THEN
		IF begin_Issue_Period < end_Issue_Period THEN
			IF MONTH(next_issue) BETWEEN begin_Issue_Period AND end_Issue_Period THEN 
             SET next_issue = DATE_ADD(CONCAT(YEAR(next_issue) , '-', end_Issue_Period, '-01'), INTERVAL 1 MONTH);
            END IF; 
        ELSE
			IF MONTH(next_issue) BETWEEN begin_Issue_Period AND 12 THEN
				SET next_issue = DATE_ADD(CONCAT(YEAR(next_issue), '-', end_Issue_Period, '-01'), INTERVAL '1T1' YEAR_MONTH);
			ELSEIF MONTH(next_issue) BETWEEN 1 AND end_Issue_Period THEN
				SET next_issue = DATE_ADD(CONCAT(YEAR(next_issue), '-', end_Issue_Period, '-01'), INTERVAL 1 MONTH);
			END IF;
		END IF;
	END IF;
END WHILE;
RETURN issue_count;
END$$

DELIMITER ;
