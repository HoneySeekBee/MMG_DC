using DCProtocol.Auth;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DCWebServer.Swagger;

public class RefreshRequestExample : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content?.ContainsKey("application/json") != true)
            return;

        var hasRefreshRequest = context.ApiDescription.ParameterDescriptions
            .Any(p => p.Type == typeof(RefreshRequest));
        if (!hasRefreshRequest)
            return;

        operation.RequestBody.Content["application/json"].Example = new Microsoft.OpenApi.Any.OpenApiObject
        {
            ["refreshToken"] = new Microsoft.OpenApi.Any.OpenApiString("로그인_응답의_refreshToken_값을_여기에_붙여넣기")
        };
    }
}
