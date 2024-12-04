using GeoStreet_Service.Models.DomainModels;
using GeoStreet_Service.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace GeoStreet_Service.Controllers
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
        public async Task<IActionResult> Create([FromBody] Street street)
        {
            await _service.CreateStreetAsync(street);
            return CreatedAtAction(nameof(GetById), new { id = street.Id }, street);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Street street)
        {
            if (id != street.Id) return BadRequest();
            await _service.UpdateStreetAsync(street);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteStreetAsync(id);
            return NoContent();
        }
    }
}