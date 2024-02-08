using Press.Core;
using Press.Hosts.WebAPI.Jobs;
using Press.Infrastructure.MongoDb;
using Press.Infrastructure.MongoDb.Configuration;
using Press.Infrastructure.Postmark;
using Press.Infrastructure.Postmark.Configuration;
using Press.Infrastructure.Scrapers;

var builder = WebApplication.CreateBuilder(args);
    
builder.Logging
    .AddSeq(builder.Configuration.GetSection("Seq"));

T GetSettings<T>(string key) => builder.Configuration.GetRequiredSection(key).Get<T>()!;

builder.Services
    .AddCore()
    .AddScrapers()
    .AddQuartzJobs()
    .AddMongoDb(GetSettings<MongoDbSettings>("MongoDb"))
    .AddPostMark(GetSettings<PostmarkSettings>("Postmark"));

builder.Services
    .AddSwaggerGen()
    .AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


// app.MapGet("/weatherforecast", () =>
//     {
//         var forecast = Enumerable.Range(1, 5).Select(index =>
//                 new WeatherForecast
//                 (
//                     DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//                     Random.Shared.Next(-20, 55),
//                     summaries[Random.Shared.Next(summaries.Length)]
//                 ))
//             .ToArray();
//         return forecast;
//     })
//     .WithName("GetWeatherForecast")
//     .WithOpenApi();

app.Run();


