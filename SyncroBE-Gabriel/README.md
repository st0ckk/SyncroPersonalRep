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