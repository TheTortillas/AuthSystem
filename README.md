# AuthSystem - Sistema de Autenticación

Un sistema completo de autenticación desarrollado con Angular 18 (frontend) y .NET 8 (backend), con características de seguridad robustas como verificación de email, recuperación de contraseñas, y protección contra ataques.

## 📋 Tabla de Contenido

- [Descripción](#descripción)
- [Tecnologías Utilizadas](#tecnologías-utilizadas)
- [Requisitos Previos](#requisitos-previos)
- [Instalación y Configuración](#instalación-y-configuración)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Ejecución del Proyecto](#ejecución-del-proyecto)
- [Características](#características)

## 📝 Descripción

AuthSystem es un proyecto integral que proporciona una solución completa de autenticación y gestión de usuarios con funciones avanzadas de seguridad. El sistema implementa prácticas recomendadas de seguridad como hashing de contraseñas con sal, protección contra ataques de fuerza bruta, verificación de email, y autenticación basada en tokens JWT.

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
- Identity para hashing de contraseñas

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
