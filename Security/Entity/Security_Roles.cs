namespace APPCORE.Security
{
    public class Security_Roles : EntityClass
	{
		[PrimaryKey(Identity = true)]
		public int? Id_Role { get; set; }
		public string? Descripcion { get; set; }
		public string? Estado { get; set; }
		[OneToMany(TableName = "Security_Permissions_Roles", KeyColumn = "Id_Role", ForeignKeyColumn = "Id_Role")]
		public List<Security_Permissions_Roles>? Security_Permissions_Roles { get; set; }
		
	}
}
