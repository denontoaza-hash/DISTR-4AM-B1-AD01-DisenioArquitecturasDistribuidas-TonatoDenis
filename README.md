**# Arquitectura de Microservicios – Comercio y Distribución Multibodega**



**## Descripción**



Proyecto desarrollado para la asignatura Aplicaciones Distribuidas.



El proyecto implementa una arquitectura de microservicios REST independientes utilizando ASP.NET Core Web API, SQL Server y Entity Framework Core.



El sistema permite administrar la información comercial de productos, controlar las existencias y movimientos de mercancía en diferentes bodegas y registrar pedidos de clientes.



Cada microservicio cuenta con su propia base de datos y responsabilidades independientes. Además, se implementa comunicación entre los servicios para validar productos, consultar precios y realizar reservas automáticas de inventario.



**## Tecnologías utilizadas**



\- C#

\- .NET 10

\- ASP.NET Core Web API

\- Entity Framework Core

\- SQL Server

\- REST API

\- HTTP Client

\- Swagger

\- Postman

\- Visual Studio Community



**## Estructura del repositorio**



Diseño de Arquitecturas Distribuidas/



│



├── Catalogo.Api/



│   └── Catalogo.Api.slnLaunch.user



│   └──  Catalogo.Api.slnx



├── Inventario.Api/



│   └── Inventario.Api.slnLaunch.user



│   └──  Inventario.Api.slnx



├── Pedidos.Api/



│   ├── Pedidos.Api/



│   └── Pedidos.Api.slnLaunch.user



│   └── Pedidos.Api.slnx



├── SQL SERVER/

│   ├── CatalogoDB.sql

│   ├── InventarioDB.sql

│   └── PedidosDB.sql



├── POSTMAN/



│   └── Proyecto Microservicios - Distribución Multibodega.postman\_collection.json



│



└── README.md





**## Instrucciones de uso**



**### 1. Crear las bases de datos**



Abrir SQL Server Management Studio y conectarse a la instancia local de SQL Server.



Crear las siguientes bases de datos correspondientes a cada microservicio:



\- CatalogoDB

\- InventarioDB

\- PedidosDB



Cada microservicio utiliza su propia base de datos para mantener la independencia de la información.



**### 2. Configurar las conexiones a las bases de datos**



Antes de ejecutar los proyectos se deben revisar los archivos appsettings.json de cada microservicio.



Las cadenas de conexión deben modificarse de acuerdo con la configuración local de SQL Server del equipo donde se ejecuta el proyecto.



Ejemplo de configuración:

{
  "ConnectionStrings": {
    "CatalogoConnection": "Data Source=SERVIDOR\\SQLEXPRESS;Initial Catalog=CatalogoDB;Integrated Security=True;Trust Server Certificate=True"
  }
}

Para los otros microservicios se debe utilizar su respectiva base de datos:



\- Catalogo.Api: CatalogoDB

\- Inventario.Api: InventarioDB

\- Pedidos.Api: PedidosDB



Nota: El nombre del servidor puede variar entre computadoras.



**### 3. Abrir los proyectos**



Abrir los proyectos correspondientes utilizando Visual Studio Community.



Los microservicios se encuentran organizados en las siguientes carpetas:



\- Catalogo.Api

\- Inventario.Api

\- Pedidos.Api



Esperar a que Visual Studio restaure las dependencias necesarias de cada proyecto.



**### 4. Ejecutar los microservicios**



Ejecutar los tres proyectos ASP.NET Core Web API desde Visual Studio.



Los puertos configurados son:



| Microservicio | Puerto |

|---|---|

| Catalogo.Api | 5145 |

| Inventario.Api | 5056 |

| Pedidos.Api | 5030 |



Cada servicio dispone de documentación y pruebas de endpoints mediante Swagger en su entorno de desarrollo.



**### 5. Importar la colección de Postman**



Abrir Postman y seleccionar la opción:



Import



Importar el archivo ubicado en:



POSTMAN/Proyecto Microservicios - Distribución Multibodega.postman\_collection.json



La colección contiene las peticiones HTTP necesarias para probar las diferentes operaciones de los tres microservicios.



**### 6. Ejecutar y comprobar el funcionamiento**



Las solicitudes REST se realizan mediante los métodos HTTP correspondientes, utilizando las direcciones configuradas para cada microservicio.



\*\*URL base de los servicios:\*\*



\- Catalogo.Api: http://localhost:5145

\- Inventario.Api: http://localhost:5056

\- Pedidos.Api: http://localhost:5030



\*\*Métodos disponibles en los microservicios:\*\*



Proyecto Microservicios - Distribución Multibodega

&#x20;   Catalogo.Api

&#x20;       GET ObtenerProductos

&#x20;       POST AgregarProducto

&#x20;       PUT Actualizar producto

&#x20;       GET ObtenerProducto

&#x20;   Inventario.Api

&#x20;       GET ObtenerBodegas

&#x20;       GET ObtenerExistencias

&#x20;       GET ObtenerReservas

&#x20;       POST RegistrarEntradaMercancia

&#x20;       GET ObtenerMovimientos

&#x20;       POST RegistrarSalidaMercancia

&#x20;       GET VerificarStockActualizado

&#x20;   Pedidos.Api

&#x20;       GET ObtenerClientes

&#x20;       GET ObtenerPedidos

&#x20;       GET ObtenerDetallesPedido

**## Comunicación entre microservicios**



El sistema implementa comunicación mediante solicitudes HTTP entre los servicios:



&#x20;   Catalogo.Api

&#x20;       GET ObtenerProductos

&#x20;       POST AgregarProducto

&#x20;       PUT Actualizar producto

&#x20;       GET ObtenerProducto

&#x20;   Inventario.Api

&#x20;       GET ObtenerBodegas

&#x20;       GET ObtenerExistencias

&#x20;       GET ObtenerReservas

&#x20;       POST RegistrarEntradaMercancia

&#x20;       GET ObtenerMovimientos

&#x20;       POST RegistrarSalidaMercancia

&#x20;       GET VerificarStockActualizado

&#x20;   Pedidos.Api

&#x20;       GET ObtenerClientes

&#x20;       GET ObtenerPedidos

&#x20;       GET ObtenerDetallesPedido



**## Validaciones y pruebas**



Se realizaron pruebas mediante Postman para comprobar:



\- Consulta, registro y actualización de información.

\- Registro de entradas y salidas de inventario.

\- Actualización automática de existencias.

\- Registro y consulta de reservas.

\- Registro de pedidos y detalles.

\- Comunicación entre los tres microservicios.

\- Validación de productos inexistentes.

\- Control de stock insuficiente.

\- Respuestas HTTP como 200 OK, 201 Created, 400 Bad Request y 404 Not Found.



**## Orden de ejecución**



Para ejecutar correctamente el proyecto se recomienda seguir este orden:



Iniciar SQL Server



↓



Crear las bases de datos CatalogoDB, InventarioDB y PedidosDB



↓



Revisar los archivos appsettings.json



↓



Abrir los proyectos en Visual Studio



↓



Ejecutar Catalogo.Api



↓



Ejecutar Inventario.Api



↓



Ejecutar Pedidos.Api



↓



Verificar los puertos configurados



↓



Abrir Postman



↓



Importar la colección desde POSTMAN/



↓



Probar y comprobar el funcionamiento de las operaciones de los tres microservicios.



**## Autor**



**Estudiante:** Denis Tonato

**Asignatura:** Aplicaciones Distribuidas

**Paralelo:** Cuarto A Matutina

**Tema:** Diseño de Arquitecturas Distribuidas

**Institución:** Instituto Superior Tecnológico Cordillera

