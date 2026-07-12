/* ════════════════════════════════════════════════════════════════════════
   FocusMind S.A.C. — Esquema de Base de Datos (SQL Server / AWS RDS)
   ════════════════════════════════════════════════════════════════════════
   Convención de prefijos:
     TM_ = Tabla Maestra   (entidad independiente con ciclo de vida propio)
     TD_ = Tabla Detalle   (dependiente 1:N de un padre; no existe sin él)
     TR_ = Tabla Relación  (resuelve N:M; PK propia IDENTITY, no PK compuesta)
   ════════════════════════════════════════════════════════════════════════ */

-- ════════════════════════════════════════════════════════════════════════
-- SECCIÓN 1 — TABLAS MAESTRAS DE CATÁLOGO (lookup)
-- Derivadas de los union types Categoria y ObjetivoCognitivo (producto.ts),
-- que además tienen diccionario de etiquetas (CATEGORIA_LABELS/OBJETIVO_LABELS).
-- Se normalizan como catálogo en vez de VARCHAR con CHECK porque se reutilizan
-- en más de una tabla (TM_PRODUCTO y TM_DIAGNOSTICO comparten TM_OBJETIVO).
-- ════════════════════════════════════════════════════════════════════════

CREATE TABLE TM_CATEGORIA (
    IDCATEGORIA     INT IDENTITY(1,1) PRIMARY KEY,
    CODIGO          VARCHAR(20)  NOT NULL,
    NOMBRE          VARCHAR(50)  NOT NULL,
    ACTIVO          BIT          NOT NULL DEFAULT (1),
    USUARIOCREA     INT          NULL,
    USUARIOMODIFICA INT          NULL,
    FECHACREA       DATETIME     NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA   DATETIME     NULL,
    CONSTRAINT UQ_TM_CATEGORIA_CODIGO UNIQUE (CODIGO)
);
GO

CREATE TABLE TM_OBJETIVO (
    IDOBJETIVO      INT IDENTITY(1,1) PRIMARY KEY,
    CODIGO          VARCHAR(40)  NOT NULL,
    NOMBRE          VARCHAR(60)  NOT NULL,
    ACTIVO          BIT          NOT NULL DEFAULT (1),
    USUARIOCREA     INT          NULL,
    USUARIOMODIFICA INT          NULL,
    FECHACREA       DATETIME     NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA   DATETIME     NULL,
    CONSTRAINT UQ_TM_OBJETIVO_CODIGO UNIQUE (CODIGO)
);
GO

-- Catálogo de alérgenos: reutilizado por TM_PRODUCTO.alergenos y
-- TM_DIAGNOSTICO.alergias (ProductoService.obtenerAlergenosUnicos() confirma
-- que es un dominio de valores compartido y finito, no texto libre).
CREATE TABLE TM_ALERGENO (
    IDALERGENO      INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE          VARCHAR(100) NOT NULL,
    ACTIVO          BIT          NOT NULL DEFAULT (1),
    USUARIOCREA     INT          NULL,
    USUARIOMODIFICA INT          NULL,
    FECHACREA       DATETIME     NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA   DATETIME     NULL,
    CONSTRAINT UQ_TM_ALERGENO_NOMBRE UNIQUE (NOMBRE)
);
GO

-- ════════════════════════════════════════════════════════════════════════
-- SECCIÓN 2 — USUARIO
-- Derivada de models/usuario/usuario.ts + validators de acceso.ts/registro.ts.
-- NOTA: PerfilCognitivo (nivelEstres, calidadSueno, objetivoPrincipal) NO se
-- materializa aquí: el propio comentario del modelo TS indica que es
-- "calculado a partir del diagnóstico más reciente" → se resuelve en runtime
-- con usp_ObtenerPerfilCognitivo_X_Usuario para respetar 3FN (evitar dato
-- derivado almacenado y su posible desincronización con TM_DIAGNOSTICO).
-- ════════════════════════════════════════════════════════════════════════

CREATE TABLE TM_USUARIO (
    IDUSUARIO       INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE          VARCHAR(100) NOT NULL,
    EMAIL           VARCHAR(254) NOT NULL,
    PASSWORD        VARCHAR(256) NOT NULL,
    ACTIVO          BIT          NOT NULL DEFAULT (1),
    USUARIOCREA     INT          NULL,
    USUARIOMODIFICA INT          NULL,
    FECHACREA       DATETIME     NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA   DATETIME     NULL,
    CONSTRAINT UQ_TM_USUARIO_EMAIL UNIQUE (EMAIL)
);
GO

-- ════════════════════════════════════════════════════════════════════════
-- SECCIÓN 3 — PRODUCTO Y ENTIDADES HIJAS
-- Derivada de models/producto/producto.ts + data/productos/productos.ts.
-- ════════════════════════════════════════════════════════════════════════

CREATE TABLE TM_PRODUCTO (
    IDPRODUCTO        INT IDENTITY(1,1) PRIMARY KEY,
    NOMBRE            VARCHAR(150)  NOT NULL,
    MARCA             VARCHAR(100)  NOT NULL,
    IDCATEGORIA       INT           NOT NULL,
    IDOBJETIVO        INT           NOT NULL,
    PRECIO            DECIMAL(10,2) NOT NULL,
    DESCRIPCION       VARCHAR(500)  NOT NULL,
    DOSISRECOMENDADA  VARCHAR(200)  NOT NULL,
    -- URLIMAGEN: URL absoluta al objeto en el bucket S3 (única fuente de
    -- verdad para la imagen del producto). NULLABLE porque no todo producto
    -- tiene aún imagen migrada a S3.
    URLIMAGEN         VARCHAR(500)  NULL,
    -- registroSanitario/entidadRegistro son nullable en pareja:
    -- ver comentario en producto.ts "null si registroSanitario es null".
    REGISTROSANITARIO VARCHAR(50)   NULL,
    ENTIDADREGISTRO   VARCHAR(10)   NULL,
    -- Se agrega para soportar el descuento transaccional en la venta (usp_InsertarPedidoDetalle)
    -- y el mantenimiento de catálogo (usp_ActualizarProducto/usp_ActualizarStockProducto).
    STOCK             INT           NOT NULL DEFAULT (0),
    ACTIVO            BIT           NOT NULL DEFAULT (1),
    USUARIOCREA       INT           NULL,
    USUARIOMODIFICA   INT           NULL,
    FECHACREA         DATETIME      NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA     DATETIME      NULL,
    CONSTRAINT FK_TM_PRODUCTO_TM_CATEGORIA FOREIGN KEY (IDCATEGORIA) REFERENCES TM_CATEGORIA (IDCATEGORIA),
    CONSTRAINT FK_TM_PRODUCTO_TM_OBJETIVO  FOREIGN KEY (IDOBJETIVO)  REFERENCES TM_OBJETIVO (IDOBJETIVO),
    CONSTRAINT CK_TM_PRODUCTO_PRECIO CHECK (PRECIO > 0),
    CONSTRAINT CK_TM_PRODUCTO_STOCK CHECK (STOCK >= 0),
    CONSTRAINT CK_TM_PRODUCTO_ENTIDADREGISTRO CHECK (ENTIDADREGISTRO IN ('DIGESA', 'DIGEMID') OR ENTIDADREGISTRO IS NULL),
    CONSTRAINT CK_TM_PRODUCTO_REGISTRO_PAR CHECK (
        (REGISTROSANITARIO IS NULL AND ENTIDADREGISTRO IS NULL) OR
        (REGISTROSANITARIO IS NOT NULL AND ENTIDADREGISTRO IS NOT NULL)
    )
);
GO

-- Producto.ingredientes: string[] → tabla detalle 1:N (no aplica FK a catálogo
-- porque cada ingrediente incluye dosis embebida en el texto, ej. "Bacopa
-- Monnieri 300mg", por lo que no es un valor reutilizable/normalizable).
CREATE TABLE TD_PRODUCTO_INGREDIENTE (
    IDPRODUCTOINGREDIENTE INT IDENTITY(1,1) PRIMARY KEY,
    IDPRODUCTO            INT          NOT NULL,
    INGREDIENTE           VARCHAR(150) NOT NULL,
    ACTIVO                BIT          NOT NULL DEFAULT (1),
    USUARIOCREA           INT          NULL,
    USUARIOMODIFICA       INT          NULL,
    FECHACREA             DATETIME     NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA         DATETIME     NULL,
    CONSTRAINT FK_TD_PRODUCTOINGREDIENTE_TM_PRODUCTO FOREIGN KEY (IDPRODUCTO) REFERENCES TM_PRODUCTO (IDPRODUCTO)
);
GO

-- Producto.contraindicaciones: string[] → tabla detalle 1:N.
CREATE TABLE TD_PRODUCTO_CONTRAINDICACION (
    IDPRODUCTOCONTRAINDICACION INT IDENTITY(1,1) PRIMARY KEY,
    IDPRODUCTO                 INT          NOT NULL,
    DESCRIPCION                VARCHAR(300) NOT NULL,
    ACTIVO                     BIT          NOT NULL DEFAULT (1),
    USUARIOCREA                INT          NULL,
    USUARIOMODIFICA            INT          NULL,
    FECHACREA                  DATETIME     NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA              DATETIME     NULL,
    CONSTRAINT FK_TD_PRODUCTOCONTRAIND_TM_PRODUCTO FOREIGN KEY (IDPRODUCTO) REFERENCES TM_PRODUCTO (IDPRODUCTO)
);
GO

-- Producto.alergenos: string[] pero reutilizado entre productos
-- (ProductoService.obtenerAlergenosUnicos) → relación N:M real.
CREATE TABLE TR_PRODUCTO_ALERGENO (
    IDPRODUCTOALERGENO INT IDENTITY(1,1) PRIMARY KEY,
    IDPRODUCTO         INT      NOT NULL,
    IDALERGENO         INT      NOT NULL,
    ACTIVO             BIT      NOT NULL DEFAULT (1),
    USUARIOCREA        INT      NULL,
    USUARIOMODIFICA    INT      NULL,
    FECHACREA          DATETIME NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA      DATETIME NULL,
    CONSTRAINT FK_TR_PRODUCTOALERGENO_TM_PRODUCTO FOREIGN KEY (IDPRODUCTO) REFERENCES TM_PRODUCTO (IDPRODUCTO),
    CONSTRAINT FK_TR_PRODUCTOALERGENO_TM_ALERGENO FOREIGN KEY (IDALERGENO) REFERENCES TM_ALERGENO (IDALERGENO)
);
GO

-- UNIQUE filtrado (no una CONSTRAINT de tabla) a propósito — el patrón de baja lógica
-- de esta tabla (ACTIVO=0 en vez de DELETE, ver usp_Desactivar_ProductoAlergeno_X_Producto)
-- necesita permitir que el MISMO par (IDPRODUCTO, IDALERGENO) tenga varias filas inactivas en
-- su historial y solo exigir unicidad entre las filas ACTIVAS. Un UNIQUE de tabla normal
-- (versión original de este script) ignora ACTIVO y choca contra su propia fila desactivada en
-- cuanto se reselecciona el mismo alérgeno en una edición posterior — encontrado y corregido
-- al resincronizar productos reales contra RDS.
CREATE UNIQUE INDEX UQ_TR_PRODUCTOALERGENO_PAR ON TR_PRODUCTO_ALERGENO (IDPRODUCTO, IDALERGENO) WHERE ACTIVO = 1;
GO

-- ════════════════════════════════════════════════════════════════════════
-- SECCIÓN 4 — DIAGNÓSTICO (Quiz Cognitivo) Y ENTIDADES HIJAS
-- Derivada de models/diagnostico/diagnostico.ts + features/quiz/quiz.ts.
-- Solo se persiste cuando hay sesión activa (DiagnosticoService.registrar
-- únicamente se invoca si AuthService.sesionActiva() es true) → IDUSUARIO NOT NULL.
-- ════════════════════════════════════════════════════════════════════════

CREATE TABLE TM_DIAGNOSTICO (
    IDDIAGNOSTICO       INT IDENTITY(1,1) PRIMARY KEY,
    IDUSUARIO           INT          NOT NULL,
    FECHA               DATETIME     NOT NULL,
    NIVELESTRES         INT          NOT NULL,
    CALIDADSUENO        INT          NOT NULL,
    IDOBJETIVO          INT          NOT NULL,
    HORASCONCENTRACION  INT          NOT NULL,
    CONDICIONMEDICA     VARCHAR(300) NULL,
    ACTIVO              BIT          NOT NULL DEFAULT (1),
    USUARIOCREA         INT          NULL,
    USUARIOMODIFICA     INT          NULL,
    FECHACREA           DATETIME     NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA       DATETIME     NULL,
    CONSTRAINT FK_TM_DIAGNOSTICO_TM_USUARIO  FOREIGN KEY (IDUSUARIO)  REFERENCES TM_USUARIO (IDUSUARIO),
    CONSTRAINT FK_TM_DIAGNOSTICO_TM_OBJETIVO FOREIGN KEY (IDOBJETIVO) REFERENCES TM_OBJETIVO (IDOBJETIVO),
    CONSTRAINT CK_TM_DIAGNOSTICO_NIVELESTRES  CHECK (NIVELESTRES BETWEEN 1 AND 10),
    CONSTRAINT CK_TM_DIAGNOSTICO_CALIDADSUENO CHECK (CALIDADSUENO BETWEEN 1 AND 10),
    CONSTRAINT CK_TM_DIAGNOSTICO_HORASCONCENTRACION CHECK (HORASCONCENTRACION BETWEEN 1 AND 16)
);
GO

-- Diagnostico.alergias: string[] → relación N:M contra el mismo catálogo TM_ALERGENO.
CREATE TABLE TR_DIAGNOSTICO_ALERGENO (
    IDDIAGNOSTICOALERGENO INT IDENTITY(1,1) PRIMARY KEY,
    IDDIAGNOSTICO         INT      NOT NULL,
    IDALERGENO            INT      NOT NULL,
    ACTIVO                BIT      NOT NULL DEFAULT (1),
    USUARIOCREA           INT      NULL,
    USUARIOMODIFICA       INT      NULL,
    FECHACREA             DATETIME NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA         DATETIME NULL,
    CONSTRAINT FK_TR_DIAGNOSTICOALERGENO_TM_DIAGNOSTICO FOREIGN KEY (IDDIAGNOSTICO) REFERENCES TM_DIAGNOSTICO (IDDIAGNOSTICO),
    CONSTRAINT FK_TR_DIAGNOSTICOALERGENO_TM_ALERGENO     FOREIGN KEY (IDALERGENO)     REFERENCES TM_ALERGENO (IDALERGENO),
    CONSTRAINT UQ_TR_DIAGNOSTICOALERGENO_PAR UNIQUE (IDDIAGNOSTICO, IDALERGENO)
);
GO

-- Diagnostico.recomendacionesIds: number[] de Producto → relación N:M.
CREATE TABLE TR_DIAGNOSTICO_RECOMENDACION (
    IDDIAGNOSTICORECOMENDACION INT IDENTITY(1,1) PRIMARY KEY,
    IDDIAGNOSTICO              INT      NOT NULL,
    IDPRODUCTO                 INT      NOT NULL,
    ACTIVO                     BIT      NOT NULL DEFAULT (1),
    USUARIOCREA                INT      NULL,
    USUARIOMODIFICA            INT      NULL,
    FECHACREA                  DATETIME NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA              DATETIME NULL,
    CONSTRAINT FK_TR_DIAGNOSTICORECOM_TM_DIAGNOSTICO FOREIGN KEY (IDDIAGNOSTICO) REFERENCES TM_DIAGNOSTICO (IDDIAGNOSTICO),
    CONSTRAINT FK_TR_DIAGNOSTICORECOM_TM_PRODUCTO     FOREIGN KEY (IDPRODUCTO)     REFERENCES TM_PRODUCTO (IDPRODUCTO),
    CONSTRAINT UQ_TR_DIAGNOSTICORECOM_PAR UNIQUE (IDDIAGNOSTICO, IDPRODUCTO)
);
GO

-- ════════════════════════════════════════════════════════════════════════
-- SECCIÓN 5 — PEDIDO Y DETALLE (Carrito/Checkout)
-- Derivada de models/pedido/pedido.ts + services/payment/payment.ts +
-- features/checkout/checkout.ts. Checkout exige authGuard → IDUSUARIO NOT NULL.
-- NOTA DE SEGURIDAD: numeroTarjeta (PaymentRequest) NO se persiste en ninguna
-- columna — ver notas de diseño al final (PCI-DSS: nunca almacenar PAN en claro).
-- ════════════════════════════════════════════════════════════════════════

CREATE TABLE TM_PEDIDO (
    IDPEDIDO          INT IDENTITY(1,1) PRIMARY KEY,
    IDUSUARIO         INT           NOT NULL,
    -- Pedido.id (string, ej. "TXN-1732999999999") es el ID de transacción de la
    -- pasarela de pago, no un IDENTITY interno → se mapea a NUMEROPEDIDO (natural key).
    NUMEROPEDIDO      VARCHAR(50)   NOT NULL,
    FECHAPEDIDO       DATETIME      NOT NULL,
    TOTAL             DECIMAL(10,2) NOT NULL,
    -- Snapshot de datos de envío capturados en el propio formulario de Checkout
    -- (ClientePago no se toma del perfil de TM_USUARIO, que no tiene domicilio).
    NOMBRECLIENTE     VARCHAR(150)  NOT NULL,
    DIRECCIONENVIO    VARCHAR(200)  NOT NULL,
    CIUDADENVIO       VARCHAR(100)  NOT NULL,
    TELEFONOCONTACTO  VARCHAR(9)    NOT NULL,
    METODOPAGO        VARCHAR(20)   NOT NULL,
    ACTIVO            BIT           NOT NULL DEFAULT (1),
    USUARIOCREA       INT           NULL,
    USUARIOMODIFICA   INT           NULL,
    FECHACREA         DATETIME      NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA     DATETIME      NULL,
    CONSTRAINT FK_TM_PEDIDO_TM_USUARIO FOREIGN KEY (IDUSUARIO) REFERENCES TM_USUARIO (IDUSUARIO),
    CONSTRAINT UQ_TM_PEDIDO_NUMEROPEDIDO UNIQUE (NUMEROPEDIDO),
    CONSTRAINT CK_TM_PEDIDO_TOTAL CHECK (TOTAL > 0),
    CONSTRAINT CK_TM_PEDIDO_METODOPAGO CHECK (METODOPAGO IN ('tarjeta', 'yape', 'contraentrega')),
    CONSTRAINT CK_TM_PEDIDO_TELEFONO CHECK (
        LEN(TELEFONOCONTACTO) = 9 AND
        TELEFONOCONTACTO LIKE '9[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'
    )
);
GO

-- Pedido.items: ItemCarrito[] → detalle 1:N. NOMBREPRODUCTO/PRECIOUNITARIO se
-- guardan como snapshot histórico (el precio/nombre del producto puede cambiar
-- después de la compra; el detalle del pedido debe reflejar lo pagado, no el
-- estado actual de TM_PRODUCTO) — decisión de diseño, no violación de 3FN.
CREATE TABLE TD_PEDIDO_DETALLE (
    IDPEDIDODETALLE INT IDENTITY(1,1) PRIMARY KEY,
    IDPEDIDO        INT           NOT NULL,
    IDPRODUCTO      INT           NOT NULL,
    NOMBREPRODUCTO  VARCHAR(150)  NOT NULL,
    PRECIOUNITARIO  DECIMAL(10,2) NOT NULL,
    CANTIDAD        INT           NOT NULL,
    ACTIVO          BIT           NOT NULL DEFAULT (1),
    USUARIOCREA     INT           NULL,
    USUARIOMODIFICA INT           NULL,
    FECHACREA       DATETIME      NOT NULL DEFAULT (GETDATE()),
    FECHAMODIFICA   DATETIME      NULL,
    CONSTRAINT FK_TD_PEDIDODETALLE_TM_PEDIDO   FOREIGN KEY (IDPEDIDO)   REFERENCES TM_PEDIDO (IDPEDIDO),
    CONSTRAINT FK_TD_PEDIDODETALLE_TM_PRODUCTO FOREIGN KEY (IDPRODUCTO) REFERENCES TM_PRODUCTO (IDPRODUCTO),
    CONSTRAINT CK_TD_PEDIDODETALLE_CANTIDAD CHECK (CANTIDAD BETWEEN 1 AND 10),
    CONSTRAINT CK_TD_PEDIDODETALLE_PRECIOUNITARIO CHECK (PRECIOUNITARIO > 0)
);
GO

-- ════════════════════════════════════════════════════════════════════════
-- SECCIÓN 6 — ÍNDICES EXPLÍCITOS
-- Sobre toda FK y sobre columnas usadas en filtros (FiltrosProducto,
-- historiales por usuario) — pensado para I/O/CPU limitados en RDS.
-- ════════════════════════════════════════════════════════════════════════

CREATE NONCLUSTERED INDEX IX_TM_PRODUCTO_IDCATEGORIA ON TM_PRODUCTO (IDCATEGORIA);
CREATE NONCLUSTERED INDEX IX_TM_PRODUCTO_IDOBJETIVO   ON TM_PRODUCTO (IDOBJETIVO);
CREATE NONCLUSTERED INDEX IX_TM_PRODUCTO_NOMBRE       ON TM_PRODUCTO (NOMBRE);
CREATE NONCLUSTERED INDEX IX_TM_PRODUCTO_PRECIO       ON TM_PRODUCTO (PRECIO);

CREATE NONCLUSTERED INDEX IX_TD_PRODUCTOINGREDIENTE_IDPRODUCTO ON TD_PRODUCTO_INGREDIENTE (IDPRODUCTO);
CREATE NONCLUSTERED INDEX IX_TD_PRODUCTOCONTRAIND_IDPRODUCTO   ON TD_PRODUCTO_CONTRAINDICACION (IDPRODUCTO);

CREATE NONCLUSTERED INDEX IX_TR_PRODUCTOALERGENO_IDPRODUCTO ON TR_PRODUCTO_ALERGENO (IDPRODUCTO);
CREATE NONCLUSTERED INDEX IX_TR_PRODUCTOALERGENO_IDALERGENO ON TR_PRODUCTO_ALERGENO (IDALERGENO);

CREATE NONCLUSTERED INDEX IX_TM_DIAGNOSTICO_IDUSUARIO  ON TM_DIAGNOSTICO (IDUSUARIO);
CREATE NONCLUSTERED INDEX IX_TM_DIAGNOSTICO_IDOBJETIVO ON TM_DIAGNOSTICO (IDOBJETIVO);

CREATE NONCLUSTERED INDEX IX_TR_DIAGNOSTICOALERGENO_IDDIAGNOSTICO ON TR_DIAGNOSTICO_ALERGENO (IDDIAGNOSTICO);
CREATE NONCLUSTERED INDEX IX_TR_DIAGNOSTICOALERGENO_IDALERGENO    ON TR_DIAGNOSTICO_ALERGENO (IDALERGENO);

CREATE NONCLUSTERED INDEX IX_TR_DIAGNOSTICORECOM_IDDIAGNOSTICO ON TR_DIAGNOSTICO_RECOMENDACION (IDDIAGNOSTICO);
CREATE NONCLUSTERED INDEX IX_TR_DIAGNOSTICORECOM_IDPRODUCTO    ON TR_DIAGNOSTICO_RECOMENDACION (IDPRODUCTO);

CREATE NONCLUSTERED INDEX IX_TM_PEDIDO_IDUSUARIO ON TM_PEDIDO (IDUSUARIO);

CREATE NONCLUSTERED INDEX IX_TD_PEDIDODETALLE_IDPEDIDO   ON TD_PEDIDO_DETALLE (IDPEDIDO);
CREATE NONCLUSTERED INDEX IX_TD_PEDIDODETALLE_IDPRODUCTO ON TD_PEDIDO_DETALLE (IDPRODUCTO);
GO


