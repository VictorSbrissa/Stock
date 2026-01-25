namespace WebApplication1.Infrastructure
{
    public interface IDbContextConnectionStringProvider
    {
        string ConnectionString { get; set; }
    }

    public class DbContextConnectionStringProvider : IDbContextConnectionStringProvider
    {
        public string ConnectionString { get; set; }
    }
}