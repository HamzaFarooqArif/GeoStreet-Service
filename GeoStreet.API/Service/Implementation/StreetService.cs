using AutoMapper;
using GeoStreet.API.Models.DomainModels;
using GeoStreet.API.Models.ViewModels;
using GeoStreet.API.Respository;
using GeoStreet.API.Services.Interfaces;
using GeoStreet.API.Utility;
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

        public async Task<bool> AddPointToStreetAsync(int id, AddPointRequest request)
        {
            bool result = false;
            Coordinate newCoordinate = new Coordinate(request.Longitude, request.Latitude);

            // Fetch only the start and end coordinates of the LineString from the database
            var startAndEndCoordinates = await _repository.GetStartAndEndCoordinatesAsync(id);
            if (startAndEndCoordinates == null || startAndEndCoordinates.Length == 0)
            {
                result = await _repository.AddPointAsync(id, newCoordinate, false);
            }
            else
            {
                // Extract start and end coordinates
                var startCoordinate = startAndEndCoordinates.FirstOrDefault();
                var endCoordinate = startAndEndCoordinates.LastOrDefault();

                // Calculate distances to start and end
                double distanceToStart = GeoUtils.CalculateDistance(newCoordinate, startCoordinate);
                double distanceToEnd = GeoUtils.CalculateDistance(newCoordinate, endCoordinate);

                // Decide whether to insert at the start or end
                bool addToEnd = distanceToStart >= distanceToEnd;
                result = await _repository.AddPointAsync(id, newCoordinate, addToEnd);
            }

            return result;
        }
    }
}
