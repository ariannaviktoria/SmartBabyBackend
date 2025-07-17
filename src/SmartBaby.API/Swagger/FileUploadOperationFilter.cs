using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace SmartBaby.API.Swagger;

/// <summary>
/// Operation filter to properly handle file uploads in Swagger UI
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileUploadParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile) || 
                       (p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>) && Nullable.GetUnderlyingType(p.ParameterType) == typeof(IFormFile)) ||
                       p.ParameterType == typeof(IEnumerable<IFormFile>) ||
                       p.ParameterType == typeof(IFormFileCollection))
            .ToArray();

        if (fileUploadParams.Any())
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>(),
                            Required = new HashSet<string>()
                        }
                    }
                }
            };

            var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

            // Add all parameters to the schema
            foreach (var parameter in context.MethodInfo.GetParameters())
            {
                var paramName = parameter.Name!;
                var isFileParam = parameter.ParameterType == typeof(IFormFile) || 
                                 IsNullableOfType(parameter.ParameterType, typeof(IFormFile));

                // Skip if property already exists to avoid duplicates
                if (schema.Properties.ContainsKey(paramName))
                {
                    continue;
                }

                if (isFileParam)
                {
                    schema.Properties[paramName] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = GetFileDescription(paramName)
                    };

                    // Mark as required if parameter is not nullable
                    if (parameter.ParameterType == typeof(IFormFile))
                    {
                        schema.Required.Add(paramName);
                    }
                }
                else
                {
                    // Handle other form parameters
                    var openApiSchema = GetSchemaForType(parameter.ParameterType);
                    if (openApiSchema != null)
                    {
                        schema.Properties[paramName] = openApiSchema;
                        
                        // Check if parameter has default value
                        if (!parameter.HasDefaultValue && !IsNullable(parameter.ParameterType))
                        {
                            schema.Required.Add(paramName);
                        }
                    }
                }
            }

            // Remove parameters that are handled by the request body
            operation.Parameters?.Clear();
        }
    }

    private static string GetFileDescription(string paramName)
    {
        return paramName.ToLower() switch
        {
            "imagefile" => "Image file (JPEG, PNG, BMP) - Max size: 10MB",
            "audiofile" => "Audio file (WAV, MP3) - Max size: 50MB", 
            "videofile" => "Video file (MP4, AVI, MOV, WMV) - Max size: 200MB",
            _ => "File to upload"
        };
    }

    private static OpenApiSchema? GetSchemaForType(Type type)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.Name switch
        {
            "Int32" => new OpenApiSchema { Type = "integer", Format = "int32" },
            "Int64" => new OpenApiSchema { Type = "integer", Format = "int64" },
            "Single" => new OpenApiSchema { Type = "number", Format = "float" },
            "Double" => new OpenApiSchema { Type = "number", Format = "double" },
            "Boolean" => new OpenApiSchema { Type = "boolean" },
            "String" => new OpenApiSchema { Type = "string" },
            "DateTime" => new OpenApiSchema { Type = "string", Format = "date-time" },
            _ => new OpenApiSchema { Type = "string" }
        };
    }

    private static bool IsNullable(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }

    private static bool IsNullableOfType(Type type, Type targetType)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return Nullable.GetUnderlyingType(type) == targetType;
        }
        
        // For reference types, check if it's the target type
        return type == targetType && !type.IsValueType;
    }
}
