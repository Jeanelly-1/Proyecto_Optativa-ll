# Implementación de Autenticación y Autorización en .NET Core (AuthNetCore)

Este proyecto demuestra una implementación básica de autenticación y autorización en una API de .NET Core, utilizando un enfoque de datos simulados (mock data) para la gestión de usuarios, sesiones y permisos. El objetivo principal es ilustrar cómo se pueden integrar estos conceptos de seguridad en una aplicación web.

## Componentes Principales

### 1. `AuthNetCore.cs` (ETLService/Security/AuthNetCore.cs)

Este archivo centraliza la lógica de autenticación y autorización. Contiene la "base de datos" de usuarios simulados (`_users`) y un registro de sesiones activas (`_activeSessions`).

-   **`_users`**: Una lista estática de `UserModel` que representa a los usuarios registrados con sus credenciales (`username`, `password`) y una lista de `Permissions` asociadas.
    -   Ejemplo de usuario: `testuser` con permisos para `CanViewUsers` y `CanViewReports`.
    -   Ejemplo de administrador: `admin` con todos los permisos (`CanViewUsers`, `CanEditUsers`, `CanDeleteUsers`, `CanViewReports`, `CanGenerateReports`).
-   **`_activeSessions`**: Un diccionario que mapea un `sessionKey` (token) a un `UserModel`, indicando las sesiones de usuario actualmente activas.
-   **`Authenticate(string? sessionKey)`**: Verifica si un `sessionKey` proporcionado corresponde a una sesión activa.
-   **`ClearSeason(string? sessionKey)`**: Elimina una sesión activa, cerrando la sesión del usuario.
-   **`Login(UserModel inst, string? sessionKey)`**: Intenta autenticar a un usuario con las credenciales dadas. Si es exitoso, genera un nuevo `sessionKey` y registra la sesión. Devuelve un objeto anónimo con el `SessionKey` y el `UserModel` si el inicio de sesión es exitoso, o `null` si las credenciales son inválidas.
-   **`RecoveryPassword(string username)`**: Simula el proceso de recuperación de contraseña. En un entorno real, esto implicaría enviar un correo electrónico o restablecer la contraseña de forma segura.
-   **`HavePermission(string? token, Permissions[] permissionsList)`**: Verifica si el usuario asociado con el `token` de sesión tiene al menos uno de los permisos requeridos (`permissionsList`).
-   **`User(string? token)`**: Devuelve el `UserModel` asociado a un `sessionKey` dado, o `null` si no se encuentra la sesión.

### 2. `AuthAttributes.cs` (ETLService/Security/AuthAttibutes.cs)

Este archivo define un `ActionFilterAttribute` personalizado (`AuthControllerAttribute`) que se utiliza para aplicar la lógica de autenticación y autorización a los endpoints de los controladores.

-   **`AuthControllerAttribute`**: Un atributo que se aplica a métodos de controlador o controladores completos.
    -   Cuando se aplica con `Permissions.CanViewUsers`, por ejemplo, el atributo asegura que solo los usuarios con el permiso `CanViewUsers` puedan acceder a ese endpoint.
    -   Utiliza `AuthNetCore.Authenticate` para verificar la autenticación de la sesión.
    -   Utiliza `AuthNetCore.HavePermission` para verificar si el usuario autenticado tiene los permisos necesarios para acceder al recurso.
    -   Si la autenticación o autorización falla, devuelve respuestas HTTP apropiadas (401 Unauthorized, 403 Forbidden).

### 3. `SecurityController.cs` (ETLService/Controllers/SecurityController.cs)

Este controlador maneja las operaciones relacionadas con la seguridad del usuario, como el inicio y cierre de sesión, y la recuperación de contraseñas.

-   **`Login(UserModel Inst)`**: Endpoint para iniciar sesión. Recibe un `UserModel` con `username` y `password`. Utiliza `HttpContext.Session.SetString` para establecer un `sessionKey` y llama a `AuthNetCore.Login` para autenticar al usuario.
-   **`LogOut()`**: Endpoint para cerrar la sesión. Llama a `AuthNetCore.ClearSeason` para invalidar el `sessionKey` actual.
-   **`RecoveryPassword(UserModel Inst)`**: Endpoint para iniciar el proceso de recuperación de contraseña. Llama a `AuthNetCore.RecoveryPassword`.

### 4. `UserController.cs` (ETLService/Controllers/UserController.cs)

Este controlador de ejemplo demuestra cómo proteger endpoints utilizando el `AuthControllerAttribute` para la autorización basada en permisos.

-   **`GetUsers()` y `GetUserById(string name)`**: Requieren el permiso `Permissions.CanViewUsers` para acceder a la lista o detalles de usuarios.
-   **`CreateUser(User user)` y `UpdateUser(User user)`**: Requieren el permiso `Permissions.CanEditUsers` para crear o modificar usuarios.
-   **`DeleteUser(User userParam)`**: Requiere el permiso `Permissions.CanDeleteUsers` para eliminar usuarios.

## Uso de Sesión, Autenticación y Autorización

-   **Sesión**: La sesión se gestiona a través de `HttpContext.Session.GetString/SetString` en el `SecurityController`, utilizando un `sessionKey` (un GUID) para identificar la sesión de un usuario. Este `sessionKey` es el token que se pasa a los métodos de `AuthNetCore`.
-   **Autenticación**: Ocurre en el método `Login` del `SecurityController`, que a su vez invoca `AuthNetCore.Login`. Una vez autenticado, se crea una sesión activa en `AuthNetCore` con el `sessionKey` generado.
-   **Autorización**: Se implementa a través del `AuthControllerAttribute`. Antes de ejecutar la acción de un controlador, el atributo intercepta la solicitud y verifica dos cosas:
    1.  **Autenticación**: Llama a `AuthNetCore.Authenticate` para asegurarse de que el `sessionKey` es válido.
    2.  **Permisos**: Si la acción tiene permisos específicos (`PermissionsList`), llama a `AuthNetCore.HavePermission` para verificar si el usuario en la sesión actual posee al menos uno de esos permisos. Si la verificación falla, se deniega el acceso con un código de estado HTTP 401 o 403.

Este enfoque permite una gestión flexible de la seguridad, donde la lógica de autenticación y autorización se centraliza en `AuthNetCore`, se aplica declarativamente a través de atributos en los controladores y se gestiona a nivel de sesión HTTP.

## Importancia de la Configuración en la Arquitectura de Autenticación y Autorización

La correcta configuración de los componentes de seguridad es crucial para la robustez de la aplicación. En esta implementación, aunque se utilizan datos simulados, los principios de configuración son los mismos que en un entorno real:

-   **Centralización en `AuthNetCore`**: Al encapsular la lógica de autenticación y autorización en una única clase (`AuthNetCore`), se facilita la configuración global de las reglas de seguridad. Por ejemplo, en un escenario real, `AuthNetCore` podría configurarse para interactuar con una base de datos de usuarios real, un proveedor de identidad externo (OAuth2, OpenID Connect) o un sistema de gestión de roles y permisos, sin afectar la lógica de los controladores.

-   **Atributos Declarativos (`AuthControllerAttribute`)**: El uso de atributos permite una configuración declarativa de la seguridad a nivel de endpoint. Esto significa que los desarrolladores pueden especificar fácilmente qué permisos se requieren para cada acción sin escribir código de seguridad repetitivo en cada método. Esta separación de preocupaciones mejora la legibilidad, mantenibilidad y escalabilidad del código de seguridad. La configuración de estos atributos (`Permissions.CanViewUsers`, `Permissions.CanEditUsers`, etc.) define la política de acceso de manera clara y concisa.

-   **Gestión de Sesiones**: Aunque en este ejemplo la gestión de sesiones es simple (usando `HttpContext.Session`), en una aplicación de producción, la configuración de la sesión implicaría aspectos como:
    -   **Duración de la sesión**: Configurar el tiempo de vida de la sesión para equilibrar la seguridad y la usabilidad.
    -   **Almacenamiento de la sesión**: Decidir dónde almacenar los datos de la sesión (en memoria, en una base de datos distribuida como Redis, etc.) para garantizar la escalabilidad y resiliencia.
    -   **Seguridad de la sesión**: Implementar medidas como la rotación de claves de sesión, el uso de cookies seguras (HttpOnly, Secure) y la prevención de ataques de fijación de sesión.

-   **Configuración de `Program.cs`**: El archivo `Program.cs` es el punto de entrada donde se configurarían los servicios de autenticación y autorización de ASP.NET Core, como la adición de `AddSession`, `AddAuthentication` y `AddAuthorization`. Aquí es donde se definiría cómo se autentican los usuarios (por ejemplo, mediante JWT, cookies) y cómo se autorizan las solicitudes (a través de políticas o roles).

En resumen, la arquitectura propuesta, aunque simplificada con mock data, establece una base sólida para la configuración y gestión de la seguridad, permitiendo una adaptación sencilla a requisitos más complejos de autenticación y autorización en futuras iteraciones del proyecto.
