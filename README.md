# 📦 Proyecto de Migración Movistar

Este repositorio contiene el **código fuente completo** del sistema de migración de usuarios desde el operador **Movistar** hacia **Tigo** o **Claro**, desarrollado como parte de un sistema de soporte a procesos masivos de portabilidad.

---

## 🚀 Tecnologías utilizadas

| Componente       | Tecnología                     |
|------------------|--------------------------------|
| 🧠 Backend       | ASP.NET Core Web API (C#), Entity Framework Core |
| 🌐 Frontend      | Angular, TypeScript, HTML5, CSS3 |
| 🗃️ Base de Datos | SQL Server                     |
| 🛠️ IDEs          | Visual Studio 2022, Visual Studio Code |
| 📦 API Docs      | Swagger (OpenAPI)              |
| 🗂️ Gestión       | Jira                           |
| 🔧 Control de versiones | Git + GitHub                |

---

## 📁 Estructura del repositorio

- /backend/ -> Código fuente de la API ASP.NET Core
- /frontend/ -> Aplicación Angular
- /scripts/ -> Script SQL para crear la base de datos
- README.md -> Descripción del proyecto

---

## 🧠 Backend: ASP.NET Core API

- Arquitectura limpia basada en capas (Controlador, Business Logic, Entidades, Utils).
- Uso de Entity Framework Core con SQL Server.
- Documentación automática de la API con Swagger.
- Manejo robusto de excepciones y validaciones personalizadas.

🔧 **Importante**:  
Configura la cadena de conexión a la base de datos en el archivo appsettings.json ubicado en el proyecto backend, sección "ConnectionStrings":

Con usuario

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR;Database=Proyect_migracion;User Id=(nombreUsuario);Password=(contrasena);TrustServerCertificate=true;"
}
```

Sin usuario

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=TU_SERVIDOR;Database=Proyect_migracion;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

---

## 🌐 Frontend: Angular

- Interfaz de usuario moderna con Angular y TypeScript.
- Consumo de la API REST.
- Comunicación eficiente entre componentes y servicios.

---

## ⚙️ Instalación y ejecución

### 🧠 Backend

- Abrir la carpeta /backend/ con Visual Studio 2022.
- Configurar la conexión en appsettings.json.
- Ejecutar el proyecto. Swagger estará disponible en https://localhost:{puerto}/swagger.

### 🌐 Frontend

- Abrir la carpeta /frontend/ con Visual Studio Code.
- Ejecutar los siguientes comandos:
  - npm install
  - ng serve
  - Acceder a la app desde http://localhost:{puerto}.

---

## 📄 Scripts de base de datos

El script SQL para crear la base de datos y sus tablas se encuentra en la carpeta /scripts/.

---

## 🧑‍💻 Autores

- Juan Diego Blandón Toro
- Jeisson Arley Osorio Llanos
- Juan Estevan Zapata Correa

📧 juand.blandont@gmail.com

---

## 📄 Licencia

Este proyecto es de uso académico y privado. No está autorizado para uso comercial sin previa autorización por parte del autor.
