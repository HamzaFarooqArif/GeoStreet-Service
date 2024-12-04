using AutoMapper;
using GeoStreet.API.Models.DomainModels;
using GeoStreet.API.Models.ViewModels;
using GeoStreet.API.Respository;
using GeoStreet.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.IO;

namespace GeoStreet.API.Services.Implementations
{
    public class StreetService : IStreetService
    {
        private readonly IStreetRepository _repository;
        private readonly IMapper _mapper;

        public StreetService(IStreetRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StreetViewModel>> GetAllStreetsAsync()
        {
            IEnumerable<StreetViewModel> streets = _mapper.Map<IEnumerable<StreetViewModel>>(await _repository.GetAllAsync());

            return streets;
        }

        public async Task<StreetViewModel> GetStreetByIdAsync(int id)
        {
            return _mapper.Map<StreetViewModel>(await _repository.GetByIdAsync(id));
        }

        public async Task CreateStreetAsync(StreetViewModel streetViewModel)
        {
            Street street = _mapper.Map<Street>(streetViewModel);

            await _repository.AddAsync(street);
        }

        public async Task UpdateStreetAsync(StreetViewModel streetViewModel)
        {
            await _repository.UpdateAsync(_mapper.Map<Street>(streetViewModel));
        }

        public async Task DeleteStreetAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<StreetViewModel> AddPointToStreetAsync(int id, AddPointRequest request)
        {
            var street = await _repository.GetByIdAsync(id);
            if (street == null)
            {
                throw new KeyNotFoundException($"Street with ID {id} not found.");
            }

            var newPoint = new Point(request.Longitude, request.Latitude) { SRID = 4326 };

            if (street.Geometry == null)
            {
                street.Geometry = new LineString(new[] { newPoint.Coordinate }) { SRID = 4326 };
            }
            else
            {
                if (request.AddToEnd)
                {
                    street.Geometry = new LineString(street.Geometry.Coordinates.Append(newPoint.Coordinate).ToArray()) { SRID = 4326 };
                }
                else
                {
                    street.Geometry = new LineString(new[] { newPoint.Coordinate }.Concat(street.Geometry.Coordinates).ToArray()) { SRID = 4326 };
                }
            }

            await _repository.UpdateAsync(street);

            return _mapper.Map<StreetViewModel>(street);
        }
    }
}
