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

/---------------------Cascade sales detail---------------------/

ALTER TABLE [dbo].[sale_detail] DROP CONSTRAINT [fk_sale_detail_purchase];GOALTER TABLE [dbo].[sale_detail] WITH CHECKADD CONSTRAINT [fk_sale_detail_purchase]    FOREIGN KEY([purchase_id])    REFERENCES [dbo].[purchase] ([purchase_id])    ON DELETE CASCADE;GO


/---------------------Phone field user---------------------/

ALTER TABLE users
ADD telefono VARCHAR(20) NULL,
    telefono_personal VARCHAR(20) NULL;

   WITH cte AS (
    SELECT 
        user_id,
        ROW_NUMBER() OVER (ORDER BY user_id) - 1 AS rn
    FROM users
)
UPDATE u
SET telefono = CAST(11111111 + c.rn AS VARCHAR(20))
FROM users u
INNER JOIN cte c ON u.user_id = c.user_id;

WITH cte AS (
    SELECT 
        user_id,
        ROW_NUMBER() OVER (ORDER BY user_id) - 1 AS rn
    FROM users
)
UPDATE u
SET telefono_personal = CAST(21111111 + c.rn AS VARCHAR(20))
FROM users u
INNER JOIN cte c ON u.user_id = c.user_id;

/------------------------Add purchase order number-------------------------/

BEGIN TRANSACTION;
ALTER TABLE [purchase] ADD [purchase_ordernumber] nvarchar(50) NOT NULL DEFAULT N'';

CREATE INDEX [purchase_ordernumber] ON [purchase] ([purchase_ordernumber]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260228161942_AddPONumberField', N'9.0.11');

COMMIT;
GO

/------------------------Add payment method-------------------------/
BEGIN TRANSACTION;
ALTER TABLE [purchase] ADD [purchase_paymentmethod] varchar(20) NOT NULL DEFAULT '';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260306163717_AddPaymentMethodToPurchases', N'9.0.11');

COMMIT;
GO

/------------------------Routes module-------------------------/

CREATE TABLE delivery_route (
    route_id INT IDENTITY(1,1) PRIMARY KEY,
    route_name VARCHAR(150) NOT NULL,
    route_date DATE NOT NULL,
    driver_user_id INT NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Draft',
    start_at_planned DATETIME2 NULL,
    end_at_estimated DATETIME2 NULL,
    estimated_duration_minutes INT NULL,
    estimated_distance_km DECIMAL(10,2) NULL,
    polyline NVARCHAR(MAX) NULL,
    notes VARCHAR(500) NULL,
    created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 NULL,
    CONSTRAINT fk_delivery_route_user
        FOREIGN KEY (driver_user_id) REFERENCES users(user_id)
);

CREATE TABLE delivery_route_stop (
    route_stop_id INT IDENTITY(1,1) PRIMARY KEY,
    route_id INT NOT NULL,
    client_id VARCHAR(20) NOT NULL,
    stop_order INT NOT NULL,
    planned_arrival DATETIME2 NULL,
    estimated_travel_minutes_from_previous INT NULL,
    latitude DECIMAL(9,6) NOT NULL,
    longitude DECIMAL(9,6) NOT NULL,
    address_snapshot VARCHAR(500) NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    delivered_at DATETIME2 NULL,
    notes VARCHAR(500) NULL,
    CONSTRAINT fk_delivery_route_stop_route
        FOREIGN KEY (route_id) REFERENCES delivery_route(route_id) ON DELETE CASCADE,
    CONSTRAINT fk_delivery_route_stop_client
        FOREIGN KEY (client_id) REFERENCES clients(client_id)
);

--Despues de meter lo anterior 

ALTER TABLE delivery_route_stop
ADD client_name_snapshot VARCHAR(150) NOT NULL CONSTRAINT DF_delivery_route_stop_client_name_snapshot DEFAULT '';

ALTER TABLE delivery_route_stop
ADD created_at DATETIME2 NOT NULL CONSTRAINT DF_delivery_route_stop_created_at DEFAULT SYSUTCDATETIME();

ALTER TABLE delivery_route_stop
ADD updated_at DATETIME2 NULL;

/------------------------Add routes to sales-------------------------/

BEGIN TRANSACTION;
ALTER TABLE [purchase] ADD [route_id] int NOT NULL DEFAULT 0;

CREATE INDEX [IX_purchase_route_id] ON [purchase] ([route_id]);

ALTER TABLE [purchase] ADD CONSTRAINT [FK_purchase_delivery_route_route_id] FOREIGN KEY ([route_id]) REFERENCES [delivery_route] ([route_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260312035839_AddRoutesToSales', N'9.0.11');

COMMIT;
GO

/------------------------Add credit accounts-------------------------/

BEGIN TRANSACTION;
CREATE TABLE [client_accounts] (
    [clientaccount_id] int NOT NULL IDENTITY,
    [client_id] varchar(20) NOT NULL,
    [user_id] int NOT NULL,
    [clientaccount_number] nvarchar(50) NOT NULL,
    [clientaccount_openingdate] datetime2 NOT NULL,
    [clientaccount_creditlimit] decimal(18,2) NOT NULL,
    [clientaccount_interestrate] decimal(18,2) NOT NULL,
    [clientaccount_currentbalance] decimal(18,2) NOT NULL,
    [clientaccount_accountstatus] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_client_accounts] PRIMARY KEY ([clientaccount_id]),
    CONSTRAINT [FK_client_accounts_clients_client_id] FOREIGN KEY ([client_id]) REFERENCES [clients] ([client_id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_client_accounts_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE NO ACTION
);

CREATE TABLE [client_accountmovements] (
    [clientaccountmovement_id] int NOT NULL IDENTITY,
    [clientaccount_id] int NOT NULL,
    [clientaccountmovement_movementdate] datetime2 NOT NULL,
    [clientaccountmovement_description] varchar(max) NOT NULL,
    [clientaccountmovement_amount] decimal(18,2) NOT NULL,
    [clientaccountmovement_newbalance] decimal(18,2) NOT NULL,
    [clientaccountmovement_type] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_client_accountmovements] PRIMARY KEY ([clientaccountmovement_id]),
    CONSTRAINT [FK_client_accountmovements_client_accounts_clientaccount_id] FOREIGN KEY ([clientaccount_id]) REFERENCES [client_accounts] ([clientaccount_id]) ON DELETE CASCADE
);

CREATE INDEX [IX_client_accountmovements_clientaccount_id] ON [client_accountmovements] ([clientaccount_id]);

CREATE INDEX [clientaccount_number] ON [client_accounts] ([clientaccount_number]);

CREATE INDEX [IX_client_accounts_client_id] ON [client_accounts] ([client_id]);

CREATE INDEX [IX_client_accounts_user_id] ON [client_accounts] ([user_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260315031656_AddCreditAccounts', N'9.0.11');

COMMIT;
GO



/------------------------Add conditions to credit accounts-------------------------/

BEGIN TRANSACTION;
ALTER TABLE [client_accounts] ADD [clientaccount_conditions] varchar(max) NOT NULL DEFAULT '';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260317032323_AddConditionsToCreditAccounts', N'9.0.11');

COMMIT;
GO

/------------------------Add credit accounts to purchases-------------------------/

BEGIN TRANSACTION;
ALTER TABLE [purchase] ADD [clientaccount_id] int NULL;

CREATE INDEX [IX_purchase_clientaccount_id] ON [purchase] ([clientaccount_id]);

ALTER TABLE [purchase] ADD CONSTRAINT [FK_purchase_client_accounts_clientaccount_id] FOREIGN KEY ([clientaccount_id]) REFERENCES [client_accounts] ([clientaccount_id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260317222633_AddClientAccountToPurchases', N'9.0.11');

COMMIT;
GO

/------------------------Add old balance to credit accounts-------------------------/

BEGIN TRANSACTION;
ALTER TABLE [client_accountmovements] ADD [clientaccountmovement_oldbalance] decimal(18,2) NOT NULL DEFAULT 0.0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260320003055_AddOldBalanceForAccounts', N'9.0.11');

COMMIT;
GO

