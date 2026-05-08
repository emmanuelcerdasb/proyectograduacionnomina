# Manual de Usuario — Sistema de Nomina AraWeb CR

---

## Acceso al sistema

1. Abrir el navegador y navegar a la URL del sistema.
2. Ingresar usuario y contrasena.
3. Si es el primer ingreso o se requiere cambio, el sistema redirige automaticamente a la pantalla de cambio de contrasena.

---

## Roles y accesos

| Rol | Que puede hacer |
|-----|----------------|
| **Administrador** | Todo: configuracion, nomina, reportes, seguridad |
| **RRHH** | Vacaciones, evaluaciones, liquidaciones, consultas, reportes |
| **Jefe / Jefa** | Aprobar horas extra, ver asistencia y nominas |
| **Colaborador** | Marcar entrada/salida, solicitar horas extra |

---

## Dashboard (Inicio)

Al ingresar se muestra el panel principal con:

- **Empleados activos**: total de colaboradores en estado Activo.
- **Vacaciones pendientes**: solicitudes que esperan aprobacion (clic lleva al modulo).
- **Horas extra por aprobar**: solicitudes sin aprobar (clic lleva al modulo).
- **Nomina del mes**: total pagado y numero de empleados procesados.
- **Resumen del ano**: aguinaldos calculados, liquidaciones y evaluaciones.
- **Tabla de solicitudes de vacaciones pendientes** con boton Revisar directo.

La **campana** en la barra superior muestra el total de pendientes y lleva a Vacaciones.

---

## Modulo: Marcacion

**Ruta:** GENERAL → Marcar Entrada / Salida

1. El empleado hace clic en **Marcar Entrada** al llegar.
2. Al salir hace clic en **Marcar Salida**.
3. El sistema registra hora exacta y calcula horas trabajadas.

---

## Modulo: Horas Extra

**Ruta:** GENERAL → Horas Extras

### Solicitar horas extra (Colaborador)
1. Hacer clic en **Nueva solicitud**.
2. Seleccionar clase (diurna 50%, nocturna 100%, feriado 100%).
3. Ingresar fecha y cantidad de horas (maximo 12).
4. Enviar — queda en estado Pendiente.

### Aprobar / Rechazar (Jefe, Admin, RRHH)
1. Ir a **Solicitudes pendientes**.
2. Hacer clic en **Aprobar** o **Rechazar** en cada solicitud.

---

## Modulo: Vacaciones

**Ruta:** RRHH → Vacaciones

### Solicitar vacaciones
1. Hacer clic en **Solicitar vacaciones**.
2. Seleccionar empleado, fecha inicio y fecha fin.
3. El sistema calcula dias habiles automaticamente.
4. Confirmar — queda en estado Pendiente.

### Aprobar / Rechazar
1. Hacer clic en el nombre de la solicitud.
2. En la pantalla de detalle, hacer clic en **Aprobar** o **Rechazar**.
3. Ingresar comentario y confirmar.

---

## Modulo: Nomina

**Ruta:** GESTION → Nominas

### Calcular nomina mensual
1. Hacer clic en **Calcular nomina**.
2. Seleccionar empleado, mes y ano.
3. El sistema muestra el detalle: salario bruto, deducciones CCSS, renta y salario neto.
4. Si el calculo es correcto, hacer clic en **Guardar nomina**.

> Una nomina ya guardada no puede recalcularse para el mismo periodo.

---

## Modulo: Aguinaldo

**Ruta:** RRHH → Aguinaldo

1. Seleccionar empleado y ano.
2. El sistema calcula el monto proporcional segun los salarios pagados entre diciembre del ano anterior y noviembre del ano actual.
3. Hacer clic en **Guardar aguinaldo** para registrarlo.

**Fecha limite de pago:** 20 de diciembre de cada ano (art. 166 CT).

---

## Modulo: Liquidaciones

**Ruta:** RRHH → Liquidaciones

1. Seleccionar empleado, tipo de liquidacion y fecha.
2. El sistema calcula automaticamente:
   - **Preaviso** (art. 28 CT): 0, 7, 14 o 30 dias segun antiguedad.
   - **Cesantia** (art. 29 CT): escala 20-22 dias por ano, maximo 8 anos.
   - **Aguinaldo proporcional**: meses trabajados en el periodo.
   - **Vacaciones proporcionales**: semanas / 50 × 14 dias.
3. Revisar el detalle con referencias legales.
4. Hacer clic en **Guardar liquidacion**.

---

## Modulo: Evaluacion de Personal

**Ruta:** RRHH → Evaluaciones

1. Hacer clic en **Nueva evaluacion**.
2. Seleccionar empleado, ano y semestre.
3. Calificar 5 criterios del 1 al 10:
   - Puntualidad
   - Calidad del trabajo
   - Trabajo en equipo
   - Iniciativa
   - Cumplimiento
4. El promedio y la calificacion se calculan automaticamente:
   - 9-10: **Excelente**
   - 7-8: **Bueno**
   - 5-6: **Regular**
   - Menos de 5: **Deficiente**
5. Ingresar evaluador y comentarios.

---

## Modulo: Consultas — Perfil 360 del Empleado

**Ruta:** SISTEMA → Consultas → Perfil del Empleado

1. Seleccionar el empleado en el desplegable.
2. El sistema muestra en una sola pantalla:
   - Datos personales y laborales
   - Ultimas 6 nominas
   - Ultimas 5 vacaciones
   - Ultimas 4 evaluaciones
   - Todas las liquidaciones
   - Ultimos 3 aguinaldos
   - Ultimas 5 horas extra aprobadas

---

## Modulo: Reportes

**Ruta:** SISTEMA → Reportes

| Reporte | Descripcion |
|---------|-------------|
| Asistencia por empleado | Marcaciones en un rango de fechas |
| Resumen mensual | Indicadores de asistencia del mes |
| Nomina por periodo | Todos los empleados de un mes/ano |
| Aguinaldo por ano | Consolidado de aguinaldos calculados |

Todos los reportes tienen boton **Imprimir** para impresion directa desde el navegador.

---

## Modulo: Seguridad (solo Administrador)

**Ruta:** SISTEMA → Seguridad

### Credenciales
- Crear, editar y desactivar usuarios.
- Resetear contrasena (la nueva temporal es `Temporal123!`, el usuario debe cambiarla al primer login).

### Sesiones Activas
- Ver todos los usuarios conectados en tiempo real (IP, dispositivo, hora de inicio).
- Cerrar sesion de cualquier usuario manualmente.

### Bitacora
- Historial completo de acciones del sistema.
- Filtros por fecha, rol y tipo de accion.
- Paginacion de 50 registros por pagina.

---

## Modulo: Mantenimientos (solo Administrador)

**Ruta:** SISTEMA → Mantenimientos

Gestion de catalogos del sistema:

| Catalogo | Para que sirve |
|----------|----------------|
| Personas | Datos personales de empleados |
| Empleados | Vinculo persona-puesto-jornada-credencial |
| Departamentos | Estructura organizacional |
| Puestos | Cargos con salario base |
| Jornadas | Tipos de horario laboral |
| Tipos de liquidacion | Motivos de salida y reglas CT aplicables |
| Credenciales | Usuarios y roles de acceso |

---

## Cambiar contrasena

**Ruta:** Icono de usuario (esquina superior derecha) → Perfil → o automatico al primer login

1. Ingresar la contrasena actual.
2. Ingresar la nueva contrasena (minimo 8 caracteres).
3. Confirmar cambio.

---

## Preguntas frecuentes

**¿Por que no veo algunos modulos?**
Los modulos disponibles dependen del rol asignado. Consultar con el administrador.

**¿Que pasa si ingreso mal la contrasena?**
El sistema muestra un mensaje de error. No hay bloqueo automatico en la version actual.

**¿Puedo imprimir los reportes?**
Si. Todos los reportes de resultados tienen un boton Imprimir que oculta el menu lateral y la barra superior para una impresion limpia.

**¿Con que frecuencia se calcula la nomina?**
El sistema permite calcular nomina mensual. El administrador decide cuando ejecutarla y guardarla.
