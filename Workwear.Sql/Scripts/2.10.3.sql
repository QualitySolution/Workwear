-- Создаем пропущенные индексы для снижения нагрузки на ЦП сервиса постоматов
ALTER TABLE `clothing_service_states` ADD INDEX(`operation_time`);