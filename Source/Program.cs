/////////////////////////////////////////////////////////////////////////////////////
//  File:   Program.cs                                              26 May 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

using CallLoadTester.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace CallLoadTester;

using SipLib.Logging;
using SipLib.Network;
using System.Diagnostics;

public class Program
{
    public const string AppName = "CallLoadTester";
    public static string Version = "1.0.0";

    //public const string HelpUriBase = "http://localhost:8080/docs/";
    public const string HelpUriBase = "https://PhrSite.github.io/CallLoadTester/docs/";
    public const string GettingStartedUri = HelpUriBase + "GettingStarted.html";
    public const string HomePageHelpUri = HelpUriBase + "MainPage.html";
    public const string SettingsPageHelpUri = HelpUriBase + "SettingsPage.html";
    public const string CallQualityPageHelpUri = HelpUriBase + "CallQualitySummary.html";
    public const string CallDetailsPageHelpHri = HelpUriBase + "CallDetailsPage.html";

    public const string MosCalculatorPageHelpUri = HelpUriBase + "MeanOpinionScore.html";

    private const string LoggingFileName = $"{AppName}.log";
    private static LoggingLevelSwitch m_LevelSwitch = new LoggingLevelSwitch();

    private const string DEFAULT_CERTIFICATE_FILE = "CallLoadTester.pfx";
    private const string DEFAULT_CERTIFICATE_PASSWORD = "CallLoadTester";

    public static void Main(string[] args)
    {
        string strVer = System.Reflection.Assembly.GetEntryAssembly()!.GetName()!.Version!.ToString();
        strVer = strVer.Substring(0, strVer.LastIndexOf("."));  // Only display the first 3 digits of the version
        Version = $"({strVer})";
        Console.WriteLine($"CallLoadTester Version = {strVer}");


        // Setup application logging using Serilog
        string LoggingDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{AppName}/Logs";
        if (Directory.Exists(LoggingDirectory) == false)
            Directory.CreateDirectory(LoggingDirectory);
        string LoggingPath = LoggingDirectory + $"/{LoggingFileName}";
        Logger log = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(m_LevelSwitch)
            .WriteTo.File(LoggingPath, fileSizeLimitBytes: 1000000, retainedFileCountLimit: 5,
            outputTemplate: "{Timestamp:yyyy-MM-ddTHH:mm:ss.ffffffzzz} [{Level}] {Message}{NewLine}{Exception}")
            .CreateLogger();
        SerilogLoggerFactory factory = new SerilogLoggerFactory(log);
        SipLogger.Log = factory.CreateLogger(AppName);

        SipLogger.LogInformation($"Starting {AppName} now");
        Console.WriteLine($"Application logging file = {LoggingPath}");

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();

        // 31 May 26 PHR
        builder.Services.AddMvc((options =>
        {
            options.InputFormatters.Add(new XmlSerializerInputFormatter(new MvcOptions()));
            options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
        }));

        // 31 May 26 PHR
        builder.WebHost.ConfigureKestrel(options =>
        {
            // HTTPS binding with a self-signed certificate
            options.Listen(IPAddress.Any, 5001, listenOptions =>
            {
                listenOptions.UseHttps(DEFAULT_CERTIFICATE_FILE, DEFAULT_CERTIFICATE_PASSWORD);
            });
        });

        // Listening on IPAddress.Any:5001 so tell the user which URIs to connect to in the browser.
        List<IPAddress> ipAddresses = IpUtils.GetIPv4Addresses();
        Console.WriteLine("Listening on: https://localhost:5001");
        foreach (IPAddress ipAddress in ipAddresses)
        {
            Console.WriteLine($"Listening on: https://{ipAddress}:5001");
        }

        X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12FromFile(DEFAULT_CERTIFICATE_FILE, DEFAULT_CERTIFICATE_PASSWORD);
        builder.Services.AddSingleton<CallManagerService>(sp => new CallManagerService(certificate));

        // Completely wipes out console, debug, and trace outputs
        builder.Logging.ClearProviders();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        // For debug only
        if (OperatingSystem.IsWindows() == true)
        {
            Task.Factory.StartNew(() =>
            {
                ProcessStartInfo psi = new ProcessStartInfo("https://localhost:5001")
                {
                    UseShellExecute = true
                };

                Process.Start(psi);
            });
        }

        app.Run();

    }
}
