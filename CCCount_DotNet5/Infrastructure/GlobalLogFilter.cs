﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace CCCount.Infrastructure
{
    public class GlobalLogFilter : ActionFilterAttribute
    {
        private readonly ILogger _logger;

        public GlobalLogFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GlobalLogFilter>();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //_logger.LogInformation("OnActionExecuting");
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //_logger.LogInformation("OnActionExecuted");
            base.OnActionExecuted(context);
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            //_logger.LogInformation("OnResultExecuting");
            base.OnResultExecuting(context);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            //_logger.LogInformation("OnResultExecuted");
            base.OnResultExecuted(context);
        }
    }
}
