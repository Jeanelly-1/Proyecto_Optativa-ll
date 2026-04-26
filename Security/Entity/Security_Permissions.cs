namespace APPCORE.Security
{
    public class Security_Permissions : EntityClass
	{
		[PrimaryKey(Identity = true)]
		public int? Id_Permission { get; set; }
		public string? Descripcion { get; set; }
		public string? Detalles { get; set; }
		public string? Estado { get; set; }

    }
}
