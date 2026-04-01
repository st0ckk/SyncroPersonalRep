-- ============================================================================
-- Electronic Invoice (Facturación Electrónica) - Costa Rica Hacienda
-- Schema changes for SyncroBE
-- ============================================================================
-- Run against your MSSQL database (syncro)
-- ============================================================================

BEGIN TRANSACTION;

-- ============================================================================
-- 1. COMPANY CONFIG (Emisor) - Business information for the invoice issuer
--    Credentials (ATV user/password, certificate PIN) go in appsettings.json
-- ============================================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'company_config')
BEGIN
    CREATE TABLE company_config (
        config_id           INT IDENTITY(1,1) PRIMARY KEY,
        company_name        VARCHAR(100)    NOT NULL,   -- Nombre del emisor
        commercial_name     VARCHAR(100)    NULL,       -- Nombre comercial (opcional)
        id_type             VARCHAR(2)      NOT NULL,   -- 01=Fisica, 02=Juridica, 03=DIMEX, 04=NITE
        id_number           VARCHAR(20)     NOT NULL,   -- Número de cédula/identificación
        activity_code       VARCHAR(6)      NOT NULL,   -- Código actividad económica (ej: 477300)
        province_code       INT             NOT NULL,
        canton_code         INT             NOT NULL,
        district_code       INT             NOT NULL,
        neighborhood_code   INT             NULL,       -- Barrio (opcional, default 01)
        other_address       VARCHAR(250)    NOT NULL,   -- OtrasSenas - dirección exacta
        phone_country_code  VARCHAR(3)      NOT NULL DEFAULT '506',
        phone_number        VARCHAR(20)     NULL,
        fax_country_code    VARCHAR(3)      NULL,
        fax_number          VARCHAR(20)     NULL,
        email               VARCHAR(160)    NOT NULL,
        branch_number       VARCHAR(3)      NOT NULL DEFAULT '001',   -- Sucursal
        terminal_number     VARCHAR(5)      NOT NULL DEFAULT '00001', -- Terminal
        environment         VARCHAR(10)     NOT NULL DEFAULT 'sandbox', -- 'sandbox' o 'production'
        is_active           BIT             NOT NULL DEFAULT 1,
        created_at          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
        updated_at          DATETIME2       NULL,

        -- FK to location tables
        CONSTRAINT fk_company_config_province FOREIGN KEY (province_code)
            REFERENCES province(province_code),
        CONSTRAINT fk_company_config_canton FOREIGN KEY (canton_code)
            REFERENCES canton(canton_code),
        CONSTRAINT fk_company_config_district FOREIGN KEY (district_code)
            REFERENCES district(district_code)
    );

    PRINT 'Created table: company_config';
END
GO

-- ============================================================================
-- 2. HACIENDA CONSECUTIVE - Sequential numbering per document type/branch
-- ============================================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'hacienda_consecutive')
BEGIN
    CREATE TABLE hacienda_consecutive (
        consecutive_id      INT IDENTITY(1,1) PRIMARY KEY,
        document_type       VARCHAR(2)      NOT NULL,   -- 01=FE, 02=ND, 03=NC, 04=TE, 08=FEC, 09=FEE
        branch_number       VARCHAR(3)      NOT NULL DEFAULT '001',
        terminal_number     VARCHAR(5)      NOT NULL DEFAULT '00001',
        last_number         BIGINT          NOT NULL DEFAULT 0,
        updated_at          DATETIME2       NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT uq_hacienda_consecutive UNIQUE (document_type, branch_number, terminal_number)
    );

    -- Seed initial rows for common document types
    INSERT INTO hacienda_consecutive (document_type, branch_number, terminal_number, last_number)
    VALUES
        ('01', '001', '00001', 0),  -- Factura Electrónica
        ('02', '001', '00001', 0),  -- Nota de Débito
        ('03', '001', '00001', 0),  -- Nota de Crédito
        ('04', '001', '00001', 0),  -- Tiquete Electrónico
        ('08', '001', '00001', 0),  -- Factura Electrónica de Compra
        ('09', '001', '00001', 0);  -- Factura Electrónica de Exportación

    PRINT 'Created table: hacienda_consecutive (with seed data)';
END
GO

-- ============================================================================
-- 3. ALTER EXISTING INVOICE TABLE - Add electronic invoice fields
--    Current columns: invoice_id, purchase_id, electronic_invoice, invoice_total, invoice_date
-- ============================================================================

-- 3a. Clave numérica (50 dígitos, identificador único del comprobante)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'clave')
BEGIN
    ALTER TABLE invoice ADD clave VARCHAR(50) NULL;
    PRINT 'Added column: invoice.clave';
END
GO

-- 3b. Número consecutivo (20 dígitos)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'consecutive_number')
BEGIN
    ALTER TABLE invoice ADD consecutive_number VARCHAR(20) NULL;
    PRINT 'Added column: invoice.consecutive_number';
END
GO

-- 3c. Tipo de documento
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'document_type')
BEGIN
    ALTER TABLE invoice ADD document_type VARCHAR(2) NULL DEFAULT '01';
    PRINT 'Added column: invoice.document_type';
END
GO

-- 3d. Estado en Hacienda
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'hacienda_status')
BEGIN
    ALTER TABLE invoice ADD hacienda_status VARCHAR(20) NULL DEFAULT 'pending';
    -- Values: pending, sent, accepted, rejected, error
    PRINT 'Added column: invoice.hacienda_status';
END
GO

-- 3e. XML firmado (el documento completo enviado a Hacienda)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'xml_signed')
BEGIN
    ALTER TABLE invoice ADD xml_signed NVARCHAR(MAX) NULL;
    PRINT 'Added column: invoice.xml_signed';
END
GO

-- 3f. XML de respuesta de Hacienda
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'xml_response')
BEGIN
    ALTER TABLE invoice ADD xml_response NVARCHAR(MAX) NULL;
    PRINT 'Added column: invoice.xml_response';
END
GO

-- 3g. Mensaje de Hacienda (resumen de respuesta)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'hacienda_message')
BEGIN
    ALTER TABLE invoice ADD hacienda_message NVARCHAR(500) NULL;
    PRINT 'Added column: invoice.hacienda_message';
END
GO

-- 3h. Fecha de emisión (la que va en el XML)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'emission_date')
BEGIN
    ALTER TABLE invoice ADD emission_date DATETIME2 NULL;
    PRINT 'Added column: invoice.emission_date';
END
GO

-- 3i. Fecha de envío a Hacienda
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'sent_at')
BEGIN
    ALTER TABLE invoice ADD sent_at DATETIME2 NULL;
    PRINT 'Added column: invoice.sent_at';
END
GO

-- 3j. Fecha de respuesta de Hacienda
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'response_at')
BEGIN
    ALTER TABLE invoice ADD response_at DATETIME2 NULL;
    PRINT 'Added column: invoice.response_at';
END
GO

-- 3k. Moneda
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'currency_code')
BEGIN
    ALTER TABLE invoice ADD currency_code VARCHAR(3) NULL DEFAULT 'CRC';
    PRINT 'Added column: invoice.currency_code';
END
GO

-- 3l. Tipo de cambio
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'exchange_rate')
BEGIN
    ALTER TABLE invoice ADD exchange_rate DECIMAL(18,5) NULL DEFAULT 1;
    PRINT 'Added column: invoice.exchange_rate';
END
GO

-- 3m. Condición de venta (01=Contado, 02=Crédito, etc.)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'sale_condition')
BEGIN
    ALTER TABLE invoice ADD sale_condition VARCHAR(2) NULL DEFAULT '01';
    PRINT 'Added column: invoice.sale_condition';
END
GO

-- 3n. Medio de pago Hacienda (01=Efectivo, 02=Tarjeta, etc.)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'payment_method_code')
BEGIN
    ALTER TABLE invoice ADD payment_method_code VARCHAR(2) NULL DEFAULT '01';
    PRINT 'Added column: invoice.payment_method_code';
END
GO

-- 3o. Referencia a documento original (para NC/ND)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'reference_document_clave')
BEGIN
    ALTER TABLE invoice ADD reference_document_clave VARCHAR(50) NULL;
    PRINT 'Added column: invoice.reference_document_clave';
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'reference_code')
BEGIN
    ALTER TABLE invoice ADD reference_code VARCHAR(2) NULL;
    -- 01=Anula, 02=Corrige texto, 04=Referencia otro doc, 05=Sustituye
    PRINT 'Added column: invoice.reference_code';
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'reference_reason')
BEGIN
    ALTER TABLE invoice ADD reference_reason VARCHAR(180) NULL;
    PRINT 'Added column: invoice.reference_reason';
END
GO

-- 3p. Código de actividad usado en esta factura
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'activity_code')
BEGIN
    ALTER TABLE invoice ADD activity_code VARCHAR(6) NULL;
    PRINT 'Added column: invoice.activity_code';
END
GO

-- 3q. Timestamps
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'created_at')
BEGIN
    ALTER TABLE invoice ADD created_at DATETIME2 NOT NULL DEFAULT GETUTCDATE();
    PRINT 'Added column: invoice.created_at';
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'invoice' AND COLUMN_NAME = 'updated_at')
BEGIN
    ALTER TABLE invoice ADD updated_at DATETIME2 NULL;
    PRINT 'Added column: invoice.updated_at';
END
GO

-- Unique index on clave
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'uq_invoice_clave')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX uq_invoice_clave
        ON invoice(clave)
        WHERE clave IS NOT NULL;
    PRINT 'Created unique index: uq_invoice_clave';
END
GO

-- Index on hacienda_status for querying pending documents
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'ix_invoice_hacienda_status')
BEGIN
    CREATE NONCLUSTERED INDEX ix_invoice_hacienda_status
        ON invoice(hacienda_status);
    PRINT 'Created index: ix_invoice_hacienda_status';
END
GO

-- ============================================================================
-- 4. ALTER PRODUCT TABLE - Add CABYS code (mandatory for each line item)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'product' AND COLUMN_NAME = 'cabys_code')
BEGIN
    ALTER TABLE product ADD cabys_code VARCHAR(13) NULL;
    -- CABYS = Clasificador de Bienes y Servicios
    -- Mandatory per line item in electronic invoice XML
    PRINT 'Added column: product.cabys_code';
END
GO

-- ============================================================================
-- 5. ALTER TAX TABLE - Add Hacienda tax type codes
-- ============================================================================

-- Código de impuesto Hacienda (01=IVA, 02=ISC, 03-06=Impuesto específico, 99=Otro)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tax' AND COLUMN_NAME = 'hacienda_tax_code')
BEGIN
    ALTER TABLE tax ADD hacienda_tax_code VARCHAR(2) NULL DEFAULT '01';
    PRINT 'Added column: tax.hacienda_tax_code';
END
GO

-- Código de tarifa IVA (01 a 08 según Hacienda)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tax' AND COLUMN_NAME = 'hacienda_iva_rate_code')
BEGIN
    ALTER TABLE tax ADD hacienda_iva_rate_code VARCHAR(2) NULL;
    PRINT 'Added column: tax.hacienda_iva_rate_code';
END
GO

-- ============================================================================
-- 6. ALTER CLIENTS TABLE - Ensure client_type maps to Hacienda ID types
--    Already has: client_type (varchar), province/canton/district codes
--    client_id is the cédula (string PK)
-- ============================================================================
-- No structural changes needed. client_type should store:
--   '01' = Cédula Física
--   '02' = Cédula Jurídica
--   '03' = DIMEX
--   '04' = NITE
-- NOTE: If your existing client_type values use different conventions,
-- you may want to run a data migration to normalize them.

-- ============================================================================
-- 7. SEED COMPANY CONFIG - Distribuidora Sion
--    NOTE: ATV credentials and certificate info go in appsettings.json, NOT here
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM company_config WHERE id_number = '303110435')
BEGIN
    INSERT INTO company_config (
        company_name,
        commercial_name,
        id_type,
        id_number,
        activity_code,
        province_code,
        canton_code,
        district_code,
        neighborhood_code,
        other_address,
        phone_country_code,
        phone_number,
        email,
        branch_number,
        terminal_number,
        environment
    )
    VALUES (
        'MARGARITA ISABEL CAMPOS ROJAS',     -- company_name (razón social = nombre legal persona física)
        'Distribuidora Sion',               -- commercial_name (nombre comercial)
        '01',                                -- id_type: Cédula Física
        '303110435',                         -- id_number: cédula física
        '477300',                            -- activity_code (4773.0 → 6 digits)
        3,                                   -- province_code: Cartago
        301,                                 -- canton_code: Cartago Central
        30101,                               -- district_code: Oriental (confirmed in DB)
        1,                                   -- neighborhood_code: default 01
        'La Pitahaya, Ciudad de Oro, Casa 13 D', -- other_address
        '506',                               -- phone_country_code
        NULL,                                -- phone_number (update when available)
        'isacampos7040@hotmail.com',         -- email
        '001',                               -- branch_number
        '00001',                             -- terminal_number
        'production'                         -- environment (you have prod credentials)
    );

    PRINT 'Inserted company config: Distribuidora Sion';
END
GO

-- ============================================================================
-- 8. UPDATE TAX RECORDS - Map existing taxes to Hacienda codes
--    Adjust WHERE clauses based on your actual tax data
-- ============================================================================

-- IVA 13% → Hacienda tax code 01, IVA rate code 08 (Tarifa General 13%)
UPDATE tax SET hacienda_tax_code = '01', hacienda_iva_rate_code = '08'
WHERE percentage = 13.00 AND hacienda_tax_code IS NULL;

-- IVA 4% → Hacienda tax code 01, IVA rate code 02 (Tarifa Reducida 4%)
UPDATE tax SET hacienda_tax_code = '01', hacienda_iva_rate_code = '02'
WHERE percentage = 4.00 AND hacienda_tax_code IS NULL;

-- IVA 2% → Hacienda tax code 01, IVA rate code 07 (Tarifa Reducida 2%)
UPDATE tax SET hacienda_tax_code = '01', hacienda_iva_rate_code = '07'
WHERE percentage = 2.00 AND hacienda_tax_code IS NULL;

-- IVA 1% → Hacienda tax code 01, IVA rate code 01 (Tarifa 0% Exento / Reducida 1%)
UPDATE tax SET hacienda_tax_code = '01', hacienda_iva_rate_code = '01'
WHERE percentage = 1.00 AND hacienda_tax_code IS NULL;

-- Exento (0%) → Hacienda tax code 01, IVA rate code 01
UPDATE tax SET hacienda_tax_code = '01', hacienda_iva_rate_code = '01'
WHERE percentage = 0.00 AND hacienda_tax_code IS NULL;

PRINT 'Updated tax records with Hacienda codes';
GO

-- ============================================================================
-- VERIFICATION QUERIES (run after script to verify)
-- ============================================================================
/*
-- Check company config
SELECT * FROM company_config;

-- Check invoice table structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'invoice'
ORDER BY ORDINAL_POSITION;

-- Check consecutive numbers
SELECT * FROM hacienda_consecutive;

-- Check product CABYS column
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'product' AND COLUMN_NAME = 'cabys_code';

-- Check tax Hacienda codes
SELECT tax_id, tax_name, percentage, hacienda_tax_code, hacienda_iva_rate_code
FROM tax;

-- Verify your province/canton/district codes for Cartago
-- Confirmed: province=3 (Cartago), canton=301 (Central), district=30101 (Oriental)
SELECT p.province_code, p.province_name,
       c.canton_code, c.canton_name,
       d.district_code, d.district_name
FROM province p
JOIN canton c ON c.province_code = p.province_code
JOIN district d ON d.canton_code = c.canton_code
WHERE d.district_code = 30101;
*/

COMMIT TRANSACTION;
PRINT '============================================';
PRINT 'Electronic Invoice schema migration complete';
PRINT '============================================';
