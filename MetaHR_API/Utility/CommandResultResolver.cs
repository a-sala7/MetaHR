using Microsoft.AspNetCore.Mvc;
using Models.Responses;

namespace MetaHR_API.Utility
{
    public static class CommandResultResolver
    {
        public static IActionResult Resolve(CommandResult cmd)
        {
            if (cmd.InternalError)
            {
                //return the static UnknownInternalErrorResult
                //so that users don't get to see internal exception details
                return new ObjectResult(CommandResult.UnknownInternalErrorResult)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }

            if (cmd.NotFound)
            {
                return new ObjectResult(cmd)
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (cmd.IsSuccessful is false)
            {
                return new ObjectResult(cmd)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            return new ObjectResult(cmd)
            {
                StatusCode = StatusCodes.Status200OK
            };
        }
    }
}
