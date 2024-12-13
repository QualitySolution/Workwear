-- добавление типа размер футболки
INSERT INTO size_types VALUES(13,'Размер футболки',1,'size',13);

-- добавление значений размеров футболок
INSERT INTO sizes VALUES(370, '3XS', 13,1,1,'40'),
						(371, '2XS', 13,1,1,'42'),
						(372, 'XS', 13,1,1,'44'),
						(373, 'S', 13,1,1,'46'),
						(374, 'M', 13,1,1,'48'),
						(375, 'L', 13,1,1,'50'),
						(376, 'XL', 13,1,1,'52'),
						(377, '2XL', 13,1,1,'54'),
						(378, '3XL', 13,1,1,'56'),
						(379, '4XL', 13,1,1,'58'),
						(380, '5XL', 13,1,1,'60'),
						(381, '6XL', 13,1,1,'62'),
						(382, '7XL', 13,1,1,'64'),
						(383, '8XL', 13,1,1,'66'),
						(384, '9XL', 13,1,1,'68'),
						(385, '10XL', 13,1,1,'70'),
						(386, '11XL', 13,1,1,'72'),
						(387, '12XL', 13,1,1,'74'),
						(388, '13XL', 13,1,1,'76'),
						(389, '14XL', 13,1,1,'78');

-- добавление размера футболок по размеру одежды сотрудника
INSERT INTO wear_cards_sizes(employee_id, size_type_id, size_id)
SELECT employee_id, 13, s2.id
FROM wear_cards_sizes
		 LEFT JOIN sizes s1 on wear_cards_sizes.size_id = s1.id
		 LEFT JOIN sizes s2 on s2.alternative_name=right(s1.name,2)
WHERE wear_cards_sizes.size_type_id=2 AND s2.size_type_id=13;
