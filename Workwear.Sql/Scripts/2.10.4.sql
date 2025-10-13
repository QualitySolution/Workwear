
-- информация о окнах
CREATE TABLE visit_windows
(
    id   INT UNSIGNED AUTO_INCREMENT
        PRIMARY KEY,
    name CHAR(32) NULL
)
    COMMENT 'информация о окнах';

-- изменяем записи посещений на новый формат
ALTER TABLE `visits`
    ADD COLUMN `service_type` ENUM('GiveWear','NewEmployee','Unidentified','Dismiss','GiveReport','WriteOff','ClothingService','Appeal') NOT NULL DEFAULT 'Unidentified' AFTER `employee_id`,
    ADD COLUMN `create_from_lk` TINYINT(1) NOT NULL DEFAULT 1 AFTER `employee_create`,
    ADD COLUMN `status` ENUM('New','Queued','Serviced','Done','Canceled','Missing') NOT NULL DEFAULT 'New' AFTER `done`,
    ADD COLUMN `ticket_number` CHAR(4) NULL DEFAULT '' COMMENT 'Талончик в очереди' AFTER `status`,
    ADD COLUMN `window_id` INT(10) UNSIGNED DEFAULT NULL COMMENT 'ID окна обслуживания' AFTER `ticket_number`,
    ADD COLUMN `time_entry` DATETIME DEFAULT NULL COMMENT 'Время постановки в очередь на ПВ' AFTER `window_id`,
    ADD COLUMN `time_start` DATETIME DEFAULT NULL COMMENT 'Начало обслуживания (перво посещение окна)' AFTER `time_entry`,
    ADD COLUMN `time_finish` DATETIME DEFAULT NULL COMMENT 'Завершение визита' AFTER `time_start`;

-- синхронизируем значениями из employee_create для уже существующих записей
UPDATE `visits` SET `create_from_lk` = `employee_create`;

ALTER TABLE `visits` ADD INDEX `fk_visits_window_id_idx` (`window_id`);

ALTER TABLE `visits` ADD CONSTRAINT `fk_visits_window_id` FOREIGN KEY (`window_id`) REFERENCES `visit_windows` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;


-- записи какой юзер в каком окне, состояние окна
CREATE TABLE visits_users_log
(
    id        INT UNSIGNED AUTO_INCREMENT
        PRIMARY KEY,
    user_id   INT UNSIGNED                                                                        NOT NULL,
    window_id INT UNSIGNED                                                                        NULL,
    visit_id  INT UNSIGNED                                                                        NULL,
    tiket     CHAR(4)                                                                             NULL,
    `time`    DATETIME                                                                            NULL,
    `type`    ENUM ('WindowStart', 'WindowFinish', 'WindowTimeout', 'StartService', 'FinishService', 'ReRouteService', 'WindowWaiting') NOT NULL COMMENT 'Типы действия',
    comment   CHAR(64)                                                                            NULL,
    CONSTRAINT visits_user_users_id_fk
        FOREIGN KEY (user_id) REFERENCES users (id)
            ON UPDATE CASCADE ON DELETE CASCADE,
    CONSTRAINT visits_user_visit_windows_id_fk
        FOREIGN KEY (window_id) REFERENCES visit_windows (id)
            ON UPDATE CASCADE ON DELETE SET NULL,
    CONSTRAINT visits_users_log_visits_id_fk
        FOREIGN KEY (visit_id) REFERENCES visits (id)
)
    COMMENT 'записи какой юзер в каком окне, состояние окна';

CREATE INDEX visits_user_users_id_fk_idx ON visits_users_log (user_id);
CREATE INDEX visits_user_visit_windows_id_fk_idx ON visits_users_log (window_id);
CREATE INDEX visits_users_log_visits_id_fk_idx ON visits_users_log (visit_id);