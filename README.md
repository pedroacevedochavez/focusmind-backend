# FocusMind - Backend Architecture

Este repositorio contiene la arquitectura del backend para la plataforma FocusMind, estructurada en un modelo multicapa que incluye la API lógica, pruebas unitarias y los esquemas de persistencia de datos.

## 📁 Estructura del Proyecto

*   **`database/`**: Contiene los scripts de inicialización, tablas y esquemas para **SQL Server**.
*   **`src/`**: Código fuente principal estructurado en capas (.NET API, Core, Business, Data).
*   **`tests/`**: Suite de pruebas unitarias implementadas para asegurar la integridad del sistema.

---

## 🛠️ Comandos Esenciales (.NET CLI)

Para gestionar y compilar el proyecto localmente, utiliza los siguientes comandos desde la raíz del repositorio:

### 1. Restaurar dependencias
```bash
dotnet restore
```

### 2. Compilar la solución entera
```bash
dotnet build
```

### 3. Ejecutar las pruebas unitarias
```bash
dotnet test
```

### 4. Levantar la API en entorno local
```bash
dotnet run --project src/FocusMind.API/FocusMind.API.csproj
```

### 5. Generar el paquete optimizado para producción
```bash
dotnet publish src/FocusMind.API/FocusMind.API.csproj -c Release -o carpeta_despliegue
```

---
💡 *Nota: Recuerda configurar la cadena de conexión correspondiente a tu instancia de SQL Server en el archivo `appsettings.json` antes de iniciar el entorno de desarrollo.*
