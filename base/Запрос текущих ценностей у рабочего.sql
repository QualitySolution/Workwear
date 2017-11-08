set @id = 1;

SELECT stock_expense_detail.id as idin, stock_expense_detail.nomenclature_id, stock_expense_detail.quantity as quantityin,
 nomenclature.name, units.name as unit, stock_expense.date as datein, stock_income_detail.life_percent as lifein, stock_income.number,
spent.*  FROM stock_expense_detail 
LEFT JOIN 
(SELECT stock_income_detail.stock_expense_detail_id as idin, stock_income_detail.id as income_id, NULL as write_off_id, stock_income_detail.quantity as count, stock_income.date as dateout, stock_income_detail.life_percent as lifeout  FROM stock_income_detail
LEFT JOIN stock_income ON stock_income.id = stock_income_detail.stock_income_id WHERE stock_expense_detail_id IS NOT NULL
UNION ALL
SELECT stock_write_off_detail.stock_expense_detail_id as idin, NULL as income_id, stock_write_off_detail.id as write_off_id, stock_write_off_detail.quantity as count, stock_write_off.date as dateout, NULL as lifeout FROM stock_write_off_detail 
LEFT JOIN stock_write_off ON stock_write_off_detail.stock_write_off_id = stock_write_off.id
WHERE stock_expense_detail_id IS NOT NULL
) as spent ON spent.idin = stock_expense_detail.id 
LEFT JOIN nomenclature ON nomenclature.id = stock_expense_detail.nomenclature_id
LEFT JOIN units ON nomenclature.units_id = units.id 
LEFT JOIN stock_expense ON stock_expense.id = stock_expense_detail.stock_expense_id 
LEFT JOIN stock_income_detail ON stock_income_detail.id = stock_expense_detail.stock_income_detail_id
LEFT JOIN stock_income ON stock_income.id = stock_income_detail.stock_income_id
WHERE stock_expense.wear_card_id = @id AND (spent.count IS NULL OR spent.count < stock_expense_detail.quantity )