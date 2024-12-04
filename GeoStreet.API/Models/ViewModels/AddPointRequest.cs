namespace GeoStreet.API.Models.ViewModels
{
    public class AddPointRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool AddToEnd { get; set; }
    }
}
