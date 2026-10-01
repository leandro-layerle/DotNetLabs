DECLARE @p int = 20;
DECLARE @lastId int = 999980;

SELECT TOP(@p)
    [o].[Id],
    [o].[Number]
FROM [Orders] AS [o]
WHERE [o].[Id] > @lastId
ORDER BY [o].[Id];
