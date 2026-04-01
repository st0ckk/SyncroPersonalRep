-- ============================================================================
-- Update company_config for SANDBOX testing
-- ATV User: cpf-03-0512-0622 → Cédula Física 305120622
-- Run this before testing with sandbox credentials
-- ============================================================================

-- Update existing company config to match sandbox test certificate
UPDATE company_config
SET
    id_number = '305120622',
    environment = 'sandbox'
WHERE id_number = '303110435';

-- If no row was updated, insert one
IF @@ROWCOUNT = 0 AND NOT EXISTS (SELECT 1 FROM company_config WHERE id_number = '305120622')
BEGIN
    INSERT INTO company_config (
        company_name, commercial_name, id_type, id_number,
        activity_code, province_code, canton_code, district_code,
        neighborhood_code, other_address, phone_country_code,
        phone_number, email, branch_number, terminal_number, environment
    )
    VALUES (
        'MARGARITA ISABEL CAMPOS ROJAS', 'Distribuidora Sion',
        '01', '305120622',
        '477300', 3, 301, 30101,
        1, 'La Pitahaya, Ciudad de Oro, Casa 13 D', '506',
        NULL, 'crsyncro@gmail.com', '001', '00001', 'sandbox'
    );
END

PRINT 'Company config updated for sandbox testing';
GO

-- Also reset consecutive numbers so sandbox starts fresh
UPDATE hacienda_consecutive SET last_number = 0;
PRINT 'Reset consecutive numbers for sandbox';
GO

-- Verify
SELECT id_number, environment, company_name FROM company_config;
SELECT * FROM hacienda_consecutive;
