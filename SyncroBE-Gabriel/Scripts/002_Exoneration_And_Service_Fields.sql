-- ============================================================================
-- Electronic Invoice - Phase 2: Exoneration & Service/Merchandise Support
-- Schema changes for SyncroBE
-- ============================================================================
-- Run against your MSSQL database (syncro) AFTER 001_ElectronicInvoice_Schema.sql
-- ============================================================================

BEGIN TRANSACTION;

-- ============================================================================
-- 1. ALTER PRODUCT TABLE - Add is_service flag
--    true = Service (UnidadMedida: Sp), false = Merchandise (UnidadMedida: Unid)
-- ============================================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'product' AND COLUMN_NAME = 'is_service')
BEGIN
    ALTER TABLE product ADD is_service BIT NOT NULL DEFAULT 0;
    PRINT 'Added column: product.is_service';
END
GO

-- ============================================================================
-- 2. ALTER CLIENTS TABLE - Add exoneration fields
--    Clients like Cruz Roja have blanket exonerations from IVA
-- ============================================================================

-- Exoneration document type (Hacienda code: 01-07, 99)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'clients' AND COLUMN_NAME = 'exoneration_doc_type')
BEGIN
    ALTER TABLE clients ADD exoneration_doc_type VARCHAR(2) NULL;
    PRINT 'Added column: clients.exoneration_doc_type';
END
GO

-- Exoneration document number (e.g. "AL-00023244-25")
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'clients' AND COLUMN_NAME = 'exoneration_doc_number')
BEGIN
    ALTER TABLE clients ADD exoneration_doc_number VARCHAR(40) NULL;
    PRINT 'Added column: clients.exoneration_doc_number';
END
GO

-- Institution code that granted the exoneration (e.g. "01" = Ministerio de Hacienda)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'clients' AND COLUMN_NAME = 'exoneration_institution_code')
BEGIN
    ALTER TABLE clients ADD exoneration_institution_code VARCHAR(2) NULL;
    PRINT 'Added column: clients.exoneration_institution_code';
END
GO

-- Institution name (e.g. "Ministerio de Hacienda")
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'clients' AND COLUMN_NAME = 'exoneration_institution_name')
BEGIN
    ALTER TABLE clients ADD exoneration_institution_name VARCHAR(160) NULL;
    PRINT 'Added column: clients.exoneration_institution_name';
END
GO

-- Date the exoneration was issued
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'clients' AND COLUMN_NAME = 'exoneration_date')
BEGIN
    ALTER TABLE clients ADD exoneration_date DATETIME2 NULL;
    PRINT 'Added column: clients.exoneration_date';
END
GO

-- Percentage exempted (e.g. 13 for full IVA 13% exemption)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'clients' AND COLUMN_NAME = 'exoneration_percentage')
BEGIN
    ALTER TABLE clients ADD exoneration_percentage INT NULL;
    PRINT 'Added column: clients.exoneration_percentage';
END
GO

-- ============================================================================
-- 3. ALTER CLIENTS TABLE - Add receptor activity code
--    Required by Hacienda for some document types
-- ============================================================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'clients' AND COLUMN_NAME = 'activity_code')
BEGIN
    ALTER TABLE clients ADD activity_code VARCHAR(6) NULL;
    PRINT 'Added column: clients.activity_code';
END
GO

-- ============================================================================
-- EXAMPLE: How to set up an exonerated client (e.g. Cruz Roja)
-- ============================================================================
/*
UPDATE clients SET
    exoneration_doc_type = '04',                -- Exenciones DGH
    exoneration_doc_number = 'AL-00023244-25',  -- Document number
    exoneration_institution_code = '01',        -- Ministerio de Hacienda
    exoneration_institution_name = 'Ministerio de Hacienda',
    exoneration_date = '2025-07-28',
    exoneration_percentage = 13,                -- Full IVA exemption
    activity_code = '702000'                    -- Receptor's activity code
WHERE client_id = '3012306520';                 -- Cruz Roja's cédula jurídica
*/

-- ============================================================================
-- EXONERATION DOCUMENT TYPE REFERENCE
-- ============================================================================
-- Code | Description
-- ---- | -----------
--  01  | Compras autorizadas
--  02  | Ventas exentas a diplomáticos y organismos internacionales
--  03  | Orden de compra (instituciones del Estado)
--  04  | Exenciones Dirección General de Hacienda
--  05  | Zonas Francas
--  06  | Régimen especial
--  07  | Transitorio
--  99  | Otros
-- ============================================================================

COMMIT TRANSACTION;
PRINT '============================================';
PRINT 'Phase 2 migration complete: Exoneration + Service fields';
PRINT '============================================';
