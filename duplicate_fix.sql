-- Исправленный скрипт для удаления дублей

START TRANSACTION;

-- Найти дублей по barcode_id
SELECT
    barcodes.title as barcode,
    clothing_service_claim.id as claim,
    nomenclature.name as nomenclature,
    clothing_service_states.operation_time as time,
    clothing_service_states.state as state,
    clothing_service_states.comment
FROM
    (SELECT barcode_id,
            COUNT(*) as c
     FROM clothing_service_claim
     WHERE NOT clothing_service_claim.is_closed
     GROUP BY barcode_id
     HAVING c > 1 ) as tab_double
        LEFT JOIN clothing_service_claim on (tab_double.barcode_id = clothing_service_claim.barcode_id AND NOT clothing_service_claim.is_closed)
        LEFT JOIN barcodes on clothing_service_claim.barcode_id = barcodes.id
        LEFT JOIN clothing_service_states on clothing_service_claim.id = clothing_service_states.claim_id
        LEFT JOIN nomenclature on barcodes.nomenclature_id = nomenclature.id
ORDER BY barcodes.id, clothing_service_claim.id, clothing_service_states.operation_time;

-- ИСПРАВЛЕННЫЙ UPDATE: объединение дублей в один claim
-- Сохраняем первый claim для каждого barcode_id, остальные переносят свои states
UPDATE clothing_service_states s 
INNER JOIN clothing_service_claim c1 ON s.claim_id = c1.id
SET s.claim_id = (
    SELECT s2.claim_id 
    FROM clothing_service_states s2
    INNER JOIN clothing_service_claim c2 ON s2.claim_id = c2.id
    WHERE c2.barcode_id = c1.barcode_id  -- ✅ ИСПРАВЛЕНО: было c2.barcode_id = c2.barcode_id
      AND s2.claim_id != s.claim_id
      AND NOT c1.is_closed
      AND NOT c2.is_closed
    ORDER BY c2.id ASC
    LIMIT 1
)
WHERE EXISTS (
    SELECT 1 FROM clothing_service_claim c2
    INNER JOIN clothing_service_states s2 ON c2.id = s2.claim_id
    WHERE c2.barcode_id = c1.barcode_id  -- ✅ ИСПРАВЛЕНО: было c2.barcode_id = c2.barcode_id
      AND s2.claim_id != s.claim_id
      AND NOT c1.is_closed
      AND NOT c2.is_closed
    LIMIT 1
);

ROLLBACK;

