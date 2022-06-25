using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Models.Responses;

namespace MetaHR_API
{
    public class ValidationActionFilter : IActionFilter
    { 
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Keys
                .SelectMany(key => context.ModelState[key].Errors.Select(x => x.ErrorMessage).ToList());
                
                var cmdResult = CommandResult.GetErrorResult(errors);
                
                context.Result = new BadRequestObjectResult(cmdResult);
            }
        }
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
