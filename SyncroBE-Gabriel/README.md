Este código maneja el backend de Syncro.

Trabaja por medio de 4 capas
  1. API
  2. Application
  3. Domain
  4. Infrastructure

El llamado de API se puede probar desde Postman o directamente en el swagger.

Convención para base de datos: snake_case

Base de datos:

Se debe de crear todas las tablas y una vez creada "dbo.stg_division_territorial" se debe de poblar por medio del cvs "DTA-TABLA POR PROVINCIA-CANTÓN-DISTRITO 2025 - Copy of DTA OFICIALIZACIÓN" 
Esta tabla sirve para poblar todas las provincias, cantones y distritos de CR, que posteriormente se utilizarán para la facturación electrónica

Recordar modificar "**appsettings.json**" e incluir su conexion a base de datos, para eso podemos editar el git.ignore o esperar a publicar una BD en la nube.


/---------------------Comandos de EF---------------------/

//Quotes y QuoteDetails

Add-Migration AddQuoteTables -Project SyncroBE.Infrastructure -StartupProject SyncroBE.API -Context SyncroDbContext

Update-Database AddQuoteTables -Project SyncroBE.Infrastructure -StartupProject SyncroBE.API -Context SyncroDbContext


/---------------------Agregar QuoteStatus a Quotes---------------------/

ALTER TABLE Quotes
ADD quote_status NVARCHAR(50) NOT NULL DEFAULT 'pending'


/---------------------Cambiar fechas a datetime2 en Quotes---------------------/

--Cambiar QuoteValidDate a datetime2
ALTER TABLE Quotes
ADD QuoteValidDate_Temp DATETIME2(7)

UPDATE Quotes
SET QuoteValidDate_Temp = CAST(quote_validdate AS DATETIME2(7))

ALTER TABLE Quotes
DROP COLUMN quote_validdate


EXEC sp_rename 'Quotes.QuoteValidDate_Temp', 'quote_validdate', 'COLUMN'


--Cambiar QuoteDate a datetime2
ALTER TABLE Quotes
ADD QuoteDate_Temp DATETIME2(7)

UPDATE Quotes
SET QuoteDate_Temp = CAST(quote_date AS DATETIME2(7))

ALTER TABLE Quotes
DROP COLUMN quote_date

EXEC sp_rename 'Quotes.QuoteDate_Temp', 'quote_date', 'COLUMN'

//Employee schedule 

Agregar migración: 

dotnet ef migrations add AddEmployeeSchedule
dotnet ef database update

/---------------------Descuentos y Quote Descuentos---------------------/

BEGIN TRANSACTION;
ALTER TABLE [quotes] ADD [discount_id] int NULL;

ALTER TABLE [quotes] ADD [quote_discountapplied] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [quotes] ADD [quote_discountpercentage] int NOT NULL DEFAULT 0;

ALTER TABLE [quotes] ADD [quote_discountreason] varchar(max) NOT NULL DEFAULT '';

CREATE TABLE [discount] (
    [discount_id] int NOT NULL IDENTITY,
    [discount_name] nvarchar(150) NOT NULL,
    [discount_percentage] int NOT NULL,
    [is_active] bit NOT NULL,
    CONSTRAINT [PK_discount] PRIMARY KEY ([discount_id])
);

CREATE INDEX [IX_quotes_discount_id] ON [quotes] ([discount_id]);

ALTER TABLE [quotes] ADD CONSTRAINT [FK_quotes_discount_discount_id] FOREIGN KEY ([discount_id]) REFERENCES [discount] ([discount_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260220152918_AddDiscountTables', N'9.0.11');

COMMIT;
GO
