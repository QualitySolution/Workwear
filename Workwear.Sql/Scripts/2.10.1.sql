-- Права на вход в настройки учета

ALTER TABLE users
	ADD COLUMN can_accounting_settings TINYINT(1) NOT NULL DEFAULT 1 AFTER can_delete;
