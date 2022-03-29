using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moveness.Models;
using System.Threading.Tasks;

namespace Moveness.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public AvailabilityController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Availability
        [HttpGet]
        [HttpHead]
        public async Task<IActionResult> Check()
        {
            if (await _context.Database.CanConnectAsync())
                return Ok();

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
