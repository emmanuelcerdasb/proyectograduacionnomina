# Guia de Deploy — Sistema Nomina AraWeb CR

## Opcion A: IIS en servidor Windows (produccion)

### Requisitos del servidor
- Windows Server 2016+ con IIS habilitado
- .NET Framework 4.7.2 instalado
- SQL Server 2017+ (o acceso a instancia remota)
- Certificado SSL para HTTPS (obligatorio en produccion)

### Pasos

**1. Publicar desde Visual Studio**

```
Build → Publish ProyectoGraduacionNomina
→ Folder: C:\inetpub\wwwroot\Nomina
```

**2. Crear sitio en IIS**

1. Abrir IIS Manager
2. Agregar sitio web:
   - Nombre: `SistemaNomina`
   - Ruta fisica: `C:\inetpub\wwwroot\Nomina`
   - Puerto: 443 (HTTPS) con certificado SSL
3. Application Pool: `.NET v4.0 Classic` o `ASP.NET v4.0`

**3. Configurar Web.config de produccion**

El `Web.Release.config` ya aplica automaticamente:
- `requireSSL="true"` en cookies
- `debug="false"` en compilacion

Solo editar la cadena de conexion con el servidor SQL de produccion.

**4. Permisos de carpeta**

El usuario del Application Pool necesita permiso de lectura en la carpeta publicada:

```
icacls "C:\inetpub\wwwroot\Nomina" /grant "IIS AppPool\SistemaNomina:(OI)(CI)R"
```

---

## Opcion B: IIS Express (desarrollo local)

Solo presionar **F5** en Visual Studio. No requiere configuracion adicional.

---

## Variables de configuracion (Web.config)

| Clave | Descripcion | Valor ejemplo |
|-------|-------------|---------------|
| `data source` | Instancia SQL Server | `TI-MARCO\SQLEXPRESS` |
| `initial catalog` | Nombre de la BD | `BD_Nomina` |
| `integrated security` | Autenticacion Windows | `True` |
| `forms timeout` | Minutos de sesion activa | `60` |

---

## Checklist pre-produccion

- [ ] Cadena de conexion apunta al servidor de produccion
- [ ] `Web.Release.config` activo (sin `debug="true"`)
- [ ] Certificado SSL instalado en IIS
- [ ] Backup de BD antes de primer deploy
- [ ] Script seed ejecutado en BD de produccion
- [ ] Contraseña admin cambiada del valor inicial
- [ ] Logs de IIS habilitados
- [ ] Firewall: solo puertos 80 y 443 abiertos

---

## Backup de base de datos

Script de backup rapido en SQL Server:

```sql
BACKUP DATABASE BD_Nomina
TO DISK = 'C:\Backups\BD_Nomina_' + CONVERT(VARCHAR, GETDATE(), 112) + '.bak'
WITH COMPRESSION, STATS = 10;
```

Se recomienda programar este script diariamente con SQL Server Agent.
