﻿-------- ORIGINAL -----------

SELECT * 
FROM [X] 
ORDER BY (
SELECT 0) OFFSET 4 ROWS

----------- RAW -------------

SELECT * 
FROM [X] 
ORDER BY (
SELECT 0) OFFSET ? ROWS

--------PARAMETRIZED --------

SELECT * 
FROM [X] 
ORDER BY (
SELECT 0) OFFSET @p0 ROWS