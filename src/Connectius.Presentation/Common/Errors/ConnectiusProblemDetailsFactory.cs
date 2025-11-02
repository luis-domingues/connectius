using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace Connectius.Presentation.Common.Errors;

public class ConnectiusProblemDetailsFactory : ProblemDetailsFactory
{
    private readonly ApiBehaviorOptions _options;

    public ConnectiusProblemDetailsFactory(IOptions<ApiBehaviorOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }
    
    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext, 
        int? statusCode = null, 
        string? title = null,
        string? type = null, 
        string? detail = null, 
        string? instance = null)
    {
        statusCode ??= 500;

        var details = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = type,
            Detail = detail,
            Instance = instance
        };

        ApplyProblemDetailsDefaults(httpContext, details, statusCode.Value);

        return details;
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ModelStateDictionary modelStateDictionary, 
        int? statusCode = null, 
        string? title = null, 
        string? type = null,
        string? detail = null, 
        string? instance = null)
    {
        statusCode ??= 400;
        
        var errors = modelStateDictionary
            .Where(x => x.Value!.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var validationDetails = new ValidationProblemDetails(errors)
        {
            Status = statusCode,
            Title = title ?? "One or more validation errors occurred.",
            Type = type,
            Detail = detail,
            Instance = instance
        };

        ApplyProblemDetailsDefaults(httpContext, validationDetails, statusCode.Value);

        return validationDetails;
    }

    private void ApplyProblemDetailsDefaults(
        HttpContext httpContext, 
        ProblemDetails problemDetails,
        int statusCode)
    {
        problemDetails.Status ??= statusCode;

        if (_options.ClientErrorMapping.TryGetValue(statusCode, out var userErrorData))
        {
            problemDetails.Title ??= userErrorData.Title;
            problemDetails.Type ??= userErrorData.Link;
        }

        var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
        if (traceId != null)
        {
            problemDetails.Extensions["traceId"] = traceId;
        }
        
        problemDetails.Extensions.Add("userProperty", "userValue");
    }
}