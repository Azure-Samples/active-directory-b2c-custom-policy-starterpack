using AADB2C.UserMigration.API.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AADB2C.UserMigration.API.Controllers
{
    public class PrePasswordResetController : ApiController
    {
        [HttpPost]
        [Route("api/PrePasswordReset/LoalAccountSignIn")]
        public IHttpActionResult LoalAccountSignIn()
        {
            // If not data came in, then return
            if (this.Request.Content == null)
            {
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("Request content is NULL", HttpStatusCode.Conflict));
            }

            // Read the input claims from the request body
            string input = Request.Content.ReadAsStringAsync().Result;

            // Check input content value
            if (string.IsNullOrEmpty(input))
            {
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("Request content is empty", HttpStatusCode.Conflict));
            }

            // Convert the input string into InputClaimsModel object
            InputClaimsModel inputClaims = JsonConvert.DeserializeObject(input, typeof(InputClaimsModel)) as InputClaimsModel;

            if (inputClaims == null)
            {
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            // Note: Azure Blob Table query is case sensitive, always set the input email to lower case
            TableEntity userMigrationEntity = UserMigrationService.RetrieveUser(inputClaims.email.ToLower());

            if (userMigrationEntity != null)
            {
                Trace.WriteLine($"User '{inputClaims.email.ToLower()}' should reset the password");
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("You need to change your password. Please click on the 'Forgot your password?' link below", HttpStatusCode.Conflict));
            }
            else
            {
                Trace.WriteLine($"User '{inputClaims.email.ToLower()}' not found in the migration table, no action required");
            }

            return Ok();
        }

        [HttpPost]
        [Route("api/PrePasswordReset/PasswordUpdated")]
        public IHttpActionResult PasswordUpdated()
        {
            // If no data came in, then return
            if (this.Request.Content == null)
            {
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("Request content is NULL", HttpStatusCode.Conflict));
            }

            // Read the input claims from the request body
            string input = Request.Content.ReadAsStringAsync().Result;

            // Check input content value
            if (string.IsNullOrEmpty(input))
            {
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("Request content is empty", HttpStatusCode.Conflict));
            }

            // Convert the input string into InputClaimsModel object
            InputClaimsModel inputClaims = JsonConvert.DeserializeObject(input, typeof(InputClaimsModel)) as InputClaimsModel;

            if (inputClaims == null)
            {
                return Content(HttpStatusCode.Conflict, new B2CResponseContent("Can not deserialize input claims", HttpStatusCode.Conflict));
            }

            // Note: Azure Blob Table query is case sensitive, always set the input email to lower case
            TableEntity userMigrationEntity = UserMigrationService.RetrieveUser(inputClaims.email.ToLower());

            if (userMigrationEntity != null)
            {
                // Remove the user entity from migration table
                UserMigrationService.RemoveUser(inputClaims.email.ToLower());
            }

            Trace.WriteLine($"User '{inputClaims.email.ToLower()}' reset the password successfully");

            return Ok();
        }
    }
}