-- Заполняем названия норма старыми данными
UPDATE `norms` SET `name`= CONCAT_WS(' ', CONCAT('ТОН №', `ton_number`), concat('прил. ', `ton_attachment`), CONCAT('п. ', `ton_paragraph`))