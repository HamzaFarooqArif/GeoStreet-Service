namespace GeoStreet.API.Models.DomainModels
{
    public class Street
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Geometry { get; set; } // PostGIS geometry in WKT format
        public int Capacity { get; set; }
    }
}
