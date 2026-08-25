using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkerSafetyDashboard.Models;
using WorkerSafetyDashboard.Services;

namespace WorkerSafetyDashboard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FortyGuardController : ControllerBase
    {
        private readonly IFortyGuardService _fortyGuardService;
        public FortyGuardController(IFortyGuardService fortyGuardService)
        {
            _fortyGuardService = fortyGuardService;
        }


        [HttpPost("enviParam")] 
        public async Task<IActionResult> SubmitEnvirParam([FromBody] EnvParamsRequest request)
        {
            try
            {
                var checkReq = request;
                //var envResult = await _fortyGuardService.GetEnvironmentalParametersAsync(request);
                //var activityId = await _fortyGuardService.SubmitEnvirParamAsync(request);
                // return Ok(new { ActivityId = activityId });
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here if needed
               // return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex.Message });
            }
            return Ok(new { Message = "Request processed successfully." });
        }


        ///1-call api to get me temp today (external or hratmap )
        ///2- call api to get me env param 
        //3- create propre prompt to gimini 
        //4 call gimini and get response 
        //5- parse response to handle fall back for faliur 
        //6- responde


    }
}
