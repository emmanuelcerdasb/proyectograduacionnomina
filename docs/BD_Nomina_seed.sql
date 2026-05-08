-- ============================================================
-- BD_Nomina_seed.sql
-- Datos iniciales del sistema de nomina AraWeb CR
-- Ejecutar UNA SOLA VEZ despues de crear la estructura de la BD
-- ============================================================

USE BD_Nomina;
GO

-- ============================================================
-- ROLES DEL SISTEMA
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Rol WHERE nombre = 'Administrador')
    INSERT INTO Rol (nombre) VALUES ('Administrador');

IF NOT EXISTS (SELECT 1 FROM Rol WHERE nombre = 'RRHH')
    INSERT INTO Rol (nombre) VALUES ('RRHH');

IF NOT EXISTS (SELECT 1 FROM Rol WHERE nombre = 'Jefe')
    INSERT INTO Rol (nombre) VALUES ('Jefe');

IF NOT EXISTS (SELECT 1 FROM Rol WHERE nombre = 'Jefa')
    INSERT INTO Rol (nombre) VALUES ('Jefa');

IF NOT EXISTS (SELECT 1 FROM Rol WHERE nombre = 'Colaborador')
    INSERT INTO Rol (nombre) VALUES ('Colaborador');

GO

-- ============================================================
-- TIPOS DE LIQUIDACION (Codigo de Trabajo CR)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM TipoLiquidacion WHERE codigo = 'REN')
    INSERT INTO TipoLiquidacion (codigo, nombre, descripcion, aplica_cesantia, aplica_preaviso)
    VALUES ('REN', 'Renuncia voluntaria',
            'El trabajador renuncia por voluntad propia. Aplica preaviso pero no cesantia.',
            0, 1);

IF NOT EXISTS (SELECT 1 FROM TipoLiquidacion WHERE codigo = 'DSJ')
    INSERT INTO TipoLiquidacion (codigo, nombre, descripcion, aplica_cesantia, aplica_preaviso)
    VALUES ('DSJ', 'Despido sin justa causa',
            'Empleador termina el contrato sin causa justificada. Aplica cesantia y preaviso (art. 28-29 CT).',
            1, 1);

IF NOT EXISTS (SELECT 1 FROM TipoLiquidacion WHERE codigo = 'DCJ')
    INSERT INTO TipoLiquidacion (codigo, nombre, descripcion, aplica_cesantia, aplica_preaviso)
    VALUES ('DCJ', 'Despido con justa causa',
            'Empleador despide por falta grave (art. 81 CT). No aplica cesantia ni preaviso.',
            0, 0);

IF NOT EXISTS (SELECT 1 FROM TipoLiquidacion WHERE codigo = 'MUT')
    INSERT INTO TipoLiquidacion (codigo, nombre, descripcion, aplica_cesantia, aplica_preaviso)
    VALUES ('MUT', 'Mutuo acuerdo',
            'Ambas partes acuerdan terminar la relacion laboral. Negociable.',
            0, 0);

IF NOT EXISTS (SELECT 1 FROM TipoLiquidacion WHERE codigo = 'PEN')
    INSERT INTO TipoLiquidacion (codigo, nombre, descripcion, aplica_cesantia, aplica_preaviso)
    VALUES ('PEN', 'Pension / Jubilacion',
            'El trabajador se pensiona. Aplica cesantia proporcional segun convenio.',
            1, 0);

GO

-- ============================================================
-- CLASES DE HORA EXTRA (Codigo de Trabajo CR)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM ClaseHoraExtra WHERE nombre = 'Hora extra diurna')
    INSERT INTO ClaseHoraExtra (nombre, porcentaje)
    VALUES ('Hora extra diurna', 50);   -- +50% sobre hora ordinaria

IF NOT EXISTS (SELECT 1 FROM ClaseHoraExtra WHERE nombre = 'Hora extra nocturna')
    INSERT INTO ClaseHoraExtra (nombre, porcentaje)
    VALUES ('Hora extra nocturna', 100); -- +100% sobre hora ordinaria

IF NOT EXISTS (SELECT 1 FROM ClaseHoraExtra WHERE nombre = 'Hora extra en dia feriado')
    INSERT INTO ClaseHoraExtra (nombre, porcentaje)
    VALUES ('Hora extra en dia feriado', 100);

GO

-- ============================================================
-- JORNADAS LABORALES
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Jornada WHERE nombre = 'Diurna')
    INSERT INTO Jornada (nombre, horas_semanales)
    VALUES ('Diurna', 48);

IF NOT EXISTS (SELECT 1 FROM Jornada WHERE nombre = 'Mixta')
    INSERT INTO Jornada (nombre, horas_semanales)
    VALUES ('Mixta', 45);

IF NOT EXISTS (SELECT 1 FROM Jornada WHERE nombre = 'Nocturna')
    INSERT INTO Jornada (nombre, horas_semanales)
    VALUES ('Nocturna', 36);

GO

-- ============================================================
-- DEPARTAMENTO Y PUESTO INICIALES
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Departamento WHERE nombre = 'Administracion')
BEGIN
    INSERT INTO Departamento (nombre) VALUES ('Administracion');
END

DECLARE @idDepto INT = (SELECT TOP 1 idDepartamento FROM Departamento WHERE nombre = 'Administracion');

IF NOT EXISTS (SELECT 1 FROM Puesto WHERE nombre = 'Administrador General')
    INSERT INTO Puesto (nombre, salario_base, Departamento_idDepartamento)
    VALUES ('Administrador General', 800000, @idDepto);

GO

-- ============================================================
-- CREDENCIAL ADMINISTRADOR INICIAL
-- Contrasena: Admin2024!
-- Hash PBKDF2 (100k iter, SHA-256) generado con PasswordHelper.HashPassword
-- NOTA: Al primer login el sistema pedira cambio de contrasena (requiere_cambio = 1)
-- ============================================================

-- IMPORTANTE: Si ya tienes un usuario admin, omite este bloque.
-- El hash a continuacion es un placeholder; ejecuta el sistema,
-- haz login con cualquier contrasena y el hash se genera automaticamente,
-- O usa el sistema para crear el primer usuario desde la interfaz.

/*
DECLARE @idRolAdmin INT = (SELECT TOP 1 idRol FROM Rol WHERE nombre = 'Administrador');

IF NOT EXISTS (SELECT 1 FROM Credencial WHERE usuario = 'admin')
BEGIN
    -- Insertar primero la persona
    INSERT INTO Persona (nombre, apellido1, apellido2, cedula, telefono, correo, fecha_nacimiento)
    VALUES ('Administrador', 'Sistema', 'AraWeb', '000000000', '00000000', 'admin@araweb.cr', '1990-01-01');

    DECLARE @idPersona INT = SCOPE_IDENTITY();

    -- Insertar credencial (contrasena se actualizara al primer login o manualmente)
    INSERT INTO Credencial (usuario, contrasena, activo, requiere_cambio, fecha_creacion, Rol_idRol)
    VALUES ('admin',
            'REEMPLAZAR_CON_HASH_GENERADO',
            1, 1, GETDATE(), @idRolAdmin);
END
*/

GO

PRINT 'Seed completado correctamente.';
