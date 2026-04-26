namespace APPCORE.Security
{
    public abstract class EntityClass
    {
        public List<T> Get<T>(string condition = "")
        {
            throw new NotImplementedException();
        }
        // Método para filtrar una lista de entidades según una o más condiciones
        public List<T> Where<T>(/*params FilterData[]? where_condition*/)
        {
            throw new NotImplementedException();
        }
        // Método para encontrar una entidad que cumpla ciertas condiciones
        public T? Find<T>(/*params FilterData[]? where_condition*/)
        {
            throw new NotImplementedException();
        }
        public Boolean Exists()
        {
            throw new NotImplementedException();
        }
        // Método para guardar una entidad en la base de datos
        public object? Save(bool fullInsert = true)
        {
            throw new NotImplementedException();
        }
        // Método para actualizar una entidad en la base de datos
        public ResponseService Update()
        {
            throw new NotImplementedException();
        }
    }

    public class ResponseService
    {
    }
}
