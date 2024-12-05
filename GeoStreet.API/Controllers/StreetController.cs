using GeoStreet.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPut("{id}/geometry/add-point")]
        public async Task<IActionResult> AddPointToGeometry(int id, [FromBody] AddPointRequest request)
        {
            try
            {
                bool updateResult = await _service.AddPointToStreetAsync(id, request);
                string result = updateResult ? "Street updated successfully" : "Update failed";
                return Ok(updateResult);
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
