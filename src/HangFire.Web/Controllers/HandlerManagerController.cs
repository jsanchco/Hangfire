using HangFire.CommonApi.Controllers;
using HangFire.Web.HostedServices;
using Microsoft.AspNetCore.Mvc;

namespace HangFire.Web.Controllers
{
    [ApiController]
    public class HandlerManagerController : BaseApiController
    {
        private readonly ILogger<HandlerManagerController> _logger;
        private readonly HandlerManagerServices _handlerManagerServices;

        public HandlerManagerController(
            ILogger<HandlerManagerController> logger,
            HandlerManagerServices handlerManagerServices)
        {
            _logger = logger;
            _handlerManagerServices = handlerManagerServices;
        }

        [HttpGet("services")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Services()
        {
            if (!_handlerManagerServices.Available)
                throw new Exception($"Services NOT Availables. Check configuration please");

            return Ok(_handlerManagerServices.JobServices);
        }

        [HttpPut("services/reload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ServicesReload(CancellationToken stoppingToken)
        {
            _handlerManagerServices.ReloadServices(stoppingToken);
            if (!_handlerManagerServices.Available)
                throw new Exception($"Services NOT Availables. Check configuration please");

            return Ok(_handlerManagerServices.JobServices);
        }
    }
}
