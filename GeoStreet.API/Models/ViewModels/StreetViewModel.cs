
namespace GeoStreet.API.Models.ViewModels
{
    public class StreetViewModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Geometry { get; set; } // PostGIS-compatible spatial type
        public int Capacity { get; set; }
    }
}
