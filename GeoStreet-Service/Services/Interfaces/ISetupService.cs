namespace GeoStreet_Service.Services.Interfaces
{
    public interface ISetupService
    {
        Task ApplyDatabaseMigrations();
    }
}
