# AuthSystem - Sistema de Autenticación

[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/TheTortillas/AuthSystem)

<!--deepwiki-title: AuthSystem-->
<!--deepwiki-description: Sistema de autenticación robusto con Angular 18, .NET 8, MySQL, JWT y hashing de contraseñas seguro (PBKDF2 + SHA512 + salt embebida). Incluye verificación de email, recuperación de contraseñas, protección contra fuerza bruta y arquitectura moderna. -->
<!--deepwiki-stack: Angular, .NET 8, C#, MySQL, JWT, PBKDF2, SHA512, TailwindCSS-->
<!--deepwiki-keywords: autenticación, hashing de contraseñas, salt embebida, PBKDF2, SHA512, JWT, verificación de email, fuerza bruta, seguridad, Angular, .NET, MySQL-->
<!--deepwiki-main: backend/Services/Auth/PasswordHasher.cs-->
<!--deepwiki-main: backend/Controllers/UserManagement/UserManagementController.cs-->
<!--deepwiki-main: backend/Services/Auth/AuthService.cs-->
<!--deepwiki-main: queryDB.sql-->
<!--deepwiki-main: frontend/src/app/core/-->
<!--deepwiki-main: frontend/src/app/pages/-->

Un sistema completo de autenticación desarrollado con Angular 18 (frontend) y .NET 8 (backend), con características de seguridad robustas como verificación de email, recuperación de contraseñas, y protección contra ataques.

## 📋 Tabla de Contenido

- Descripción
- Tecnologías Utilizadas
- Requisitos Previos
- Instalación y Configuración
- Estructura del Proyecto
- Ejecución del Proyecto
- Características
- Seguridad
- Implementación de Hashing de Contraseñas

## 📝 Descripción

AuthSystem es un proyecto integral que proporciona una solución completa de autenticación y gestión de usuarios con funciones avanzadas de seguridad. El sistema implementa prácticas recomendadas de seguridad como hashing de contraseñas con sal embebida, protección contra ataques de fuerza bruta, verificación de email, y autenticación basada en tokens JWT.

## 🛠️ Tecnologías Utilizadas

### Frontend

- Angular 18
- TypeScript
- TailwindCSS
- SweetAlert2
- JWT Decode

### Backend

- .NET 8 API
- C#
- JWT Authentication
- MySQL
- PBKDF2 con SHA512 para hashing de contraseñas

## 📋 Requisitos Previos

Antes de comenzar, asegúrate de tener instalado:

1. **Node.js** (v18 o superior) y **npm** (v10 o superior)
2. **.NET 8 SDK**
3. **MySQL** (v8 o superior)
4. **Git**
5. **Visual Studio**, **Visual Studio Code** o un IDE compatible

## 🚀 Instalación y Configuración

### Paso 1: Clonar el Repositorio

```bash
git clone https://github.com/yourusername/AuthSystem.git
cd AuthSystem
```

### Paso 2: Configurar el Backend (.NET)

1. **Restaurar paquetes NuGet**:

```bash
cd backend
dotnet restore
```

2. **Configuración de la Base de Datos**:
   - Crea una base de datos MySQL
   - Actualiza la cadena de conexión en `appsettings.json`:

```json
"ConnectionStrings": {
  "default": "Server=localhost;Database=authsystem;Uid=tu_usuario;Pwd=tu_contraseña;"
}
```

3. **Configuración de JWT**:
   - Actualiza la sección JWTSettings en `appsettings.json`:

```json
"JWTSettings": {
  "securityKey": "tu_clave_secreta_larga_y_segura",
  "validIssuer": "AuthSystem",
  "validAudience": "AuthSystemUsers",
  "expiryInMinutes": "60"
}
```

4. **Configuración de Email**:
   - Actualiza la sección EmailSettings en `appsettings.json`:

```json
"EmailSettings": {
  "SenderEmail": "tu_email@ejemplo.com",
  "SmtpPassword": "tu_contraseña",
  "SmtpServer": "smtp.ejemplo.com",
  "SmtpPort": 587,
  "DisplayName": "AuthSystem"
}
```

5. **Ejecuta las migraciones de la base de datos**:
   - Los scripts SQL se encuentran en la carpeta `/database` (si están disponibles)

### Paso 3: Configurar el Frontend (Angular)

1. **Instalar dependencias**:

```bash
cd ../frontend
npm install
```

2. **Configuración del entorno**:
   - Verifica y ajusta la URL de la API en los archivos de entorno:
   - `src/app/environments/environment.ts`
   - `src/app/environments/environment.development.ts`

## 📁 Estructura del Proyecto

```
AuthSystem/
├── backend/               # Proyecto .NET
│   ├── Controllers/       # Controladores API
│   ├── DTOs/              # Data Transfer Objects
│   ├── Repositories/      # Acceso a datos
│   ├── Services/          # Servicios de negocio
│   │   ├── Auth/          # Servicios de autenticación
│   │   │   ├── PasswordHasher.cs  # Implementación de hashing de contraseñas
│   ├── appsettings.json   # Configuración
│
├── frontend/              # Proyecto Angular
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/      # Servicios compartidos
│   │   │   ├── pages/     # Páginas de la aplicación
│   │   │   ├── environments/ # Configuración de entorno
```

## ▶️ Ejecución del Proyecto

### Backend (.NET)

```bash
cd backend
dotnet run
```

El servidor API se iniciará en `https://localhost:7135`.

### Frontend (Angular)

```bash
cd frontend
ng serve
```

La aplicación Angular se ejecutará en `http://localhost:4200`.

## ✨ Características

- **Registro de usuarios** con verificación de email
- **Inicio de sesión** por email o nombre de usuario
- **Protección contra ataques de fuerza bruta**
- **Recuperación de contraseñas** con tokens seguros
- **Autenticación basada en JWT** con renovación de tokens
- **Validación de formularios** en tiempo real
- **Diseño responsive** con TailwindCSS
- **Internacionalización** (Español por defecto)

## 🔐 Seguridad

- **Hashing de contraseñas** avanzado con PBKDF2 + SHA512
- **Sal embebida** en el hash para mayor seguridad y simplicidad
- **Protección contra ataques de timing** usando comparaciones de tiempo constante
- Verificación de email obligatoria
- Protección contra múltiples intentos fallidos de inicio de sesión (bloqueo de cuenta)
- Tokens JWT con tiempo de expiración
- HTTPS para comunicaciones seguras

## 🔒 Implementación de Hashing de Contraseñas

El sistema utiliza un método seguro y moderno para el almacenamiento y verificación de contraseñas:

### Clase PasswordHasher

```csharp
public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;       // Tamaño de la sal en bytes
    private const int HashSize = 32;       // Tamaño del hash en bytes
    private const int Iterations = 100000; // Número de iteraciones para PBKDF2

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public string Hash(string password)
    {
        // Generación de sal segura usando RandomNumberGenerator
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Derivación de clave usando PBKDF2 con SHA512
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        // Combinación de hash y sal en un solo string (hash-salt)
        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        // Separación del hash y la sal
        string[] parts = passwordHash.Split('-');
        byte[] hash = Convert.FromHexString(parts[0]);
        byte[] salt = Convert.FromHexString(parts[1]);

        // Cálculo del hash con la contraseña proporcionada
        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

        // Comparación en tiempo constante para prevenir ataques de timing
        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }
}
```

### Características de Seguridad

- **PBKDF2 con SHA512**: Algoritmo criptográficamente seguro para derivación de claves
- **100,000 iteraciones**: Mayor resistencia contra ataques de fuerza bruta
- **Sal aleatoria de 16 bytes**: Generada usando RandomNumberGenerator para máxima seguridad
- **Hash de 32 bytes**: Longitud suficiente para resistir colisiones
- **Formato `{hash}-{sal}`**: Almacenamiento eficiente de ambos valores en un solo campo
- **Comparación de tiempo constante**: Previene ataques de timing utilizando `CryptographicOperations.FixedTimeEquals`
- **Sin necesidad de campo adicional**: La sal se almacena junto con el hash, simplificando la estructura de la base de datos

### Base de Datos

La implementación simplifica el esquema de la base de datos al eliminar la necesidad de una columna separada para la sal:

```sql
CREATE TABLE users (
    /* Otros campos */
    password_hash VARCHAR(255) NOT NULL,
    /* No se necesita una columna password_salt */
    /* Otros campos */
);
```

Este enfoque de hashing es superior a las implementaciones tradicionales ya que:

1. Simplifica el modelo de datos
2. Reduce la posibilidad de errores en la gestión de la sal
3. Facilita la migración y el respaldo de datos
4. Proporciona seguridad de nivel empresarial contra diversos ataques

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo LICENSE para detalles.

## 📧 Contacto

Para preguntas o sugerencias, por favor contacta a [sebasverac331@gmail.com](mailto:sebasverac331@gmail.com).

1. **Node.js** (v18 o superior) y **npm** (v10 o superior)
2. **.NET 8 SDK**
3. **MySQL** (v8 o superior)
4. **Git**
5. **Visual Studio**, **Visual Studio Code** o un IDE compatible

## 🚀 Instalación y Configuración

### Paso 1: Clonar el Repositorio

```bash
git clone https://github.com/yourusername/AuthSystem.git
cd AuthSystem
```

### Paso 2: Configurar el Backend (.NET)

1. **Restaurar paquetes NuGet**:

```bash
cd backend
dotnet restore
```

2. **Configuración de la Base de Datos**:
   - Crea una base de datos MySQL
   - Actualiza la cadena de conexión en `appsettings.json`:

```json
"ConnectionStrings": {
  "default": "Server=localhost;Database=authsystem;Uid=tu_usuario;Pwd=tu_contraseña;"
}
```

3. **Configuración de JWT**:
   - Actualiza la sección JWTSettings en `appsettings.json`:

```json
"JWTSettings": {
  "securityKey": "tu_clave_secreta_larga_y_segura",
  "validIssuer": "AuthSystem",
  "validAudience": "AuthSystemUsers",
  "expiryInMinutes": "60"
}
```

4. **Configuración de Email**:
   - Actualiza la sección EmailSettings en `appsettings.json`:

```json
"EmailSettings": {
  "SenderEmail": "tu_email@ejemplo.com",
  "SmtpPassword": "tu_contraseña",
  "SmtpServer": "smtp.ejemplo.com",
  "SmtpPort": 587,
  "DisplayName": "AuthSystem"
}
```

5. **Ejecuta las migraciones de la base de datos**:
   - Los scripts SQL se encuentran en la carpeta `/database` (si están disponibles)

### Paso 3: Configurar el Frontend (Angular)

1. **Instalar dependencias**:

```bash
cd ../frontend
npm install
```

2. **Configuración del entorno**:
   - Verifica y ajusta la URL de la API en los archivos de entorno:
   - `src/app/environments/environment.ts`
   - `src/app/environments/environment.development.ts`

## 📁 Estructura del Proyecto

```
AuthSystem/
├── backend/               # Proyecto .NET
│   ├── Controllers/       # Controladores API
│   ├── DTOs/              # Data Transfer Objects
│   ├── Repositories/      # Acceso a datos
│   ├── Services/          # Servicios de negocio
│   ├── appsettings.json   # Configuración
│
├── frontend/              # Proyecto Angular
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/      # Servicios compartidos
│   │   │   ├── pages/     # Páginas de la aplicación
│   │   │   ├── environments/ # Configuración de entorno
```

## ▶️ Ejecución del Proyecto

### Backend (.NET)

```bash
cd backend
dotnet run
```

El servidor API se iniciará en `https://localhost:7135`.

### Frontend (Angular)

```bash
cd frontend
ng serve
```

La aplicación Angular se ejecutará en `http://localhost:4200`.

## ✨ Características

- **Registro de usuarios** con verificación de email
- **Inicio de sesión** por email o nombre de usuario
- **Protección contra ataques de fuerza bruta**
- **Recuperación de contraseñas** con tokens seguros
- **Autenticación basada en JWT** con renovación de tokens
- **Validación de formularios** en tiempo real
- **Diseño responsive** con TailwindCSS
- **Internacionalización** (Español por defecto)

## 🔐 Seguridad

- Contraseñas almacenadas con hash + salt usando Identity
- Verificación de email obligatoria
- Protección contra múltiples intentos fallidos de inicio de sesión
- Tokens JWT con tiempo de expiración
- HTTPS para comunicaciones seguras

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo LICENSE para detalles.

## 📧 Contacto

Para preguntas o sugerencias, por favor contacta a [sebasverac331@gmail.com](mailto:tu@email.com).
