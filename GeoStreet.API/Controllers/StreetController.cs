using GeoStreet.API.Models.DomainModels;
using GeoStreet.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries;
using System.Linq;
using System.IO;
using GeoStreet.API.Models.ViewModels;

namespace GeoStreet.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StreetController : ControllerBase
    {
        private readonly IStreetService _service;
        public StreetController(IStreetService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var streets = await _service.GetAllStreetsAsync();
            return Ok(streets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var street = await _service.GetStreetByIdAsync(id);
            if (street == null) return NotFound();
            return Ok(street);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StreetViewModel street)
        {
            await _service.CreateStreetAsync(street);
            return CreatedAtAction(nameof(GetById), new { id = street.Id }, street);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StreetViewModel streetViewModel)
        {
            if (id != streetViewModel.Id) return BadRequest();
            await _service.UpdateStreetAsync(streetViewModel);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteStreetAsync(id);
            return NoContent();
        }

        [HttpPut("add-point/{streetId}")]
        public async Task<IActionResult> AddPointToGeometry(int streetId, [FromBody] AddPointRequest request)
        {
            try
            {
                var updatedStreet = await _service.AddPointToStreetAsync(streetId, request);
                return Ok(updatedStreet);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}
