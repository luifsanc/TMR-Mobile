# Auth Migration and Login Debug Summary

## Resumen general

Realicé una verificación y ajuste completo del flujo de autenticación para que el móvil use el backend desplegado correctamente con las credenciales reales `admin@tmr.com / adminadmin`.

## Qué se hizo

1. Verifiqué la conexión del backend con la base de datos.
   - Confirmé la cadena de conexión en `tmr-backend/tmr-backend/appsettings.Development.json`.
   - Confirmé que el backend puede leer el registro de usuario `admin@tmr.com` desde `autenticacion.tbl_autenticacion_usuario`.

2. Validé la autenticación en el backend.
   - Revisé `tmr-backend/tmr-backend/Features/Auth/Services/AuthService.cs`.
   - El login valida email, verifica el hash con `BCrypt.Net.BCrypt.Verify`, crea la sesión y emite tokens.
   - Confirmé que el fallo anterior no era la conexión a la DB, sino el camino que seguía el cliente móvil.

3. Probé directamente el endpoint REST de login.
   - Endpoint probado: `https://dev.api.tmr2.dokploy.integritysolutions.com.ec/api/auth/login`.
   - Resultado: `success: true`, token JWT válido y usuario correcto.
   - Esto confirmó que las credenciales y el backend están bien.

## Cambios aplicados al móvil

### `tmr-mobile/Services/ApiService.cs`
- Actualicé `BaseUrl` para apuntar al backend desplegado:
  - antes: `https://localhost:7198/`
  - ahora: `https://dev.api.tmr2.dokploy.integritysolutions.com.ec/api/`
- Esto asegura que todas las llamadas REST vayan al servidor correcto.

### `tmr-mobile/Services/AuthService.cs`
- Cambié el flujo de login de gRPC a REST.
- Agregué un nuevo modelo local `ApiLoginResponse` para deserializar la respuesta típica de `ApiResponse<T>` del backend.
- Ahora el servicio hace:
  - `POST auth/login` con `{ user, password }`
  - almacena `Data.AccessToken` en `SecureStorage`
  - crea `CurrentUser` usando los datos retornados en `Data.User`
- Eliminé el uso directo de `GrpcAuthService` en el flujo de login.

### `tmr-mobile/tmr-mobile.csproj`
- Añadí soporte gRPC en el proyecto:
  - `PackageReference Include="Grpc.Net.Client" Version="2.66.0"`
  - `Protobuf Include="..\tmr-shared\Protos\auth.proto" GrpcServices="Client"`
- Nota: el login final actualmente usa REST, pero el proyecto aún contiene las referencias gRPC que se añadieron durante el proceso de diagnóstico.

### `tmr-mobile/Services/GrpcAuthService.cs`
- El archivo existe en el árbol de trabajo, pero **no es el camino activo de login** después de los cambios.
- Queda disponible como referencia o posible fallback para implementar gRPC de nuevo en el futuro.

## Verificación final

- Compilé exitosamente `tmr-mobile/tmr-mobile.csproj` después de corregir un error de tipo en la construcción de `UserResponse`.
- Cerré procesos bloqueados para que la compilación pudiera escribirse correctamente.
- Confirmé que el login REST funciona con las credenciales reales.

## Archivos clave afectados

- `tmr-mobile/Services/ApiService.cs`
- `tmr-mobile/Services/AuthService.cs`
- `tmr-mobile/tmr-mobile.csproj`
- `tmr-mobile/Services/GrpcAuthService.cs` (presente como artefacto gRPC no usado)

## Nota importante

Aunque hubo cambios en el proyecto móvil para habilitar gRPC, el flujo seguro y comprobado que quedó operativo es el login REST. Si quieres, puedo también limpiar el proyecto para eliminar las referencias gRPC no usadas.
