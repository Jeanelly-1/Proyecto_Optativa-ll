namespace APPCORE.Security
{
    public class Security_Permissions_Roles : EntityClass
	{
		[PrimaryKey(Identity = false)]
		public int? Id_Role { get; set; }
		[PrimaryKey(Identity = false)]
		public int? Id_Permission { get; set; }
		public string? Estado { get; set; }
		[ManyToOne(TableName = "Security_Permissions", KeyColumn = "Id_Permission", ForeignKeyColumn = "Id_Permission")]
		public Security_Permissions? Security_Permissions { get; set; }
	}
}
