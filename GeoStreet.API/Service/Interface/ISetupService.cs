namespace GeoStreet.API.Services.Interfaces
{
    public interface ISetupService
    {
        Task ApplyDatabaseMigrations();
        Task DeleteDatabase();
    }
}
