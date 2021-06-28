-- Заполняем названия норма старыми данными
UPDATE `norms` SET `name`= CONCAT_WS(' ', CONCAT('ТОН №', `ton_number`), concat('прил. ', `ton_attachment`), CONCAT('п. ', `ton_paragraph`))

-- Очищаем старые параметры базы
DELETE FROM base_parameters WHERE name = 'micro_updates';
DELETE FROM base_parameters WHERE name = 'edition';