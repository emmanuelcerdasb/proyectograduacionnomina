# Sistema de Nómina AraWeb CR

Sistema de gestión de nómina para empresas costarricenses. Desarrollado con ASP.NET MVC 5 / .NET Framework 4.7.2 / SQL Server / Entity Framework 6.

---

## Requisitos de software

| Componente | Versión mínima |
|-----------|----------------|
| Windows | 10 / 11 / Server 2016+ |
| Visual Studio | 2019 o 2022 (Community o superior) |
| .NET Framework | 4.7.2 |
| SQL Server | 2017 o superior (Express válido) |
| SQL Server Management Studio | 18+ |
| IIS / IIS Express | 10+ |

---

## Instalación local (desarrollo)

### 1. Clonar el repositorio

```bash
git clone <URL-del-repositorio>
cd ProyectoGraduacionNomina
```

### 2. Crear la base de datos

1. Abrir **SQL Server Management Studio**.
2. Conectarse a la instancia local (ej: `TI-MARCO\SQLEXPRESS`).
3. Ejecutar el script de estructura:
   ```
   docs/BD_Nomina_estructura.sql
   ```
4. Ejecutar el script de datos iniciales:
   ```
   docs/BD_Nomina_seed.sql
   ```

### 3. Configurar la conexión

Copiar la plantilla de configuración:

```
ProyectoGraduacionNomina/Web.config.example  →  ProyectoGraduacionNomina/Web.config
```

Editar `Web.config` y actualizar la cadena de conexión:

```xml
<connectionStrings>
  <add name="BD_NominaEntities"
       connectionString="metadata=res://*/Model1.csdl|res://*/Model1.ssdl|res://*/Model1.msl;
                         provider=System.Data.SqlClient;
                         provider connection string=&quot;
                         data source=TU_SERVIDOR\SQLEXPRESS;
                         initial catalog=BD_Nomina;
                         integrated security=True;
                         MultipleActiveResultSets=True;
                         App=EntityFramework&quot;"
       providerName="System.Data.EntityClient" />
</connectionStrings>
```

Cambiar `TU_SERVIDOR\SQLEXPRESS` por el nombre real de tu instancia SQL Server.

### 4. Restaurar paquetes NuGet

En Visual Studio: **Herramientas → Administrador de paquetes NuGet → Restaurar paquetes de la solución**

O desde la terminal:

```
nuget restore ProyectoGraduacionNomina.sln
```

### 5. Compilar y ejecutar

- Abrir `ProyectoGraduacionNomina.sln` en Visual Studio.
- Presionar **Ctrl+Shift+B** para compilar.
- Presionar **F5** para ejecutar con IIS Express.

---

## Credenciales iniciales

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| `admin` | `Admin2024!` | Administrador |

> **Importante:** Al primer inicio de sesión el sistema pedirá cambio de contraseña.

---

## Roles del sistema

| Rol | Permisos principales |
|-----|---------------------|
| **Administrador** | Acceso total: mantenimientos, nómina, seguridad, reportes |
| **RRHH** | Vacaciones, evaluaciones, liquidaciones, consultas, reportes |
| **Jefe / Jefa** | Aprobar horas extra, ver asistencia y nómina de su área |
| **Colaborador** | Marcación, solicitar horas extra, ver sus propias solicitudes |

---

## Estructura de carpetas

```
ProyectoGraduacionNomina/
├── Controllers/        # Lógica de negocio por módulo
├── Views/              # Vistas Razor por módulo
├── Servicios/          # Servicios de cálculo (nómina, liquidación, aguinaldo)
├── Helpers/            # Utilidades: bitácora, contraseñas, notificaciones
├── Content/            # CSS e imágenes
├── Scripts/            # JavaScript
├── Models/             # ViewModels adicionales
└── Model1.edmx         # Modelo Entity Framework (Database-First)
docs/
├── BD_Nomina_seed.sql  # Datos iniciales de catálogos
└── manual_usuario.md   # Manual de usuario por rol
```

---

## Módulos implementados

| Módulo | Descripción |
|--------|-------------|
| Autenticación | Login con PBKDF2, sesiones, bitácora, cambio de contraseña |
| Empleados | CRUD con datos personales y laborales |
| Marcación | Entrada / salida con registro de asistencia |
| Nómina | Cálculo mensual: salario bruto, deducciones CCSS/renta, neto |
| Horas Extra | Solicitud, aprobación y cálculo por clase (50%/100%) |
| Vacaciones | Solicitud, aprobación, historial, cálculo días hábiles |
| Aguinaldo | Cálculo proporcional (art. 166 CT) |
| Liquidaciones | Preaviso, cesantía, aguinaldo y vacaciones proporcionales (CT) |
| Evaluaciones | 5 criterios 1-10, promedio automático, calificación |
| Consultas | Perfil 360° del empleado |
| Reportes | Nómina por período, aguinaldo por año, asistencia |
| Seguridad | Sesiones activas, bitácora con filtros, gestión de credenciales |
| Mantenimientos | Catálogos del sistema (roles, puestos, departamentos, etc.) |

---

## Tecnologías utilizadas

- **Backend:** ASP.NET MVC 5, C#, .NET Framework 4.7.2
- **ORM:** Entity Framework 6 (Database-First)
- **Base de datos:** SQL Server (compatible Express)
- **Frontend:** Bootstrap 5.3, Bootstrap Icons, JavaScript vanilla
- **Seguridad:** Forms Authentication, PBKDF2 (SHA-256, 100k iteraciones)
- **Ley aplicada:** Código de Trabajo de Costa Rica (arts. 28, 29, 153, 166)
