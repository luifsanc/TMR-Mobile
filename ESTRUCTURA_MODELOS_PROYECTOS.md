# 📦 Estructura de Modelos y DTOs - Módulo Proyectos

## Resumen Ejecutivo

Para que el módulo de Proyectos visualice correctamente los datos, se han creado varias capas de modelos:

1. **DTOs en tmr-shared** (Compartidos entre backend y frontend)
2. **Modelos en tmr-mobile** (Específicos de la UI mobile)
3. **Transformación de datos** (ProyectoResponse → ProyectoItem)

---

## 📊 Arquitectura de Capas

```
┌─────────────────────────────────────────────────────────┐
│                    Backend (tmr-backend)                │
│                   GET /api/proyectos                    │
└──────────────────────────┬──────────────────────────────┘
                           │ JSON Response
                           ▼
┌─────────────────────────────────────────────────────────┐
│         ProyectoResponse (tmr-shared/DTOs)              │
│  ├─ Id                                                   │
│  ├─ Codigo                                               │
│  ├─ Nombre                                               │
│  ├─ Cliente                                              │
│  ├─ Estado                                               │
│  ├─ Presupuesto                                          │
│  ├─ Horas                                                │
│  ├─ Lider                                                │
│  ├─ NumeroRecursos                                       │
│  ├─ Tipo                                                 │
│  ├─ FechaInicio (DateOnly)                              │
│  └─ FechaFin (DateOnly)                                 │
└──────────────────────────┬──────────────────────────────┘
                           │ Mapeo/Transformación
                           ▼
┌─────────────────────────────────────────────────────────┐
│          ProyectoItem (tmr-mobile/Models)               │
│  ├─ Id                                                   │
│  ├─ Codigo                                               │
│  ├─ Nombre                                               │
│  ├─ Cliente                                              │
│  ├─ Estado                                               │
│  ├─ Presupuesto (decimal)                               │
│  ├─ Horas (decimal)                                      │
│  ├─ Lider                                                │
│  ├─ NumeroRecursos                                       │
│  ├─ Tipo                                                 │
│  ├─ FechaInicio (string formateado)                     │
│  ├─ FechaFin (string formateado)                        │
│  └─ FechaRango (string calculado)                       │
└──────────────────────────┬──────────────────────────────┘
                           │ Binding a XAML
                           ▼
┌─────────────────────────────────────────────────────────┐
│              ProyectosPage.xaml (UI)                    │
│         ➤ CollectionView.ItemsSource                    │
│         ➤ Converters (colores, estados, etc)           │
└─────────────────────────────────────────────────────────┘
```

---

## 🗂️ Archivos Creados

### 1. **tmr-shared/DTOs/Proyectos/ProyectoResponse.cs**

```csharp
namespace tmr_shared.DTOs.Proyectos;

public record ProyectoResponse
{
    public int Id { get; init; }                    // Identificador único
    public string Codigo { get; init; }             // PROJ-001
    public string Nombre { get; init; }             // "Migración Cloud"
    public string Cliente { get; init; }            // Nombre del cliente
    public string Estado { get; init; }             // "Activo", "Inactivo", etc.
    public decimal? Presupuesto { get; init; }      // Monto en USD
    public decimal? Horas { get; init; }            // Horas totales
    public string Lider { get; init; }              // Nombre completo
    public int NumeroRecursos { get; init; }        // Cantidad de colaboradores
    public string Tipo { get; init; }               // "Cloud Migration", etc.
    public DateOnly? FechaInicio { get; init; }     // YYYY-MM-DD
    public DateOnly? FechaFin { get; init; }        // YYYY-MM-DD
}
```

**Uso**: Deserialización del endpoint `GET /api/proyectos`

**Ejemplo JSON del API**:
```json
[
  {
    "id": 1,
    "codigo": "PROJ-001",
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
  }
]
```

---

### 2. **tmr-shared/DTOs/Proyectos/ProyectoLookupsResponse.cs**

```csharp
namespace tmr_shared.DTOs.Proyectos;

public record ProyectoLookupsResponse
{
    public List<LookupItem> Clientes { get; init; }       // Para combo de clientes
    public List<LookupItem> Lideres { get; init; }        // Para combo de líderes
    public List<LookupItem> Empleados { get; init; }      // Para recursos
    public List<LookupItem> Estados { get; init; }        // Estados disponibles
    public List<LookupItem> Tipos { get; init; }          // Tipos de proyectos
}

public record LookupItem
{
    public int Id { get; init; }
    public string Nombre { get; init; }
}
```

**Uso**: Llenar dropdowns y filtros

**Endpoint**: `GET /api/proyectos/lookups`

**Ejemplo JSON**:
```json
{
  "clientes": [
    { "id": 1, "nombre": "Grupo Financiero del..." },
    { "id": 2, "nombre": "CLARO - CONECEL S.A." }
  ],
  "estados": [
    { "id": 1, "nombre": "Activo" },
    { "id": 2, "nombre": "Inactivo" },
    { "id": 3, "nombre": "En Progreso" },
    { "id": 4, "nombre": "Completado" }
  ],
  "tipos": [
    { "id": 1, "nombre": "Cloud Migration" },
    { "id": 2, "nombre": "Desarrollo Web" }
  ]
}
```

---

### 3. **tmr-shared/DTOs/Proyectos/ProyectoCreateUpdateRequest.cs**

```csharp
namespace tmr_shared.DTOs.Proyectos;

public record ProyectoCreateUpdateRequest
{
    public string Codigo { get; init; }              // Código único
    public string Nombre { get; init; }              // Nombre del proyecto
    public int IdCliente { get; init; }              // ID del cliente
    public int IdEstado { get; init; }               // ID del estado
    public decimal? Presupuesto { get; init; }       // Presupuesto
    public decimal? Horas { get; init; }             // Horas estimadas
    public int IdLider { get; init; }                // ID del líder
    public int IdTipo { get; init; }                 // ID del tipo
    public DateOnly? FechaInicio { get; init; }      // Fecha inicio
    public DateOnly? FechaFin { get; init; }         // Fecha fin
    public List<int> IdRecursos { get; init; }       // IDs de colaboradores
}
```

**Uso**: Crear (POST) o actualizar (PUT) proyectos

**Endpoints**:
- `POST /api/proyectos` - Crear nuevo proyecto
- `PUT /api/proyectos/{id}` - Actualizar proyecto

---

### 4. **tmr-mobile/Models/Operaciones/ProyectoItem.cs**

```csharp
namespace tmr_mobile.Models.Operaciones;

public class ProyectoItem
{
    public int Id { get; set; }
    public string Codigo { get; set; }
    public string Nombre { get; set; }
    public string Cliente { get; set; }
    public string Estado { get; set; }
    public decimal Presupuesto { get; set; }
    public decimal Horas { get; set; }
    public string Lider { get; set; }
    public int NumeroRecursos { get; set; }
    public string Tipo { get; set; }
    public string FechaInicio { get; set; }
    public string FechaFin { get; set; }
    
    // Propiedad calculada para display
    public string FechaRango
    {
        get => $"{FechaInicio} - {FechaFin}";
    }
}
```

**Uso**: Modelo para binding en XAML (ViewModel)

---

## 🔄 Flujo de Datos

### Cargar Proyectos

```
1. ProyectosPage.xaml.cs -> OnAppearing()
   └─> vm.CargarProyectosCommand.Execute(null)

2. ProyectosViewModel.CargarProyectosAsync()
   └─> _apiService.GetAsync<List<ProyectoResponse>>("api/proyectos")
       └─> Backend devuelve JSON[]

3. Mapeo ProyectoResponse → ProyectoItem
   ├─ Fecha: DateOnly → string (dd/MM/yyyy)
   ├─ Presupuesto: decimal? → decimal (default 0)
   └─ Horas: decimal? → decimal (default 0)

4. Agregar a ObservableCollection<ProyectoItem>
   └─> Proyectos.Add(item)

5. XAML CollectionView.ItemsSource = ProyectosFiltrados
   ├─ Bind: {Binding Nombre}
   ├─ Bind: {Binding Estado, Converter={StaticResource EstadoColorConverter}}
   ├─ Bind: {Binding Lider, Converter={StaticResource LiderInicialConverter}}
   └─ Bind: {Binding FechaRango}
```

### Crear/Editar Proyecto

```
1. Usuario hace clic en "Nuevo" o "Ver más →"
   └─> vm.CrearProyecto() o vm.AbrirEditar(item)

2. ViewModel abre form
   └─> IsFormVisible = true

3. Usuario completa y clic en "Guardar"
   └─> vm.GuardarProyectoAsync()

4. Crear ProyectoCreateUpdateRequest
   ├─ Si Id > 0: PUT /api/proyectos/{id}
   └─ Si Id = 0: POST /api/proyectos

5. Recargar lista
   └─> CargarProyectosAsync()
```

---

## 🎯 Endpoints Requeridos en Backend

El backend debe proporcionar estos endpoints:

### GET /api/proyectos
```
Response:
200 OK - Array<ProyectoResponse>
400 Bad Request
401 Unauthorized
```

### GET /api/proyectos/lookups
```
Response:
200 OK - ProyectoLookupsResponse
400 Bad Request
401 Unauthorized
```

### POST /api/proyectos
```
Request:
{
  "codigo": "PROJ-001",
  "nombre": "...",
  "idCliente": 1,
  ...
}

Response:
201 Created - ProyectoResponse
400 Bad Request - Validaciones
401 Unauthorized
```

### PUT /api/proyectos/{id}
```
Request: ProyectoCreateUpdateRequest
Response:
200 OK - ProyectoResponse
404 Not Found
400 Bad Request
401 Unauthorized
```

### DELETE /api/proyectos/{id}
```
Response:
200 OK
404 Not Found
401 Unauthorized
```

---

## 📱 Binding en XAML

### Ejemplo de Binding a Propiedades

```xml
<!-- Nombre del proyecto -->
<Label Text="{Binding Nombre}" FontAttributes="Bold" FontSize="18" />

<!-- Estado con color dinámico -->
<Label 
  Text="{Binding Estado}"
  BackgroundColor="{Binding Estado, Converter={StaticResource EstadoColorConverter}}"
  TextColor="{Binding Estado, Converter={StaticResource EstadoTextColorConverter}}" />

<!-- Líder con avatar inicial -->
<Label 
  Text="{Binding Lider, Converter={StaticResource LiderInicialConverter}}"
  FontAttributes="Bold" />

<!-- Cantidad de recursos -->
<Label Text="{Binding NumeroRecursos, StringFormat='{0} recurso(s)'}" />

<!-- Rango de fechas -->
<Label Text="{Binding FechaRango, StringFormat='📅 {0}'}" />
```

---

## ✅ Checklist de Instalación

- [x] Crear carpeta `DTOs/Proyectos` en tmr-shared
- [x] Crear `ProyectoResponse.cs` en tmr-shared/DTOs/Proyectos/
- [x] Crear `ProyectoLookupsResponse.cs` en tmr-shared/DTOs/Proyectos/
- [x] Crear `ProyectoCreateUpdateRequest.cs` en tmr-shared/DTOs/Proyectos/
- [x] Crear carpeta `Models/Operaciones` en tmr-mobile
- [x] Crear `ProyectoItem.cs` en tmr-mobile/Models/Operaciones/
- [x] Verificar ProyectosViewModel importa los DTOs correctos
- [x] Verificar converters registrados en App.xaml
- [x] Compilar proyecto sin errores

---

## 🐛 Troubleshooting

### Problem: "No se cargan proyectos"

**Causas posibles**:
1. API no está devolviendo datos
2. Endpoint es incorrecto
3. Falta configuración de autorización

**Solución**:
```csharp
// Revisar en ProyectosViewModel
var lista = await _apiService.GetAsync<List<ProyectoResponse>>("api/proyectos");
// Verificar que devuelve datos
```

### Problem: "Tarjetas no se visualizan"

**Causas posibles**:
1. ProyectosFiltrados está vacío
2. CollectionView ItemTemplate tiene error
3. Falta bind de datos

**Solución**:
```xml
<!-- Verificar en ProyectosPage.xaml -->
<CollectionView ItemsSource="{Binding ProyectosFiltrados}">
    <CollectionView.ItemTemplate>
        <!-- DataTemplate debe estar correcto -->
    </CollectionView.ItemTemplate>
</CollectionView>
```

### Problem: "Estados no se colorean"

**Causa**: Converters no están registrados en App.xaml

**Solución**:
```xml
<!-- En App.xaml ResourceDictionary -->
<converters:EstadoColorConverter x:Key="EstadoColorConverter" />
<converters:EstadoTextColorConverter x:Key="EstadoTextColorConverter" />
```

---

## 📊 Ejemplo Completo de JSON

### Respuesta de GET /api/proyectos

```json
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

---

## 🎓 Próximos Pasos

1. **Implementar endpoint** en tmr-backend si no existe
2. **Probar la carga** de proyectos con datos reales
3. **Personalizar** colores en converters si es necesario
4. **Agregar filtros avanzados** en lookups (departamentos, etc.)
5. **Agregar paginación** si hay muchos proyectos

---

**Versión**: 1.0  
**Fecha**: 2026-08-11  
**Estado**: ✅ Completo
