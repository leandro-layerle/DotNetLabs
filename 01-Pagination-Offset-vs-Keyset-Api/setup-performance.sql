SET NOCOUNT ON;

DELETE FROM Orders;
DBCC CHECKIDENT ('Orders', RESEED, 0);

;WITH
E1(N) AS
(
    SELECT 1
    FROM (VALUES
        (1),(1),(1),(1),(1),(1),(1),(1),(1),(1)
    ) AS X(N)
),
E2(N) AS
(
    SELECT 1
    FROM E1 A
    CROSS JOIN E1 B
),
E4(N) AS
(
    SELECT 1
    FROM E2 A
    CROSS JOIN E2 B
),
E6(N) AS
(
    SELECT 1
    FROM E4 A
    CROSS JOIN E2 B
),
Numbers AS
(
    SELECT TOP (1000000)
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
    FROM E6
)
INSERT INTO Orders (Number)
SELECT
    'ORD-' + RIGHT('0000000' + CAST(N AS varchar(7)), 7)
FROM Numbers;

SELECT
    COUNT(*) AS TotalOrders,
    MIN(Id) AS MinId,
    MAX(Id) AS MaxId
FROM Orders;
