namespace APPCORE.Security
{
    public class Tbl_Profile : EntityClass
	{
		[PrimaryKey(Identity = true)]
		public int? Id_Perfil { get; set; }
		public string? Nombres { get; set; }
		public string? Apellidos { get; set; }
		public DateTime? FechaNac { get; set; }
		public int? IdUser { get; set; }
		public string? Sexo { get; set; }
		public string? Foto { get; set; }
		public string? DNI { get; set; }
		public string? Correo_institucional { get; set; }
		public string? Estado { get; set; }

		public string GetNombreCompleto()
		{
			return $"{Nombres} {Apellidos}";
		}
	}
}
