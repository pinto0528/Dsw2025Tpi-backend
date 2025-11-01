# Trabajo Práctico Integrador
## Desarrollo de Software

## Integrantes del Grupo

* **56605** - Jeremias Mastafa Nazar - jeremias.mastafanazar@alu.frt.utn.edu.ar
* **53390** - Nicolas Pinto - nclpnt0528@gmail.com
* **53160** - Maria de la Paz Castro - castromariadelapaz01@gmail.com

### Backend

## Introducción
Se desea desarrollar una plataforma de comercio electrónico (E-commerce). 
En esta primera etapa el objetivo es construir el módulo de Órdenes, permitiendo la gestión completa de éstas.

## Visión General del Producto
Del relevamiento preliminar se identificaron los siguientes requisitos:
- Los visitantes pueden consultar los productos sin necesidad de estar registrados o iniciar sesión.
- Para realizar un pedido se requiere el inicio de sesión.
- Una orden, para ser aceptada, debe incluir la información básica del cliente, envío y facturación.
- Antes de registrar la orden se debe verificar la disponibilidad de stock (o existencias) de los productos.
- Si la orden es exitosa hay que actualizar el stock de cada producto.
- Se deben poder consultar órdenes individuales o listar varias con posibilidad de filtrado.
- Será necesario el cambio de estado de una orden a medida que avanza en su ciclo de vida.
- Los administradores solo pueden gestionar los productos (alta, modificación y baja) y actualizar el estado de la orden.
- Los clientes pueden crear y consultar órdenes.

## Alcance para el Primer Parcial
> [!IMPORTANT]
> Del apartado `IMPLEMENTACIÓN` (Pag. 7), completo hasta el punto `6` (inclusive)


### Características de la Solución

- Lenguaje: C# 12.0
- Plataforma: .NET 8

# 

# Configuración de la base de datos

### Modificar `appsettings.json`

Dentro del archivo `Dsw2025Tpi.Api/appsettings.json`, actualizar la sección `ConnectionSettings` con los datos de tu entorno local:

```json
  "ConnectionSettings": {
    "Server": "(localdb)\\nombre-del-server",
    "Database": "nombre-de-la-db"
    }
```

### Migraciones y base de datos
Si usás Entity Framework, podés aplicar migraciones con:
```bash 
dotnet ef database update
```
Además, el proyecto soporta carga automática (seeding) desde archivos JSON.

Asegurate de tener el archivo customers.json en la carpeta Sources/.

### Cómo ejecutar el proyecto

Desde la terminal o consola de Visual Studio, ejecutá:
```bash
dotnet run --project Dsw2025Tpi.Api
```

Por defecto, la API estará disponible en:
- https://localhost:5001
- http://localhost:5000

#### Endpoints principales
| Método | Ruta                    | Descripción                    |
|--------|--------------------------|--------------------------------|
| GET    | /api/products            | Listado de productos           |
| POST   | /api/products            | Crear nuevo producto           |
| PUT    | /api/products/{id}       | Actualizar producto existente  |
| PATCH  | /api/products/{id}       | Deshabilitar producto          |
| GET    | /api/products/{id}       | Obtener producto por ID        |
| POST   | /api/orders              | Crear nueva orden              |

