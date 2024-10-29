using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Press.Core.Features.Users.Create;

namespace Press.Hosts.WebAPI.Endpoints;

public static class WebhookEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    public static WebApplication MapWebhookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/webhooks");

        group.MapPost("/user-created",
                async ([FromServices] IMediator mediator, [FromServices] IOptionsSnapshot<JwtBearerOptions> options, HttpContext context) =>
                {
                    var result = await ValidateTokenAsync(context.Request.Body, options);

                    if (!result.IsValid) return Results.Unauthorized();

                    var jwt = (JsonWebToken) result.SecurityToken;
                    
                    if (jwt.GetPayloadValue<string>("type") != "user.created")
                        return Results.UnprocessableEntity();

                    var user = jwt.GetPayloadValue<JsonElement>("data")
                        .Deserialize<KindeWebhookData>(JsonOptions)!
                        .User;
                    
                    await mediator.Send(new CreateUser(user.Id, user.Email));

                    return Results.Ok();
                });

        return app;
    }

    private static async Task<TokenValidationResult> ValidateTokenAsync(Stream body, IOptionsSnapshot<JwtBearerOptions> options)
    {
        using var reader = new StreamReader(body);
        
        var token = await reader.ReadToEndAsync();

        var handler = new JsonWebTokenHandler();
        
        var configuration = (BaseConfigurationManager)options.Get("Bearer").ConfigurationManager!;

        return await handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateLifetime = false,
            ValidateIssuer = false,
            ValidateAudience = false,
            ConfigurationManager = configuration
        });
    }

    //  ********** Webhook example data **************
    // {
    //     "data": {
    //         "user": {
    //             "email": "email@example.com",
    //             "first_name": null,
    //             "id": "kp_43aa0000cf6c4d7b0ee6f35dcf10000",
    //             "is_password_reset_requested": false,
    //             "is_suspended": false,
    //             "last_name": null,
    //             "organizations": [
    //             {
    //                 "code": "org_8993161880b",
    //                 "permissions": null,
    //                 "roles": null
    //             }
    //             ],
    //             "phone": null,
    //             "username": null
    //         }
    //     },
    //     "event_id": "event_0142d56b6251f3c96624fc286ee7e1d8",
    //     "event_timestamp": "2024-10-29T10:17:16.933127+11:00",
    //     "source": "user",
    //     "timestamp": "2024-10-29T10:17:17.309867177+11:00",
    //     "type": "user.created"
    // }

    record KindeWebhookData(KindeUser User);
    record KindeUser(string Id, string Email);

}