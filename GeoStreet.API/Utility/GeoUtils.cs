using NetTopologySuite.Geometries;

namespace GeoStreet.API.Utility
{
    public class GeoUtils
    {
        public static double CalculateDistance(Coordinate c1, Coordinate c2)
        {
            const double R = 6371e3; // Earth's radius in meters

            var lat1 = c1.Y * (Math.PI / 180); // Latitude of first coordinate
            var lat2 = c2.Y * (Math.PI / 180); // Latitude of second coordinate
            var deltaLat = (c2.Y - c1.Y) * (Math.PI / 180);
            var deltaLon = (c2.X - c1.X) * (Math.PI / 180);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(lat1) * Math.Cos(lat2) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c; // Distance in meters
        }
    }
}
