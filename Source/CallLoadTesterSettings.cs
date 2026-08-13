/////////////////////////////////////////////////////////////////////////////////////
//  File:   CallLoadTesterSettings.cs                               1 Jun 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

using Ng911Lib.Utilities;
using SipLib.Logging;
using SipLib.Core;
using SipLib.Network;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace CallLoadTester;

/// <summary>
/// Class for managing the configuration settings of the CallLoadTester application.
/// </summary>
public class CallLoadTesterSettings
{
    private const int MAX_TOTAL_CALLS = 10000;

    /// <summary>
    /// Specifies the total number of calls to generate
    /// </summary>
    [Required(ErrorMessage = "TotalCalls is required")]
    [Range(1, MAX_TOTAL_CALLS)]
    public int TotalCalls { get; set; } = 10;

    /// <summary>
    /// Specifies the number of simultaneous calls to generate
    /// </summary>
    [Required(ErrorMessage = "Simultaneous Calls is required.")]
    [Range(1, MAX_TOTAL_CALLS)]
    public int SimultaneousCalls { get; set; } = 10;

    /// <summary>
    /// Specifies the number of milliseconds to wait between generating each call. The minimum value is 0. 
    /// A value of 0 means that the thread generating call requests will not wait between call requests.
    /// The maximum value is not limited. The default value is 10 milliseconds.
    /// </summary>
    [Required(ErrorMessage = "Call Interval is required.")]
    [Range(0, int.MaxValue)]
    public int CallIntervalMs { get; set; } = 10;

    /// <summary>
    /// Specifies the maximum duration of a call. The Call Load Tester application will terminate calls
    /// by sending a BYE request if they exceed this duration.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int CallDurationSeconds { get; set; } = 10;

    private const long MIN_FROM_NUMBER = 1000000000;
    private const long MAX_FROM_NUMBER = 9999999999;

    /// <summary>
    /// Specifies the starting From 10-digit telephone number. The minimum value is 1000000000.
    /// This value is incremented by one for each new call request so that each call appears to originate
    /// from a unique caller.
    /// </summary>
    [Required(ErrorMessage = "The Starting From Number is required")]
    [Range(MIN_FROM_NUMBER, MAX_FROM_NUMBER)]
    public long StartingFromNumber { get; set; } = MIN_FROM_NUMBER;

    /// <summary>
    /// If true, then the application will use the IPv4 transport protocol
    /// </summary>
    public bool EnableIPv4 { get; set; } = true;

    /// <summary>
    /// If true, then the application will use the IPv6 transport protocol
    /// </summary>
    public bool EnableIPv6 { get; set; } = true;

    /// <summary>
    /// The default will be the last selected IP address for the IPv4 network if it’s available, or the
    /// first IP address in the list of available IP addresses for the IPv4 network.
    /// </summary>
    public string IPv4Address { get; set; } = string.Empty;

    /// <summary>
    /// The default will be the last selected IP address for the IPv6 network if it’s available, or the
    /// first IP address in the list of available IP addresses for the IPv6 network.
    /// </summary>
    public string IPv6Address { get; set; } = string.Empty;

    /// <summary>
    /// SIP port for UDP and TCP.
    /// </summary>
    [Required]
    [Range(1000, ushort.MaxValue)]
    public int LocalSipPortNumber { get; set; } = 5060;

    /// <summary>
    /// SIP port for Transport Layer Security (TLS).
    /// </summary>
    [Required]
    [Range(1000, ushort.MaxValue)]
    public int LocalSipsPortNumber { get; set; } = 5061;

    /// <summary>
    /// Contains the last "To" SIP URI entered by the user. The To SIP URI is used to call the remote party.
    /// </summary>
    [Required(ErrorMessage = "The SIP To URI is required and must be a valid SIP URI")]
    public string SipToUri { get; set; } = string.Empty;

    /// <summary>
    /// This setting is applied if the application needs to do a DNS host name lookup for the remote host and the DNS
    /// server returns an IPv6 address for the host.
    /// </summary>
    public bool PreferIPv6 { get; set; } = false;

    /// <summary>
    /// Specifies the audio codec to offer for each call.
    /// Must be one of "PCMU", "PCMA", "G722", "G729" or "AMR-WB".
    /// </summary>
    public string AudioCodec {  get; set; } = "PCMU";

    /// <summary>
    /// Specifies the type of encryption to offer for RTP type media (audio, video, RTT).
    /// Allowed values are "None", "SDES-SRTP" and "DTLS-SRTP".
    /// </summary>
    public string RtpEncryption { get; set; } = "None";

    /// <summary>
    /// First UDP port number to use for audio media
    /// </summary>
    [Required(ErrorMessage = "The Audio Port is required")]
    [Range(6000, ushort.MaxValue)]
    public int StartingAudioPortNumber { get; set; } = 6000;

    /// <summary>
    /// Number of UDP ports to reserve for audio media
    /// </summary>
    [Required(ErrorMessage = "The Number of Ports is required")]
    [Range(1, 10000)]
    public int NumPorts { get; set; } = 10000;

    /// <summary>
    /// If true, then a simulated location will be sent by-value with the call's INVITE request.
    /// </summary>
    public bool SendLocation { get; set; } = false;

    /// <summary>
    /// Specifies the starting latitude in decimal degrees.
    /// The minimum value is -90 and the maximum value is 90 degrees.
    /// </summary>
    [Required(ErrorMessage = "Starting Latitude in decimal degrees is required")]
    [Range(-90, 90)]
    public double StartingLatitudeDegrees { get; set; } = 34.24803;

    /// <summary>
    /// Specifies the starting longitude in decimal degrees.
    /// The minimum value is -180 and the maximum value is 180 degrees.
    /// </summary>
    [Required(ErrorMessage = "Starting Longitude in decimal degrees is required")]
    [Range(-180, 180)]
    public double StartingLongitudeDegrees { get; set; } = -118.80304;

    /// <summary>
    /// Specifies the delta to use for latitude. All latitudes for the caller location will be between
    /// the StartingLatitudeDegrees value and this value plus the StartingLatitudeDegrees value.
    /// The minimum value is 0 and the maximum value is limited to 10 degrees.
    /// </summary>
    [Required(ErrorMessage = "Delta Latitude in decimal degrees is required")]
    [Range(0, 10.0)]
    
    public double DeltaLatitudeDegrees { get; set; } = .1;

    /// <summary>
    /// Specifies the delta to use for longitude. All longitudes for the caller location will be between
    /// the StartingLongitudeDegrees value and this value plus the StartingLongitudeDegrees.
    /// The minimum value is 0 and the maximum value is 10 degrees.
    /// </summary>
    [Required(ErrorMessage = "Delta Longitude in decimal degrees is required")]
    [Range(0, 10)]
    public double DeltaLongitudeDegrees { get; set; } = .1;

    /// <summary>
    /// If true then use urn:service:sos for the request URI. Else, use the SIP To URI for the request URI.
    /// </summary>
    public bool UseUrnServiceSos { get; set; } = false;

    /// <summary>
    /// Constructor
    /// </summary>
    public CallLoadTesterSettings()
    {
        Random random = new Random();
        double min = StartingLatitudeDegrees - DeltaLatitudeDegrees;
        double max = StartingLatitudeDegrees + DeltaLatitudeDegrees;

        // Generates a double between within a range
        double randomLatitude = min + (random.NextDouble() * (max - min));
    }

    private const string SettingsFileName = $"{Program.AppName}.json";

    /// <summary>
    /// Gets the saved configuration settings if they exist or the default settings if they do not.
    /// </summary>
    /// <returns>Returns the configuration settings</returns>
    public static CallLoadTesterSettings GetSettings()
    {
        CallLoadTesterSettings? settings = new CallLoadTesterSettings();
        string MyDocmentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string SettingsDirectory = @$"{MyDocmentsDir}/{Program.AppName}";
        string SettingsFilePath = Path.Combine(SettingsDirectory, SettingsFileName);

        if (File.Exists(SettingsFilePath) == true)
        {
            string strSettings = File.ReadAllText(SettingsFilePath);
            settings = JsonHelper.DeserializeFromString<CallLoadTesterSettings>(strSettings);
            if (settings == null)
            {   // An error occurred, use the default settings
                settings = new CallLoadTesterSettings();
            }
        }

        return settings;
    }

    /// <summary>
    /// Saves the configuration settings to a file called CallLoadTester.json in 
    /// \Users\UserName\OneDrive\Documents\CallLoadTester\ directory so each user has their own settings.
    /// </summary>
    /// <param name="settings">Contains the settings to save.</param>
    public static void SaveSettings(CallLoadTesterSettings settings)
    {
        string MyDocmentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string SettingsDirectory = @$"{MyDocmentsDir}/{Program.AppName}";
        string SettingsFilePath = Path.Combine(SettingsDirectory, SettingsFileName);
        try
        {
            if (Directory.Exists(SettingsDirectory) == false)
            {
                DirectoryInfo dirInfo = Directory.CreateDirectory(SettingsDirectory);
                if (dirInfo.Exists == false)
                {
                    SipLogger.LogError($"Unable to create the settings directory: {SettingsDirectory}. " +
                        "Settings not saved.");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            SipLogger.LogError(ex, $"Unable to create the settings directory: {SettingsDirectory}. " +
                "Settings not saved.");
            return;
        }

        string strSettings = JsonHelper.SerializeToString(settings);
        if (string.IsNullOrEmpty(strSettings) == true)
        {
            SipLogger.LogError("Unable to serialize the settings. Settings not saved");
            return;
        }

        try
        {
            File.WriteAllText(SettingsFilePath, strSettings);
        }
        catch (Exception Ex)
        {
            SipLogger.LogError(Ex, $"Unable to write the settings file: {SettingsFileName}");
        }
    }

    /// <summary>
    /// Validates the settings. This method assumes basic validation to check for required properties and
    /// valid parameter ranges has already been performed. 
    /// </summary>
    /// <param name="settings">The settings to validate.</param>
    /// <returns>Returns null if no errors are detected. Returns a text message to display for the first
    /// error that is detected.</returns>
    public static string? ValidateSettings(CallLoadTesterSettings settings)
    {
        string? ErrorMessage = null;

        if (string.IsNullOrEmpty(settings.SipToUri) == true || SIPURI.TryParse(settings.SipToUri,
            out _) == false)
            return "The SIP URI is required and must be a valid SIP URI";

        double LatitudeMax = settings.StartingLatitudeDegrees + settings.DeltaLatitudeDegrees;
        if (LatitudeMax > 90 || LatitudeMax < -90)
            return "The Starting Latitude + Delta Latitude is out of range";

        double LongitudeMax = settings.StartingLongitudeDegrees + settings.DeltaLongitudeDegrees;
        if (LongitudeMax > 180 || LongitudeMax < -180)
            return "The Starting Longitude + Delta Longitude is out of range";

        if (settings.EnableIPv4 == false && settings.EnableIPv6 == false)
            return "Both IPv4 and IPv6 are disabled. At least one must be enabled";

        if (settings.EnableIPv4 == true)
        {
            IPAddress? IPv4Address = null;
            if (IPAddress.TryParse(settings.IPv4Address, out IPv4Address) == false)
                return "IPv4 is enabled but no valid IPv4 address is specified";

            // Make sure that the specified IPv4 address is available on this computer
            if (IPAddressIsAvailable(IPv4Address, IpUtils.GetIPv4Addresses()) == false)
                return $"The specified IPv4 address: '{IPv4Address}' is not available.";
        }

        if (settings.EnableIPv6 == true)
        {
            IPAddress? IPv6Address = null;
            if (IPAddress.TryParse(settings.IPv6Address, out IPv6Address) == false)
                return "IPv6 is enabled but no valid IPv6 address is specified.";

            // Make sure that the specified IPv6 address is available on this computer
            if (IPAddressIsAvailable(IPv6Address, IpUtils.GetIPv6Addresses()) == false)
                return $"The specified IPv6 address: '{IPv6Address}' is not available.";
        }

        // Make sure that the SIP and the SIPS ports are outside the audio port range.
        int MaxAudioPort = settings.StartingAudioPortNumber + settings.NumPorts - 1;
        if (PortInRange(settings.LocalSipPortNumber, settings.StartingAudioPortNumber, MaxAudioPort) == true)
            return "The Local SIP Port is within the audio port range.";

        if (PortInRange(settings.LocalSipsPortNumber, settings.StartingAudioPortNumber, MaxAudioPort) == true)
            return "The Local SIPS Port is within the audio port range.";

        if (settings.NumPorts / 2 < settings.SimultaneousCalls)
            return "Number of Ports must be at least twice the Simultaneous Calls setting";

        return ErrorMessage;
    }

    private static bool PortInRange(int port, int min, int max)
    {
        if (port >= min && port <= max)
            return true;
        else
            return false;
    }

    private static bool IPAddressIsAvailable(IPAddress address, List<IPAddress> addresses)
    {
        foreach (IPAddress addr in addresses)
        {
            if (address.Equals(addr) == true)
                return true;
        }

        return false;
    }

}
