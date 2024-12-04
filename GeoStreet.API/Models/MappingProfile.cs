using AutoMapper;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using GeoStreet.API.Models.ViewModels;
using GeoStreet.API.Models.DomainModels;

namespace GeoStreet.API.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Street, StreetViewModel>()
            .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src =>
                src.Geometry == null ? null : new WKTWriter().Write(src.Geometry)))
            .ReverseMap()
            .ForMember(dest => dest.Geometry, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.Geometry) ? null : new WKTReader().Read(src.Geometry) as LineString));
        }
    }
}
