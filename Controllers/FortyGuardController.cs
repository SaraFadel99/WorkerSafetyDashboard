using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using WorkerSafetyDashboard.Models;
using WorkerSafetyDashboard.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WorkerSafetyDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FortyGuardController : ControllerBase
    {
        private readonly IFortyGuardService _fortyGuardService;
        private readonly IOpenMeteoService _openMeteoService;

        public FortyGuardController(IFortyGuardService fortyGuardService,IOpenMeteoService openMeteoService)
        {
            _fortyGuardService = fortyGuardService;
            _openMeteoService = openMeteoService;

        }


        //[HttpPost("enviParam")] 
        //public async Task<IActionResult> SubmitEnvirParam([FromBody] EnvParamsRequest request)
        //{
        //    var validation = SafetyRequestValidator.Validate(request.Latitude, request.Longitude, DateTime.Parse(request.DateTime.StartDate));
        //    if (!validation.IsValid)
        //        return BadRequest(new { error = validation.ErrorMessage });
        //    try
        //    {
        //        request.Temperature = await _openMeteoService.GetTemperatureAsync(
        //        request.Latitude, request.Longitude, request.DateTime);
        //        var envResult = await _fortyGuardService.GetEnvironmentalParametersAsync(request);
        //         return Ok(new { envResult });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception (ex) here if needed
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex.Message });
        //    }
        //}



        //[HttpPost("SubmitTest")]
        //public async Task<IActionResult> SubmitTest([FromBody] EnvParamsRequest request)
        //{
        //    var validation = SafetyRequestValidator.Validate(
        //        request.Latitude, request.Longitude, DateTime.Parse(request.DateTime.StartDate));
        //    if (!validation.IsValid)
        //        return BadRequest(new { error = validation.ErrorMessage });

        //    try
        //    {
        //        request.Temperature = await _openMeteoService.GetTemperatureAsync(
        //         request.Latitude, request.Longitude, request.DateTime);
        //        //request.Temperature = await _openMeteoService.GetTemperatureAsync(
        //        //    request.Latitude, request.Longitude, request.DateTime);

        //        //var envResult = await _fortyGuardService.GetEnvironmentalParametersAsync(request);

        //        return Ok(request);
        //    }
        //    catch (Exception ex)
        //    {
        //        // TODO: swap for real logging (ILogger) before submission
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        //    }
        //}

        ///1-call api to get me temp today (external or hratmap )
        ///2- call api to get me env param 
        //3- create propre prompt to gimini 
        //4 call gimini and get response 
        //5- parse response to handle fall back for faliur 
        //6- responde


    }
}
