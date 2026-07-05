-- Prerrequisito: ejecutar 01_Schema_Tablas.sql y 02_Schema_ProcedimientosAlmacenados.sql antes.
/* ════════════════════════════════════════════════════════════════════════
   FocusMind S.A.C. — Seed de catálogo (8 productos, HU-03/HU-04)
   ════════════════════════════════════════════════════════════════════════
   Prerrequisito: haber ejecutado database/focusmind_ecommerce.sql (crea
   tablas y procedimientos). Este script usa los propios procedimientos
   usp_Insertar* para que el seed ejerza el mismo camino de código que
   usaría el backend .NET (SCOPE_IDENTITY + auditoría estándar incluidos
   automáticamente por cada proc: ACTIVO, USUARIOCREA, FECHACREA).

   AJUSTE S3: URLIMAGEN es la única columna de imagen (la columna IMAGEN de
   ruta local fue eliminada del esquema). Usa un placeholder de bucket S3
   ('URL').

   Pensado para ejecutarse una sola vez sobre un esquema vacío: los
   catálogos (TM_CATEGORIA/TM_OBJETIVO/TM_ALERGENO) tienen UNIQUE en su
   código/nombre, por lo que una segunda ejecución fallaría por duplicados.
   ════════════════════════════════════════════════════════════════════════ */

DECLARE @USUARIOSISTEMA INT = 1; -- Placeholder de usuario/admin que ejecuta el seed.
DECLARE @IDTEMP         INT;     -- Recibe IDs OUTPUT que no se reutilizan (hijos).

-- ── 1. Catálogos base (CATEGORIA_LABELS / OBJETIVO_LABELS de producto.ts) ──
DECLARE @IDCAT INT;
EXEC usp_InsertarCategoria @CODIGO = 'memoria', @NOMBRE = 'Memoria', @USUARIOCREA = @USUARIOSISTEMA, @IDCATEGORIA = @IDCAT OUTPUT;
EXEC usp_InsertarCategoria @CODIGO = 'enfoque', @NOMBRE = 'Enfoque', @USUARIOCREA = @USUARIOSISTEMA, @IDCATEGORIA = @IDCAT OUTPUT;
EXEC usp_InsertarCategoria @CODIGO = 'energia', @NOMBRE = 'Energía', @USUARIOCREA = @USUARIOSISTEMA, @IDCATEGORIA = @IDCAT OUTPUT;
EXEC usp_InsertarCategoria @CODIGO = 'sueno',   @NOMBRE = 'Sueño',   @USUARIOCREA = @USUARIOSISTEMA, @IDCATEGORIA = @IDCAT OUTPUT;
EXEC usp_InsertarCategoria @CODIGO = 'estres',  @NOMBRE = 'Estrés',  @USUARIOCREA = @USUARIOSISTEMA, @IDCATEGORIA = @IDCAT OUTPUT;
EXEC usp_InsertarCategoria @CODIGO = 'animo',   @NOMBRE = 'Ánimo',   @USUARIOCREA = @USUARIOSISTEMA, @IDCATEGORIA = @IDCAT OUTPUT;

DECLARE @IDOBJ INT;
EXEC usp_InsertarObjetivo @CODIGO = 'mejorar-memoria',         @NOMBRE = 'Mejorar la memoria',          @USUARIOCREA = @USUARIOSISTEMA, @IDOBJETIVO = @IDOBJ OUTPUT;
EXEC usp_InsertarObjetivo @CODIGO = 'aumentar-concentracion',  @NOMBRE = 'Aumentar la concentración',   @USUARIOCREA = @USUARIOSISTEMA, @IDOBJETIVO = @IDOBJ OUTPUT;
EXEC usp_InsertarObjetivo @CODIGO = 'reducir-estres',          @NOMBRE = 'Reducir el estrés',           @USUARIOCREA = @USUARIOSISTEMA, @IDOBJETIVO = @IDOBJ OUTPUT;
EXEC usp_InsertarObjetivo @CODIGO = 'mejorar-sueno',           @NOMBRE = 'Mejorar el sueño',            @USUARIOCREA = @USUARIOSISTEMA, @IDOBJETIVO = @IDOBJ OUTPUT;
EXEC usp_InsertarObjetivo @CODIGO = 'aumentar-energia',        @NOMBRE = 'Aumentar la energía',         @USUARIOCREA = @USUARIOSISTEMA, @IDOBJETIVO = @IDOBJ OUTPUT;
EXEC usp_InsertarObjetivo @CODIGO = 'mejorar-animo',           @NOMBRE = 'Mejorar el ánimo',            @USUARIOCREA = @USUARIOSISTEMA, @IDOBJETIVO = @IDOBJ OUTPUT;

-- Alérgenos únicos observados en el catálogo (ProductoService.obtenerAlergenosUnicos()).
DECLARE @IDALG INT;
EXEC usp_InsertarAlergeno @NOMBRE = 'Soya',    @USUARIOCREA = @USUARIOSISTEMA, @IDALERGENO = @IDALG OUTPUT;
EXEC usp_InsertarAlergeno @NOMBRE = 'Gluten',  @USUARIOCREA = @USUARIOSISTEMA, @IDALERGENO = @IDALG OUTPUT;
EXEC usp_InsertarAlergeno @NOMBRE = 'Pescado', @USUARIOCREA = @USUARIOSISTEMA, @IDALERGENO = @IDALG OUTPUT;
GO
