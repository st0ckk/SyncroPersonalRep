-- =============================================
-- US-SPU-003 + US-SPU-LOGS: Tabla de auditoría
-- Ejecutar en la BD [syncro]
-- =============================================
USE [syncro]
GO

CREATE TABLE [dbo].[audit_log](
    [log_id]        [bigint] IDENTITY(1,1) NOT NULL,
    [entity_type]   [varchar](100)  NOT NULL,       -- "Purchase", "User", "Product", etc.
    [entity_id]     [varchar](50)   NOT NULL,        -- PK de la entidad afectada
    [action]        [varchar](50)   NOT NULL,        -- "SALE_CREATED", "LOGIN_SUCCESS", etc.
    [user_id]       [int]           NOT NULL,        -- quién realizó la acción
    [details]       [nvarchar](max) NULL,            -- info adicional / payload resumido
    [ip_address]    [varchar](45)   NULL,            -- IPv4 o IPv6
    [created_at]    [datetime2](7)  NOT NULL DEFAULT GETUTCDATE(),
 CONSTRAINT [PK_audit_log] PRIMARY KEY CLUSTERED
(
    [log_id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
        ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Índices para consultas frecuentes
CREATE NONCLUSTERED INDEX [IX_audit_log_entity] ON [dbo].[audit_log]([entity_type], [entity_id])
GO

CREATE NONCLUSTERED INDEX [IX_audit_log_user] ON [dbo].[audit_log]([user_id])
GO

CREATE NONCLUSTERED INDEX [IX_audit_log_action] ON [dbo].[audit_log]([action])
GO

CREATE NONCLUSTERED INDEX [IX_audit_log_created] ON [dbo].[audit_log]([created_at] DESC)
GO
