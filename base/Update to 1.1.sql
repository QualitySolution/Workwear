## Переносим единицы измерения из номенклатуры в тип номенклатуры.
UPDATE item_types, nomenclature SET item_types.units_id = nomenclature.units_id WHERE item_types.id = nomenclature.type_id;