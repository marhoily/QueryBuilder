﻿-------- ORIGINAL -----------

SELECT * FROM "X" ORDER BY (SELECT 0 FROM DUAL) OFFSET 0 ROWS FETCH NEXT 3 ROWS ONLY

----------- RAW -------------

SELECT * FROM "X" ORDER BY (SELECT 0 FROM DUAL) OFFSET ? ROWS FETCH NEXT ? ROWS ONLY

--------PARAMETRIZED --------

SELECT * FROM "X" ORDER BY (SELECT 0 FROM DUAL) OFFSET :p0 ROWS FETCH NEXT :p1 ROWS ONLY