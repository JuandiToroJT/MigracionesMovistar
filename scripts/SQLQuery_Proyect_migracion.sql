CREATE DATABASE Proyect_migracion;
USE Proyect_migracion;

-- Tabla: USUARIO
CREATE TABLE USUARIO (
    id_usuario INT IDENTITY(1,1) PRIMARY KEY,
    tipo_identificacion VARCHAR(20),
    numero_identificacion VARCHAR(50),
    nombre VARCHAR(100),
    correo VARCHAR(100),
    celular VARCHAR(20),
    clave VARCHAR(100),
    rol VARCHAR(7) CHECK (rol IN ('cliente', 'admin'))
);

-- Tabla: OPERADOR
CREATE TABLE OPERADOR (
    id_operador INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(100)
);

-- Tabla: SERVICIO
CREATE TABLE SERVICIO (
    id_servicio INT IDENTITY(1,1) PRIMARY KEY,
    tipo VARCHAR(5) CHECK (tipo IN ('Movil', 'Fibra', 'Banda')),
    nombre VARCHAR(100),
    descripcion VARCHAR(255),
    id_operador INT FOREIGN KEY REFERENCES OPERADOR(id_operador)
);

-- Tabla: CUENTA
CREATE TABLE CUENTA (
    id_cuenta INT IDENTITY(1,1) PRIMARY KEY,
    numero_cuenta VARCHAR(50),
    migrada char(1) CHECK (migrada IN ('S', 'N')),
    id_usuario INT FOREIGN KEY REFERENCES USUARIO(id_usuario),
    id_servicio INT FOREIGN KEY REFERENCES SERVICIO(id_servicio)
);

-- Tabla: PROCESO
CREATE TABLE PROCESO (
    id_proceso INT IDENTITY(1,1) PRIMARY KEY,
    origen VARCHAR(9) CHECK (origen IN ('Migracion', 'Cargue')),
    estado_proceso varchar(3) CHECK (estado_proceso IN ('PRO', 'FIN', 'ERR')),
    fecha DATETIME DEFAULT GETDATE(),
    total_registros INT,
    cantidad_exito INT,
    cantidad_error INT,
    cantidad_duplicado INT,
    notas VARCHAR(255),
    id_usuario INT FOREIGN KEY REFERENCES USUARIO(id_usuario)
);

-- Tabla: DETALLE
CREATE TABLE DETALLE (
    id_detalle INT IDENTITY(1,1) PRIMARY KEY,
    fecha DATETIME DEFAULT GETDATE(),
    estado VARCHAR(3) CHECK (estado IN ('APL', 'ERR', 'DUP')),
    tipoProceso VARCHAR(9) CHECK (tipoProceso IN ('Migracion', 'Cargue')),
    notas varchar(255),
    codigoError varchar(10),
    id_operador_destino INT NULL FOREIGN KEY REFERENCES OPERADOR(id_operador),
    id_operador_origen INT NULL FOREIGN KEY REFERENCES OPERADOR(id_operador),
    id_cuenta INT NULL FOREIGN KEY REFERENCES CUENTA(id_cuenta),
    id_proceso INT NULL FOREIGN KEY REFERENCES PROCESO(id_proceso)
);

-- Tabla: AUDITORIA
CREATE TABLE AUDITORIA (
    id_auditoria INT IDENTITY(1,1) PRIMARY KEY,
    fecha DATETIME DEFAULT GETDATE(),
    tipo_evento VARCHAR(9) CHECK (tipo_evento IN ('Migracion', 'Cargue')),
    descripcion VARCHAR(255),
    id_usuario INT FOREIGN KEY REFERENCES USUARIO(id_usuario)
);