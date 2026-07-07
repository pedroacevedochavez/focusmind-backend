-- Prerrequisito: ejecutar 03_Seed_Catalogos.sql antes que este script.
-- ── 2. Productos (PRODUCTOS_MOCK, data/productos/productos.ts) ─────────────

-- HU-21: TR_PRODUCTO_ALERGENO pasó de UNIQUE(IDPRODUCTO, IDALERGENO) de tabla a un índice
-- único FILTRADO (WHERE ACTIVO = 1) — ver 01_Schema_Tablas.sql. Este seed no se ve afectado:
-- inserta cada par (producto, alérgeno) exactamente una vez sobre un esquema vacío, así que el
-- índice filtrado se comporta igual que el UNIQUE anterior en este caso de uso. Tampoco llama a
-- ninguno de los 3 usp_Desactivar_Producto*_X_Producto nuevos (esos son exclusivos del flujo de
-- edición, PUT /api/productos/:id). El SET de abajo se agrega solo por higiene/consistencia con
-- 02_Schema_ProcedimientosAlmacenados.sql, no porque este script lo necesitara para funcionar.
SET QUOTED_IDENTIFIER ON;
GO
SET ANSI_NULLS ON;
GO

DECLARE @USUARIOSISTEMA INT = 1;
DECLARE @IDTEMP INT, @IDPRODUCTO INT, @IDCATEGORIA INT, @IDOBJETIVO INT, @IDALERGENO INT;

-- Producto 1 — NeuroFocus Alpha
SELECT @IDCATEGORIA = IDCATEGORIA FROM TM_CATEGORIA WHERE CODIGO = 'memoria';
SELECT @IDOBJETIVO  = IDOBJETIVO  FROM TM_OBJETIVO  WHERE CODIGO = 'mejorar-memoria';

EXEC usp_InsertarProducto
    @NOMBRE = 'NeuroFocus Alpha', @MARCA = 'NutraLab Perú',
    @IDCATEGORIA = @IDCATEGORIA, @IDOBJETIVO = @IDOBJETIVO, @PRECIO = 119.90,
    @DESCRIPCION = 'Fórmula avanzada con Bacopa Monnieri y Fosfatidilserina para potenciar la memoria de trabajo y la retención de información a largo plazo.',
    @DOSISRECOMENDADA = '1 cápsula al día, junto con el desayuno.',
    @URLIMAGEN = 'https://focusmind-s3-pics-bucket.s3.us-east-1.amazonaws.com/nootropico-memoria-alpha.jpg',
    @REGISTROSANITARIO = 'N-0012456-2024', @ENTIDADREGISTRO = 'DIGESA', @STOCK = 40,
    @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTO = @IDPRODUCTO OUTPUT;

EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Bacopa Monnieri 300mg',        @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Fosfatidilserina 100mg',       @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Vitamina B6 5mg',              @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Ginkgo Biloba 120mg',          @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;

EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'No recomendado en gestantes y mujeres en periodo de lactancia.',                          @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'Consultar con un médico si se encuentra en tratamiento con anticoagulantes.',             @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;

SELECT @IDALERGENO = IDALERGENO FROM TM_ALERGENO WHERE NOMBRE = 'Soya';
EXEC usp_InsertarProductoAlergeno @IDPRODUCTO = @IDPRODUCTO, @IDALERGENO = @IDALERGENO, @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOALERGENO = @IDTEMP OUTPUT;
SELECT @IDALERGENO = IDALERGENO FROM TM_ALERGENO WHERE NOMBRE = 'Gluten';
EXEC usp_InsertarProductoAlergeno @IDPRODUCTO = @IDPRODUCTO, @IDALERGENO = @IDALERGENO, @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOALERGENO = @IDTEMP OUTPUT;

-- Producto 2 — ZenCalm Magnesio
SELECT @IDCATEGORIA = IDCATEGORIA FROM TM_CATEGORIA WHERE CODIGO = 'estres';
SELECT @IDOBJETIVO  = IDOBJETIVO  FROM TM_OBJETIVO  WHERE CODIGO = 'reducir-estres';

EXEC usp_InsertarProducto
    @NOMBRE = 'ZenCalm Magnesio', @MARCA = 'Andes Wellness',
    @IDCATEGORIA = @IDCATEGORIA, @IDOBJETIVO = @IDOBJETIVO, @PRECIO = 75.00,
    @DESCRIPCION = 'Magnesio bisglicinato de alta absorción combinado con L-Teanina para reducir los niveles de cortisol y promover la calma mental durante el día.',
    @DOSISRECOMENDADA = '2 cápsulas al día, preferiblemente en la noche.',
    @URLIMAGEN = 'https://focusmind-s3-pics-bucket.s3.us-east-1.amazonaws.com/suplemento-estres-magnesio.jpg',
    @REGISTROSANITARIO = 'N-0023871-2023', @ENTIDADREGISTRO = 'DIGESA', @STOCK = 60,
    @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTO = @IDPRODUCTO OUTPUT;

EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Magnesio Bisglicinato 200mg', @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'L-Teanina 150mg',             @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Vitamina B6 2mg',             @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;

EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'No combinar con medicamentos sedantes sin supervisión médica.', @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;
-- Sin alérgenos declarados.

-- Producto 3 — EnergyBoost Cordyceps
SELECT @IDCATEGORIA = IDCATEGORIA FROM TM_CATEGORIA WHERE CODIGO = 'energia';
SELECT @IDOBJETIVO  = IDOBJETIVO  FROM TM_OBJETIVO  WHERE CODIGO = 'aumentar-energia';

EXEC usp_InsertarProducto
    @NOMBRE = 'EnergyBoost Cordyceps', @MARCA = 'PerúVital Labs',
    @IDCATEGORIA = @IDCATEGORIA, @IDOBJETIVO = @IDOBJETIVO, @PRECIO = 145.50,
    @DESCRIPCION = 'Extracto micelar de Cordyceps Militaris combinado con Rhodiola Rosea para incrementar los niveles de energía física y mental sin el efecto rebote de la cafeína.',
    @DOSISRECOMENDADA = '1 cápsula en la mañana, alejada de los alimentos.',
    @URLIMAGEN = 'https://focusmind-s3-pics-bucket.s3.us-east-1.amazonaws.com/suplemento-energia-cordyceps.jpg',
    @REGISTROSANITARIO = 'M-0098712-2024', @ENTIDADREGISTRO = 'DIGEMID', @STOCK = 25,
    @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTO = @IDPRODUCTO OUTPUT;

EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Cordyceps Militaris 500mg', @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Rhodiola Rosea 200mg',      @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Cafeína Anhidra 50mg',      @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;

EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'No recomendado para personas con hipertensión no controlada.', @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'Evitar su consumo después de las 4:00 p.m.',                    @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;
-- Sin alérgenos declarados.

-- Producto 4 — DeepSleep Melatonina
SELECT @IDCATEGORIA = IDCATEGORIA FROM TM_CATEGORIA WHERE CODIGO = 'sueno';
SELECT @IDOBJETIVO  = IDOBJETIVO  FROM TM_OBJETIVO  WHERE CODIGO = 'mejorar-sueno';

EXEC usp_InsertarProducto
    @NOMBRE = 'DeepSleep Melatonina', @MARCA = 'Andes Wellness',
    @IDCATEGORIA = @IDCATEGORIA, @IDOBJETIVO = @IDOBJETIVO, @PRECIO = 58.90,
    @DESCRIPCION = 'Combinación de Melatonina, L-Triptófano y Manzanilla para conciliar el sueño de forma natural y mejorar la calidad del descanso profundo.',
    @DOSISRECOMENDADA = '1 cápsula, 30 minutos antes de dormir.',
    @URLIMAGEN = 'https://focusmind-s3-pics-bucket.s3.us-east-1.amazonaws.com/suplemento-sueno-melatonina.jpg',
    @REGISTROSANITARIO = 'N-0034567-2022', @ENTIDADREGISTRO = 'DIGESA', @STOCK = 50,
    @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTO = @IDPRODUCTO OUTPUT;

EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Melatonina 3mg',                  @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'L-Triptófano 200mg',              @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Extracto de Manzanilla 100mg',    @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;

EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'No conducir ni operar maquinaria pesada después de su consumo.', @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;
-- Sin alérgenos declarados.

-- Producto 5 — MoodLift Omega-3
SELECT @IDCATEGORIA = IDCATEGORIA FROM TM_CATEGORIA WHERE CODIGO = 'animo';
SELECT @IDOBJETIVO  = IDOBJETIVO  FROM TM_OBJETIVO  WHERE CODIGO = 'mejorar-animo';

EXEC usp_InsertarProducto
    @NOMBRE = 'MoodLift Omega-3', @MARCA = 'NutraLab Perú',
    @IDCATEGORIA = @IDCATEGORIA, @IDOBJETIVO = @IDOBJETIVO, @PRECIO = 99.90,
    @DESCRIPCION = 'Aceite de pescado de alta concentración en EPA y DHA, formulado para favorecer el equilibrio del estado de ánimo y la salud cerebral a largo plazo.',
    @DOSISRECOMENDADA = '2 cápsulas al día, junto con las comidas.',
    @URLIMAGEN = 'https://focusmind-s3-pics-bucket.s3.us-east-1.amazonaws.com/suplemento-animo-omega3.jpg',
    @REGISTROSANITARIO = 'N-0045123-2023', @ENTIDADREGISTRO = 'DIGESA', @STOCK = 35,
    @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTO = @IDPRODUCTO OUTPUT;

EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Aceite de Pescado 1000mg', @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'EPA 400mg',                @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'DHA 300mg',                @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Vitamina E 5mg',           @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;

EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'Consultar con un médico si se encuentra en tratamiento con anticoagulantes.', @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;

SELECT @IDALERGENO = IDALERGENO FROM TM_ALERGENO WHERE NOMBRE = 'Pescado';
EXEC usp_InsertarProductoAlergeno @IDPRODUCTO = @IDPRODUCTO, @IDALERGENO = @IDALERGENO, @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOALERGENO = @IDTEMP OUTPUT;

-- Producto 6 — ClarityMind Bacopa (sin registro sanitario, badge ABET 2 — HU-06)
SELECT @IDCATEGORIA = IDCATEGORIA FROM TM_CATEGORIA WHERE CODIGO = 'enfoque';
SELECT @IDOBJETIVO  = IDOBJETIVO  FROM TM_OBJETIVO  WHERE CODIGO = 'aumentar-concentracion';

EXEC usp_InsertarProducto
    @NOMBRE = 'ClarityMind Bacopa', @MARCA = 'BioCognition',
    @IDCATEGORIA = @IDCATEGORIA, @IDOBJETIVO = @IDOBJETIVO, @PRECIO = 64.90,
    @DESCRIPCION = 'Extracto estandarizado de Bacopa Monnieri al 50% de bacósidos, orientado a mejorar la claridad mental y la capacidad de concentración sostenida.',
    @DOSISRECOMENDADA = '1 cápsula al día, junto con alimentos.',
    @URLIMAGEN = 'https://focusmind-s3-pics-bucket.s3.us-east-1.amazonaws.com/nootropico-enfoque-bacopa.jpg',
    @REGISTROSANITARIO = NULL, @ENTIDADREGISTRO = NULL, @STOCK = 20,
    @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTO = @IDPRODUCTO OUTPUT;

EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Bacopa Monnieri 320mg (50% bacósidos)', @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Pimienta Negra (Piperina) 5mg',        @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;

EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'Puede causar malestar estomacal leve en dosis altas.', @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;
-- Sin alérgenos declarados.

-- Producto 7 — MemoryPlus Ginkgo
SELECT @IDCATEGORIA = IDCATEGORIA FROM TM_CATEGORIA WHERE CODIGO = 'memoria';
SELECT @IDOBJETIVO  = IDOBJETIVO  FROM TM_OBJETIVO  WHERE CODIGO = 'mejorar-memoria';

EXEC usp_InsertarProducto
    @NOMBRE = 'MemoryPlus Ginkgo', @MARCA = 'PerúVital Labs',
    @IDCATEGORIA = @IDCATEGORIA, @IDOBJETIVO = @IDOBJETIVO, @PRECIO = 89.50,
    @DESCRIPCION = 'Ginkgo Biloba estandarizado combinado con Colina, diseñado para mejorar la circulación cerebral y la velocidad de procesamiento de información.',
    @DOSISRECOMENDADA = '1 cápsula al día, junto con el almuerzo.',
    @URLIMAGEN = 'https://focusmind-s3-pics-bucket.s3.us-east-1.amazonaws.com/nootropico-memoria-ginkgo.jpg',
    @REGISTROSANITARIO = 'M-0076543-2023', @ENTIDADREGISTRO = 'DIGEMID', @STOCK = 45,
    @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTO = @IDPRODUCTO OUTPUT;

EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Ginkgo Biloba 240mg',            @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Colina (Bitartrato) 150mg',      @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Ácido Fólico 400mcg',            @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;

EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'No recomendado junto con anticoagulantes o antiagregantes plaquetarios.', @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'Suspender su consumo dos semanas antes de cirugías programadas.',         @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;

SELECT @IDALERGENO = IDALERGENO FROM TM_ALERGENO WHERE NOMBRE = 'Gluten';
EXEC usp_InsertarProductoAlergeno @IDPRODUCTO = @IDPRODUCTO, @IDALERGENO = @IDALERGENO, @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOALERGENO = @IDTEMP OUTPUT;

-- Producto 8 — FocusPro L-Teanina
SELECT @IDCATEGORIA = IDCATEGORIA FROM TM_CATEGORIA WHERE CODIGO = 'enfoque';
SELECT @IDOBJETIVO  = IDOBJETIVO  FROM TM_OBJETIVO  WHERE CODIGO = 'aumentar-concentracion';

EXEC usp_InsertarProducto
    @NOMBRE = 'FocusPro L-Teanina', @MARCA = 'BioCognition',
    @IDCATEGORIA = @IDCATEGORIA, @IDOBJETIVO = @IDOBJETIVO, @PRECIO = 110.00,
    @DESCRIPCION = 'L-Teanina pura combinada con Cafeína en proporción 2:1, formulada para lograr un estado de concentración relajada ("flow") sin nerviosismo.',
    @DOSISRECOMENDADA = '1 cápsula en la mañana, antes de iniciar actividades de concentración.',
    @URLIMAGEN = 'https://focusmind-s3-pics-bucket.s3.us-east-1.amazonaws.com/nootropico-enfoque-teanina.jpg',
    @REGISTROSANITARIO = 'N-0056789-2024', @ENTIDADREGISTRO = 'DIGESA', @STOCK = 30,
    @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTO = @IDPRODUCTO OUTPUT;

EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'L-Teanina 200mg',        @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Cafeína Anhidra 100mg',  @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoIngrediente @IDPRODUCTO = @IDPRODUCTO, @INGREDIENTE = 'Vitamina B12 10mcg',     @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOINGREDIENTE = @IDTEMP OUTPUT;

EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'No recomendado en personas sensibles a la cafeína.',                  @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;
EXEC usp_InsertarProductoContraindicacion @IDPRODUCTO = @IDPRODUCTO, @DESCRIPCION = 'Evitar su consumo en las últimas 6 horas antes de dormir.',           @USUARIOCREA = @USUARIOSISTEMA, @IDPRODUCTOCONTRAINDICACION = @IDTEMP OUTPUT;
-- Sin alérgenos declarados.
GO
