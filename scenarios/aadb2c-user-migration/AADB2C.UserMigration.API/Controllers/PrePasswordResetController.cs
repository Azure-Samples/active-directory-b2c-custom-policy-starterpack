using AADB2C.UserMigration.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace AADB2C.UserMigration.API.Controllers
{
    [Route("api/[controller]")]
    public class PrePasswordResetController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly UserMigrationService _userMigrationService;

        public PrePasswordResetController(ILogger<PrePasswordResetController> logger, UserMigrationService userMigrationService)
        {
            _logger = logger;
            _userMigrationService = userMigrationService;
        }

        [HttpPost("LocalAccountSignIn")]
        public async Task<IActionResult> LocalAccountSignInAsync([FromBody]InputClaimsModel inputClaims)
        {
            // If not data came in, then return
            if (inputClaims == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseContent("Request content is NULL", HttpStatusCode.BadRequest));
            }
            
            // Note: Azure Blob Table query is case sensitive, always set the input email to lower case
            var userMigrationEntity = await _userMigrationService.RetrieveUserAsync(inputClaims.email.ToLower());

            if (userMigrationEntity != null)
            {
                _logger.LogInformation($"User '{inputClaims.email.ToLower()}' should reset the password");
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseContent("You need to change your password. Please click on the 'Forgot your password?' link below", HttpStatusCode.Conflict));
            }
            else
            {
                _logger.LogInformation($"User '{inputClaims.email.ToLower()}' not found in the migration table, no action required");
            }

            return Ok();
        }

        [HttpPost("PasswordUpdated")]
        public async Task<IActionResult> PasswordUpdatedAsync([FromBody]InputClaimsModel inputClaims)
        {
            if (inputClaims == null)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseContent("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            // Note: Azure Blob Table query is case sensitive, always set the input email to lower case
            var userMigrationEntity = await _userMigrationService.RetrieveUserAsync(inputClaims.email.ToLower());

            if (userMigrationEntity != null)
            {
                // Remove the user entity from migration table
                await _userMigrationService.RemoveUser(inputClaims.email.ToLower());
            }

            _logger.LogInformation($"User '{inputClaims.email.ToLower()}' reset the password successfully");

            return Ok();
        }
    }
}