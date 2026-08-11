# 🔗 GUÍA DE VERIFICACIÓN: CONECTAR CON EL BACKEND

## 🎯 Objetivo

Verificar que el backend está devolviendo los datos en el formato correcto que espera la aplicación móvil.

---

## 🔧 Paso 1: Verificar Endpoint GET /api/proyectos

### Con Postman o Insomnia

```
GET /api/proyectos
Authorization: Bearer {tu_token_aqui}
Accept: application/json
```

### Respuesta Esperada

```json
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8

[
  {
    "id": 1,
    "codigo": "PROJ-2023-001",
    "nombre": "Migración Cloud Infraestructura",
    "cliente": "Grupo Financiero del...",
    "estado": "En Progreso",
    "presupuesto": 150000.00,
    "horas": 1200.00,
    "lider": "Marta Reyes",
    "numeroRecursos": 8,
    "tipo": "Cloud Migration",
    "fechaInicio": "2024-01-01",
    "fechaFin": "2024-11-15"
  },
  {
    "id": 2,
    "codigo": "PROJ-2023-002",
    "nombre": "Calidad Fabrica de software",
    "cliente": "CLARO - CONECEL S.A.",
    "estado": "Completado",
    "presupuesto": 85000.00,
    "horas": 640.00,
    "lider": "Juan Carlos Pérez",
    "numeroRecursos": 5,
    "tipo": "Desarrollo Web",
    "fechaInicio": "2024-03-15",
    "fechaFin": "2024-09-30"
  }
]
```

### ✅ Criterios de Validación

- [ ] Status 200 OK
- [ ] Es un array JSON `[]`
- [ ] Cada objeto tiene **12 propiedades**:
  - [x] `id` (número)
  - [x] `codigo` (texto)
  - [x] `nombre` (texto)
  - [x] `cliente` (texto)
  - [x] `estado` (texto)
  - [x] `presupuesto` (número/decimal)
  - [x] `horas` (número/decimal)
  - [x] `lider` (texto - **CRÍTICO**)
  - [x] `numeroRecursos` (número)
  - [x] `tipo` (texto)
  - [x] `fechaInicio` (fecha YYYY-MM-DD o null)
  - [x] `fechaFin` (fecha YYYY-MM-DD o null)

---

## 🔧 Paso 2: Verificar Endpoint GET /api/proyectos/lookups

### Con Postman o Insomnia

```
GET /api/proyectos/lookups
Authorization: Bearer {tu_token_aqui}
Accept: application/json
```

### Respuesta Esperada

```json
HTTP/1.1 200 OK

{
  "clientes": [
    { "id": 1, "nombre": "Grupo Financiero del..." },
    { "id": 2, "nombre": "CLARO - CONECEL S.A." },
    { "id": 3, "nombre": "BANRURAL" }
  ],
  "lideres": [
    { "id": 1, "nombre": "Marta Reyes" },
    { "id": 2, "nombre": "Juan Carlos Pérez" },
    { "id": 3, "nombre": "María González" }
  ],
  "empleados": [
    { "id": 1, "nombre": "Empleado 1" },
    { "id": 2, "nombre": "Empleado 2" }
  ],
  "estados": [
    { "id": 1, "nombre": "Activo" },
    { "id": 2, "nombre": "Inactivo" },
    { "id": 3, "nombre": "En Progreso" },
    { "id": 4, "nombre": "Completado" },
    { "id": 5, "nombre": "En Riesgo" }
  ],
  "tipos": [
    { "id": 1, "nombre": "Cloud Migration" },
    { "id": 2, "nombre": "Desarrollo Web" },
    { "id": 3, "nombre": "Consultoría" }
  ]
}
```

### ✅ Criterios de Validación

- [ ] Status 200 OK
- [ ] Contiene 5 propiedades: `clientes`, `lideres`, `empleados`, `estados`, `tipos`
- [ ] Cada lista contiene objetos con `id` y `nombre`
- [ ] No debe estar vacía (al menos 1 elemento por lista)

---

## 🔧 Paso 3: Verificar Mapeo en ProyectosViewModel

### Código en ProyectosViewModel.cs

```csharp
[RelayCommand]
private async Task CargarProyectosAsync()
{
    IsBusy = true;
    ErrorMessage = string.Empty;
    try
    {
        // ✅ ESTO DEBE DEVOLVER List<ProyectoResponse>
        var proyectos = await _apiService.GetAsync<List<ProyectoResponse>>("api/proyectos");
        
        Proyectos.Clear();
        ProyectosFiltrados.Clear();

        // ✅ MAPEO: ProyectoResponse → ProyectoItem
        foreach (var proyecto in proyectos ?? new List<ProyectoResponse>())
        {
            var item = new ProyectoItem
            {
                Id = proyecto.Id,
                Codigo = proyecto.Codigo,
                Nombre = proyecto.Nombre,
                Cliente = proyecto.Cliente,
                Estado = proyecto.Estado ?? "Activo",
                Presupuesto = proyecto.Presupuesto ?? 0m,
                Horas = proyecto.Horas ?? 0m,
                Lider = proyecto.Lider,                    // ✅ CRÍTICO
                NumeroRecursos = proyecto.NumeroRecursos,   // ✅ CRÍTICO
                Tipo = proyecto.Tipo,
                FechaInicio = proyecto.FechaInicio?.ToString("dd/MM/yyyy") ?? string.Empty,
                FechaFin = proyecto.FechaFin?.ToString("dd/MM/yyyy") ?? string.Empty
            };
            Proyectos.Add(item);
        }

        ProyectosCount = Proyectos.Count;
        AplicarFiltros();
    }
    catch (Exception ex)
    {
        ErrorMessage = $"Error al cargar proyectos: {ex.Message}";
        System.Diagnostics.Debug.WriteLine($"ERROR: {ex}");
    }
    finally
    {
        IsBusy = false;
    }
}
```

### ✅ Verificar en Código

1. Abre `tmr-mobile/ViewModels/ProyectosViewModel.cs`
2. Busca el método `CargarProyectosAsync()`
3. Verifica que llama:
   ```csharp
   var proyectos = await _apiService.GetAsync<List<ProyectoResponse>>("api/proyectos");
   ```
4. Verifica que mapea todos los campos:
   - [ ] `Id`
   - [ ] `Codigo`
   - [ ] `Nombre`
   - [ ] `Cliente`
   - [ ] `Estado`
   - [ ] `Presupuesto`
   - [ ] `Horas`
   - [ ] `Lider` ← **IMPORTANTE**
   - [ ] `NumeroRecursos` ← **IMPORTANTE**
   - [ ] `Tipo`
   - [ ] `FechaInicio`
   - [ ] `FechaFin`

---

## 🧪 Paso 4: Testing en la Aplicación

### En Android/iOS Emulador

```bash
# Compilar
dotnet build -f net10.0-android -c Debug

# Ejecutar
dotnet maui run -f net10.0-android
```

### Qué Observar

1. **En ProyectosPage**:
   - [ ] Se muestran tarjetas de proyectos
   - [ ] Cada tarjeta muestra:
     - Nombre del proyecto (ej: "Migración Cloud Infraestructura")
     - Cliente
     - Estado con color dinámico
     - Avatar inicial del líder (ej: "M" para Marta)
     - Número de recursos
     - Rango de fechas

2. **Si No Visualiza**:
   - Abre **Debug Console** de VS Code
   - Busca mensajes de error en Proyectos
   - Revisa que ApiService está usando el token correcto

### Debug Log

Agrega esto en ProyectosViewModel para ver qué está ocurriendo:

```csharp
private async Task CargarProyectosAsync()
{
    try
    {
        var proyectos = await _apiService.GetAsync<List<ProyectoResponse>>("api/proyectos");
        
        // ✅ VER EN DEBUG
        System.Diagnostics.Debug.WriteLine($"✓ Proyectos cargados: {proyectos?.Count}");
        
        foreach (var p in proyectos ?? new())
        {
            System.Diagnostics.Debug.WriteLine($"  - {p.Codigo}: {p.Nombre}");
            System.Diagnostics.Debug.WriteLine($"    Líder: {p.Lider}");
            System.Diagnostics.Debug.WriteLine($"    Recursos: {p.NumeroRecursos}");
        }
        
        // ... resto del código
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"✗ ERROR: {ex.Message}");
        System.Diagnostics.Debug.WriteLine(ex.StackTrace);
    }
}
```

---

## 📋 Checklist Final

### Backend

- [ ] Endpoint `GET /api/proyectos` devuelve array con 12 campos
- [ ] Endpoint incluye campos: `lider` y `numeroRecursos`
- [ ] Endpoint `GET /api/proyectos/lookups` devuelve listas
- [ ] Autenticación funcionando (token válido)
- [ ] CORS configurado (si la app y API están en dominios diferentes)

### Frontend

- [ ] DTOs creados en tmr-shared:
  - [ ] `ProyectoResponse.cs`
  - [ ] `ProyectoLookupsResponse.cs`
  - [ ] `ProyectoCreateUpdateRequest.cs`
- [ ] Modelos creados en tmr-mobile:
  - [ ] `ProyectoItem.cs`
- [ ] Converters registrados en `App.xaml`
- [ ] `ProyectosViewModel` tiene método `CargarProyectosAsync()`
- [ ] Compilación exitosa (0 errores)
- [ ] App visualiza proyectos en la UI

### Testing

- [ ] Proyectos se cargan y visualizan
- [ ] Estados tienen colores correctos
- [ ] Iniciales del líder se muestran en avatar
- [ ] Filtros funcionan
- [ ] Números de recursos se muestran

---

## 🆘 Troubleshooting

### Problem: "HTTP 401 Unauthorized"

**Causa**: Token expirado o no válido  
**Solución**:
1. Verificar que el usuario está autenticado
2. Revisar que ApiService está inyectando el token correctamente

```csharp
// En ApiService.cs
var token = SecureStorage.GetAsync("auth_token");
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);
```

### Problem: "HTTP 404 Not Found"

**Causa**: Endpoint no existe en el backend  
**Solución**:
1. Verificar la ruta: `/api/proyectos` (no `proyectos` sin `/api`)
2. Revisar que el controlador tiene el atributo `[Route("api/[controller]")]`

### Problem: "0 proyectos cargados"

**Causa 1**: API devuelve array vacío  
**Solución**: Verificar que hay datos en la BD del backend

**Causa 2**: Propiedad `Lider` es null  
**Causa**: Mapeo incompleto en backend  
**Solución**: Asegurarse que ProyectoResponse incluye Lider

### Problem: "No se colorean estados"

**Causa**: Converters no están registrados  
**Solución**: Revisar que `App.xaml` tiene:
```xml
<converters:EstadoColorConverter x:Key="EstadoColorConverter" />
<converters:EstadoTextColorConverter x:Key="EstadoTextColorConverter" />
```

### Problem: "NullReferenceException en Lider"

**Causa**: El backend devuelve `null` para Lider  
**Solución**: Agregar fallback en ViewModel:
```csharp
Lider = proyecto.Lider ?? "Asignado",
```

---

## 📞 Contacto

Si necesitas help, revisa:
1. `ESTRUCTURA_MODELOS_PROYECTOS.md` - Guía completa de la arquitectura
2. `REFERENCIA_TECNICA_CONVERTERS.md` - Info de converters
3. `ProyectosViewModel.cs` - Código fuente del ViewModel

---

**Última actualización**: 2026-08-11  
**Versión**: 1.0
