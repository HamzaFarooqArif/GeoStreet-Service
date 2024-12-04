﻿using GeoStreet_Service.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GeoStreet_Service.Controllers
{
    public class _SetupController : Controller
    {
        private readonly ISetupService _service;
        public _SetupController(ISetupService service)
        {
            _service = service;
        }

        [HttpPost("ApplyDatabaseMigrations")]
        public async Task<IActionResult> ApplyDatabaseMigrations()
        {
            try
            {
                await _service.ApplyDatabaseMigrations();
                return Ok("Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error applying migrations: {ex.Message}");
            }
        }
    }
}