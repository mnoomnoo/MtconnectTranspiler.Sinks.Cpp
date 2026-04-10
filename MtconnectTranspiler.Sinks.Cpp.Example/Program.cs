using ConsoulLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MtconnectTranspiler;
using MtconnectTranspiler.CodeGenerators.ScribanTemplates;
using MtconnectTranspiler.Extensions;
using MtconnectTranspiler.Interpreters;
using MtconnectTranspiler.Sinks.Cpp.Example;
using MtconnectTranspiler.XmiOptions;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0) throw new ArgumentNullException(nameof(args), "Missing output directory argument");

        string outputDir = args[0];
        if (!Directory.Exists(outputDir))
        {
            Consoul.Write("Creating project output path: " + outputDir);
            Directory.CreateDirectory(outputDir);
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#else
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#endif
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var services = new ServiceCollection()
            .AddLogging((builder) =>
            {
                builder.AddConsoulLogger();
            });
        var serviceProvider = services
            .AddSingleton(configuration)
            .AddScribanServices(builder =>
            {
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
                builder
                    .ConfigureTemplateLoader((loader) =>
                        loader.UseTemplatesPath(templatePath)
                            .AddMarkdownInterpreter("vscode_docs", new VisualStudioSummaryInterpreter())
                    )
                    .ConfigureGenerator((options) => {
                        options.OutputPath = outputDir;
                    });
            })
            .AddScoped<Transpiler>()
            .BuildServiceProvider();

        var logger = serviceProvider.GetService<ILoggerFactory>()
            .CreateLogger<Program>();
        logger.LogDebug("Starting application");

        TranspilerDispatcherOptions? dispatchOptions = null;

        string modelDir = "";
        if (args.Length > 1)
            modelDir = args[1];

        if (!string.IsNullOrEmpty(modelDir))
        {
            if (!File.Exists(modelDir)) throw new FileNotFoundException(modelDir);
            dispatchOptions = new FromFileOptions() { Filepath = modelDir };
            Consoul.Write("Dispatching from file: " + modelDir);
        }
        else
        {
            dispatchOptions = new FromGitHubOptions() { GitHubRelease = "latest" };
            Consoul.Write("Dispatching from GitHub's latest release");
        }

        using (var tokenSource = new CancellationTokenSource())
        using (var dispatcher = new TranspilerDispatcher(dispatchOptions, serviceProvider.GetService<ILoggerFactory>()
            .CreateLogger<TranspilerDispatcher>()))
        {
            var defaultTranspiler = serviceProvider.GetService<Transpiler>();
            dispatcher.AddSink(defaultTranspiler);

            Consoul.Write("Beginning deserialization and dispatching");
            var task = Task.Run(() => dispatcher.TranspileAsync(tokenSource.Token));

#if DEBUG
            task = task.ContinueWith((t) => tokenSource.Cancel());
            Consoul.Wait(cancellationToken: tokenSource.Token);
#else
            task.Wait();
#endif

            if (task.IsCompletedSuccessfully)
            {
                Consoul.Write("Done!", ConsoleColor.Green);
                Environment.Exit(0);
            }
            else
            {
                Consoul.Write("Cancelled", ConsoleColor.Red);
                Environment.Exit(1);
            }
        }
    }
}
