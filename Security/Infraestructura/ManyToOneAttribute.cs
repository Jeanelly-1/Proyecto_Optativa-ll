namespace APPCORE.Security
{
    internal class ManyToOneAttribute : Attribute
    {
        public string? TableName { get; set; }
        public string? KeyColumn { get; set; }
        public string? ForeignKeyColumn { get; set; }
    }
}
