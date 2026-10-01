# 01 — Paginación en EF Core: Skip/Take vs Keyset

Laboratorio reproducible para `DotNetLabs` que compara paginación Offset (`Skip/Take`) y Keyset en EF Core 10 + SQL Server.

## Qué demuestra

1. **Consistencia:** si se elimina un registro ya visto entre dos requests, Offset puede omitir un registro posterior. En la prueba verificada, el usuario vio `1..5`, se eliminó `3` y la segunda página Offset devolvió `7..11`; el `6` nunca apareció. Keyset, usando cursor `5`, devolvió `6..10`.
2. **Páginas profundas:** con 1.000.000 de Orders, la página 50.000 (`Skip(999980).Take(20)`) produjo en la ejecución verificada un `Clustered Index Scan` que procesó 1.000.000 de filas para devolver 20. El equivalente Keyset (`Id > 999980`) produjo un `Clustered Index Seek` que procesó 20 filas para devolver 20.

## Stack

- .NET 10
- EF Core SQL Server 10.0.12
- ASP.NET Core Web API
- SQL Server Express
- Visual Studio 2026

## Ejecución

1. Abrir `PaginationLab.slnx`.
2. Verificar la connection string de `PaginationLab.Api/appsettings.json`.
3. Ejecutar la API.
4. Ejecutar `PaginationLab.Api/PaginationLab.Api.http` en orden para la prueba de consistencia.
5. Para performance, ejecutar `setup-performance.sql` una vez sobre `PaginationLabDb` y luego los dos endpoints `performance/*`.
6. Para reproducir los planes reales, activar **Include Actual Execution Plan** en SSMS y ejecutar `execution-plan-offset.sql` y `execution-plan-keyset.sql`.

## Importante

Los tiempos de `Stopwatch` se incluyen como observación, no como benchmark. En nuestras ejecuciones variaron mucho por warm-up/caché. La evidencia fuerte de la POC 2 son los planes reales incluidos en `/evidencias`.

Keyset no reemplaza siempre a Offset. Offset sigue siendo natural cuando se necesita salto aleatorio a una página específica. Keyset encaja especialmente bien en siguiente/anterior, cargar más, feeds e interfaces basadas en cursor.

El orden debe ser totalmente único. En un caso real ordenado por fecha, por ejemplo, conviene `OrderBy(CreatedAt).ThenBy(Id)` y conservar ambas claves en el cursor.
