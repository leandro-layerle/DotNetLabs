DECLARE @p int = 999980;
DECLARE @p1 int = 20;

SELECT [o].[Id], [o].[Number]
FROM [Orders] AS [o]
ORDER BY [o].[Id]
OFFSET @p ROWS
FETCH NEXT @p1 ROWS ONLY;
