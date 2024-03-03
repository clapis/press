using System.CommandLine;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Press.Hosts.Cli.Extensions;

public static class CommandBuilderExtensions
{
    public static Command AddSubcommand(this Command command, string name, Action<Command> configure)
    {
        var section = new Command(name);

        configure(section);
        
        command.AddCommand(section);

        return command;
    }
    
    public static Command AddSubcommand<TRequest>(this Command command, string name, string description) where TRequest : IBaseRequest
    {
        var subcommand = new Command(name, description);

        foreach (var prop in typeof(TRequest).GetProperties())
            subcommand.AddOption((Option)Activator.CreateInstance(typeof(Option<>).MakeGenericType(prop.PropertyType), $"--{prop.Name.ToLower()}", null)!);
            
        subcommand.Handler = CommandHandler.Create<InvocationContext, TRequest>(async (context, request) =>
        {
            using var host = context.GetHost();
            
            var mediator = host.Services.GetRequiredService<IMediator>();
            
            var response = await mediator.Send(request!, context.GetCancellationToken());
            
            if (response is not Unit)
                Console.WriteLine(JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true }));
        });
        
        command.AddCommand(subcommand);

        return command;
    }
}