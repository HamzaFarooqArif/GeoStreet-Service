using NetTopologySuite.Geometries;

namespace GeoStreet.API.Models.DomainModels
{
    public class Street
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required LineString Geometry { get; set; } // PostGIS-compatible spatial type
        public int Capacity { get; set; }
    }
}
