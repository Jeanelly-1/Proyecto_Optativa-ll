# Lógica de Implementación del Paquete Infraestructura y su Relación con el Paquete Entity

Este documento explica la lógica de implementación de los paquetes `Infraestructura` y `Entity`, la relación entre ellos, el uso de atributos personalizados y la abstracción proporcionada por `EntityClass`.

## 1. El Paquete Entity

El paquete `Entity` contiene las definiciones de las entidades de la aplicación. Estas entidades son clases que representan tablas en la base de datos o conceptos de negocio. Cada clase de entidad define las propiedades que corresponden a las columnas de una tabla.

**Ejemplo:**

```csharp
namespace APPCORE.Security
{
    public class Security_Users : EntityClass
	{
		[PrimaryKey(Identity = true)]
		public int? Id_User { get; set; }
		public string? Nombres { get; set; }
		// ... otras propiedades ...

		[OneToMany(TableName = "Security_Users_Roles", KeyColumn = "Id_User", ForeignKeyColumn = "Id_User")]
		public List<Security_Users_Roles>? Security_Users_Roles { get; set; }
	}
}
```

En este ejemplo, `Security_Users` es una entidad que hereda de `EntityClass` y tiene propiedades como `Id_User` y `Nombres`.

## 2. El Paquete Infraestructura

El paquete `Infraestructura` proporciona las herramientas y la lógica base para interactuar con la persistencia de datos y definir cómo las entidades se relacionan entre sí. Su objetivo principal es abstraer las operaciones de base de datos y permitir que las entidades se comporten como objetos de negocio sin preocuparse por los detalles de almacenamiento.

### 2.1. Abstracción de `EntityClass`

`EntityClass` es una clase abstracta que sirve como la base para todas las entidades en el sistema. Define un conjunto de métodos comunes para la manipulación de datos, como `Get`, `Where`, `Find`, `Exists`, `Save` y `Update`.

**Propósito de `EntityClass`:**

*   **Abstracción de Operaciones CRUD:** Proporciona una interfaz unificada para las operaciones básicas de Crear, Leer, Actualizar y Eliminar (CRUD), lo que reduce la duplicación de código en las clases de entidad.
*   **Extensibilidad:** Al ser una clase abstracta, permite que las implementaciones concretas de estos métodos se definan en un nivel inferior (posiblemente a través de un ORM o un patrón de repositorio) sin afectar la lógica de negocio de las entidades.

**Ejemplo de `EntityClass`:**

```csharp
namespace APPCORE.Security
{
    public abstract class EntityClass
    {
        public List<T> Get<T>(string condition = "") { throw new NotImplementedException(); }
        public List<T> Where<T>() { throw new NotImplementedException(); }
        public T? Find<T>() { throw new NotImplementedException(); }
        public Boolean Exists() { throw new NotImplementedException(); }
        public object? Save(bool fullInsert = true) { throw new NotImplementedException(); }
        public ResponseService Update() { throw new NotImplementedException(); }
    }
}
```

### 2.2. Atributos Personalizados (Custom Attributes)

Los atributos personalizados son clases que heredan de `System.Attribute` y se utilizan para agregar metadatos declarativos a las entidades y sus propiedades. Estos atributos son interpretados por la lógica del paquete `Infraestructura` para entender cómo se deben manejar las entidades en relación con la base de datos y otras entidades.

**Ejemplos de Atributos Personalizados:**

*   **`PrimaryKeyAttribute`**: Se utiliza para identificar la propiedad que actúa como clave primaria en una entidad.
    *   **Propiedad `Identity`**: Un booleano que indica si la clave primaria es auto-incremental en la base de datos.
    
    ```csharp
    [PrimaryKey(Identity = true)]
    public int? Id_User { get; set; }
    ```

*   **`OneToManyAttribute`**: Define una relación uno a muchos entre dos entidades. Indica que una instancia de la entidad actual puede estar asociada con múltiples instancias de otra entidad.
    *   **Propiedad `TableName`**: El nombre de la tabla de la entidad relacionada.
    *   **Propiedad `KeyColumn`**: El nombre de la columna de la clave primaria en la entidad actual.
    *   **Propiedad `ForeignKeyColumn`**: El nombre de la columna de la clave foránea en la entidad relacionada que apunta a la entidad actual.

    ```csharp
    [OneToMany(TableName = "Security_Users_Roles", KeyColumn = "Id_User", ForeignKeyColumn = "Id_User")]
    public List<Security_Users_Roles>? Security_Users_Roles { get; set; }
    ```

*   **`ManyToOneAttribute`**: Define una relación muchos a uno entre dos entidades. Indica que múltiples instancias de la entidad actual pueden estar asociadas con una única instancia de otra entidad.
    *   Similar a `OneToMany`, utiliza `TableName`, `KeyColumn` y `ForeignKeyColumn` para describir la relación.
    
    ```csharp
    // Ejemplo hipotético de uso en una entidad 'Security_Users_Roles'
    // [ManyToOne(TableName = "Security_Users", KeyColumn = "Id_User", ForeignKeyColumn = "Id_User")]
    // public Security_Users? User { get; set; }
    ```

## 3. Relación entre Infraestructura y Entity

La relación entre los paquetes `Infraestructura` y `Entity` es fundamental para el funcionamiento del sistema:

*   **Entidades que Heredan de `EntityClass`**: Las clases de entidad en el paquete `Entity` heredan de `EntityClass` del paquete `Infraestructura`. Esto les otorga la capacidad de utilizar los métodos de persistencia definidos en `EntityClass`.

*   **Metadatos a través de Atributos**: Las entidades utilizan los atributos personalizados (como `PrimaryKey`, `OneToMany`, `ManyToOne`) definidos en `Infraestructura` para describir su estructura y relaciones. La lógica en `Infraestructura` (o un componente que la implementa) lee estos atributos a través de la reflexión para construir consultas SQL, gestionar relaciones y realizar operaciones de mapeo objeto-relacional.

*   **Separación de Preocupaciones**: Esta arquitectura promueve una clara separación de preocupaciones:
    *   **Entity**: Se encarga de la definición de los datos y las relaciones lógicas.
    *   **Infraestructura**: Se encarga de cómo esos datos se persisten, se consultan y se relacionan en un nivel más bajo (base de datos).

En resumen, el paquete `Infraestructura` proporciona el andamiaje y las convenciones (a través de `EntityClass` y los atributos personalizados) para que las entidades del paquete `Entity` puedan ser persistidas y manipuladas de manera consistente y desacoplada de la lógica de negocio directa. Los atributos actúan como un lenguaje declarativo para que la infraestructura entienda la estructura y las interacciones de las entidades.