namespace SutureHealth.Hchb.Services
{
    public class DbContextSchema: IDbContextSchema
    {
        public string Schema { get; }
        public DbContextSchema(string schema)
        {
            Schema = schema;
        }
    }
}
