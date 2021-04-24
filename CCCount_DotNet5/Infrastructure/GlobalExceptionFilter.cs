using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Net;
using Microsoft.Extensions.Logging;

namespace CCCount.Infrastructure
{
    public class GlobalExceptionFilter : ActionFilterAttribute, IExceptionFilter
    {
        private readonly ILogger _logger;

        public GlobalExceptionFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GlobalExceptionFilter>();
        }

        public void OnException(ExceptionContext context)
        {
            // TODO: Save context.Exception.StackTrace to log

            var exceptionType = context.Exception.GetType();
            var response = context.HttpContext.Response;
            var status = HttpStatusCode.InternalServerError;
            var message = String.Empty;

            // Handle exception based on type
            //  - Add specific handling for exception type here
            //  - Uses the default case normally
            if (exceptionType == typeof(System.NullReferenceException)) {
                message = "Null reference error.";
                status = HttpStatusCode.NotImplemented;
            } else {
                message = context.Exception.Message;
                status = HttpStatusCode.InternalServerError;
            }

            // Log error
            _logger.LogError($"{message} ({status.ToString()})");

            // Build response object
            response.StatusCode = (int)status;
            response.ContentType = "application/json";
            response.WriteAsync($"{message}", Encoding.UTF8);

            // Set exception handled
            context.ExceptionHandled = true;
        }
    }

    
}
