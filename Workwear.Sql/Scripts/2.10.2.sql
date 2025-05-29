-- Черновик документа выдачи
alter table stock_expense
	add issue_date date DEFAULT date after date;
