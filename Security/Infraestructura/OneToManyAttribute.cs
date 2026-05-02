namespace APPCORE.Security
{
    internal class OneToManyAttribute : Attribute
    {
        public string? TableName { get; set; }
        public string? KeyColumn { get; set; }
        public string? ForeignKeyColumn { get; set; }
    }
}
