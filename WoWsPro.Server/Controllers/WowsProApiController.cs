using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WoWsPro.Shared.Exceptions;

namespace WoWsPro.Server.Controllers
{
    public abstract class WowsProApiController : ControllerBase
    {
        [NonAction]
        public IActionResult Error (Exception exception)
        {
            return exception switch
            {
                ArgumentException => BadRequest(new { Reason = exception.Message }),
                NotSupportedException => BadRequest(new { Reason = exception.Message }),
                KeyNotFoundException => NotFound(),
                UnauthorizedException => Unauthorized(),
                UnauthenticatedException => Redirect("/account/login"),
                _ => StatusCode(500)
            };
        }
    }
}