/////////////////////////////////////////////////////////////////////////////////////
//  File:   CallManagerService.cs                                   1 Jun 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace CallLoadTester;

using NAudio.Wave;
using Ng911Lib.Utilities;
using Pidf;
using SipLib.Body;
using SipLib.Channels;
using SipLib.Core;
using SipLib.Logging;
using SipLib.Media;
using SipLib.Network;
using SipLib.Rtp;
using SipLib.Sdp;
using SipLib.Threading;
using SipLib.Transactions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

/// <summary>
/// Delegate type for the ErrorDetected event of the CallManagerService.
/// </summary>
/// <param name="errorMessage"></param>
public delegate void ErrorDetectedDelegate(string errorMessage);

/// <summary>
/// Delegate type for the UpdateCallStatistics event of the CallManagerService
/// </summary>
/// <param name="callStatistics"></param>
public delegate void UpdateCallStatisticsDelegate(CallStatistics callStatistics);

/// <summary>
/// This class manages all calls to the system under test.
/// </summary>
public class CallManagerService : IDisposable
{
    private const string DEFAULT_AUDIO_RECORDING_FILE = "DefaultAudioRecording.wav";
    private AudioSampleData m_AudioRecordingSampleData;

    private CallLoadTesterSettings m_Settings = new CallLoadTesterSettings();

    private SdpOfferSettings m_SdpOfferSettings = new SdpOfferSettings();

    /// <summary>
    /// This event is fired if the CallManagerService detects an error.
    /// </summary>
    public event ErrorDetectedDelegate? ErrorDetected = null;

    /// <summary>
    /// This event is fired every 1 second to update the current call statistics
    /// </summary>
    public event UpdateCallStatisticsDelegate? UpdateCallStatistics = null;

    private X509Certificate2 m_Certificate;

    // Create with the default port settings
    private MediaPortListManager m_PortManager = new MediaPortListManager(new MediaPortSettings());

    /// <summary>
    /// Gets or sets the state of the CallManagerService
    /// </summary>
    public CallManagerStateEnum CallManagerState { get; private set; } = CallManagerStateEnum.Idle;

    /// <summary>
    /// The key is the Call-ID.
    /// </summary>
    private Dictionary<string, OutgoingCall> m_Calls = new Dictionary<string, OutgoingCall>();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="certificate">X.509 certificate to use for SIPS</param>
    public CallManagerService(X509Certificate2 certificate)
    {
        m_Certificate = certificate;
        m_AudioRecordingSampleData = ReadWaveFile(DEFAULT_AUDIO_RECORDING_FILE);
    }

    private bool m_IsDisposed = false;

    /// <summary>
    /// Not used
    /// </summary>
    public void Dispose()
    {
        if (m_IsDisposed == false)
            return;

        // TODO: Do what needs to be done here
    }

    private SIPURI m_RemoteSipUri = new SIPURI(SIPSchemesEnum.sip, IPAddress.Any, 5060);
    private IPEndPoint m_RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 5060);

    /// <summary>
    /// Starts the call generation service.
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    public async Task StartService(CallLoadTesterSettings settings)
    {
        if (CallManagerState == CallManagerStateEnum.Running)
        {
            ErrorDetected?.Invoke("The Call Manager Service is already running.");
            return;
        }

        string? error = CallLoadTesterSettings.ValidateSettings(settings);
        if (error != null)
        {
            ErrorDetected?.Invoke(error);
            return;
        }

        m_GeneratingCalls = false;
        m_Settings = settings;
        m_CallStatistics = new CallStatistics();
        m_Calls.Clear();

        SipChannelSettings sipChannelSettings = new SipChannelSettings();
        sipChannelSettings.LocalUser = "CallLoadTester";
        if (m_Settings.EnableIPv4 == true)
            sipChannelSettings.LocalIPv4Address = IPAddress.Parse(m_Settings.IPv4Address);

        if (m_Settings.EnableIPv6 == true)
            sipChannelSettings.LocalIPv6Address = IPAddress.Parse(m_Settings.IPv6Address);

        sipChannelSettings.LocalSipPort = m_Settings.LocalSipPortNumber;
        sipChannelSettings.LocalSipsPort = m_Settings.LocalSipsPortNumber;
        // Using the defaults for all other SipChannelSettings

        SIPURI? SipToUri = SIPURI.ParseSIPURI(m_Settings.SipToUri);
        if (SipToUri is null)
        {
            ErrorDetected?.Invoke("Failed to parse the SIP To URI");
            return;
        }

        SIPURI? resolvedSipUri = null;
        try
        {
            resolvedSipUri = await SipDnsClient.ResolveSipServerAsync(SipToUri, true,
                m_Settings.PreferIPv6, false);

        }
        catch { }

        if (resolvedSipUri is null)
        {
            ErrorDetected?.Invoke($"Unable to resolve the SIP URI: {SipToUri.ToSIPEndPoint()}");
            return;
        }

        IPAddress resolvedIpAddress = resolvedSipUri.ToSIPEndPoint()!.GetIPEndPoint().Address;
        if (m_Settings.IPv4Address == null && resolvedIpAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            ErrorDetected?.Invoke("Cannot reach the SIP server because the SIP server's IP address is IPv4 but this PC does not have an IPv4 address");
            return;
        }

        if (m_Settings.IPv6Address == null && resolvedIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            ErrorDetected?.Invoke("Cannot reach the SIP server because the SIP server's IP address is IPv6 but this PC does not have an IPv6 address");
            return;
        }

        m_RemoteSipUri = resolvedSipUri;
        m_RemoteIpEndPoint = resolvedSipUri!.ToSIPEndPoint()!.GetIPEndPoint();

        try
        {
            m_SipTransport = SipTransport.CreateFromRemoteSipUri(resolvedSipUri, sipChannelSettings, m_Certificate);
        }
        catch (Exception ex)
        {
            ErrorDetected?.Invoke($"Failed to create the SipTransport. Reason = {ex.Message}");
            return;
        }

        m_LocalIpEndPoint = m_SipTransport.SipChannel.SIPChannelEndPoint.GetIPEndPoint();
        m_LocalIpAddress = m_LocalIpEndPoint.Address;
        m_SipTransport.SipRequestReceived += OnSipRequestReceived;
        m_SipTransport.Start();

        MediaPortSettings Mps = new MediaPortSettings();
        Mps.AudioPorts = new PortRange()
        {
            StartPort = m_Settings.StartingAudioPortNumber,
            Count = m_Settings.NumPorts
        };
        // Note: This application only supports audio so it is leaving the port ranges for video, RTT and 
        // MSRP at the default port ranges. If support for these media type is required in the future then
        // the port ranges for these media types must be initialized.

        m_PortManager = new MediaPortListManager(Mps);
        m_SdpOfferSettings = new SdpOfferSettings(new List<string> { m_Settings.AudioCodec },
            new List<string> { "H264" }, "CallLoadTester", RtpChannel.CertificateFingerprint!, m_PortManager);
        switch (m_Settings.RtpEncryption)
        {
            case "SDES-SRTP":
                m_SdpOfferSettings.RtpEncryptionType = RtpEncryptionEnum.SdesSrtp;
                break;
            case "DTLS-SRTP":
                m_SdpOfferSettings.RtpEncryptionType = RtpEncryptionEnum.DtlsSrtp;
                break;
        }

        CallManagerState = CallManagerStateEnum.Running;
        m_Thread = new Thread(ThreadLoop);
        m_IsRunning = true;
        m_Thread.IsBackground = true;
        m_Thread.Priority = ThreadPriority.Highest;
        //Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
        m_Thread.Start();
    }

    /// <summary>
    /// Ends all calls that are on-line or pending a response from the called party.
    /// </summary>
    public void EndCalls()
    {
        EnqueueWork(() =>
        {
            List<OutgoingCall> activeCalls = new List<OutgoingCall>();
            foreach (OutgoingCall call in m_Calls.Values)
            {
                if (call.CallState == OutgoingCallState.Calling || call.CallState == OutgoingCallState.Ringing ||
                    call.CallState == OutgoingCallState.OnLine)
                    activeCalls.Add(call);
            }

            if (activeCalls.Count == 0)
            {
                ErrorDetected?.Invoke("There are no active calls to end.");
                return;
            }

            foreach (OutgoingCall call in activeCalls)
            {
                if (call.CallState == OutgoingCallState.Calling)
                {   // No response has been received yet.
                    if (call.clientInviteTransaction != null)
                        call.clientInviteTransaction.CancelInvite();
                    else
                        SipLogger.LogError("Unable to cancel an INVITE request because the clientInviteTransaction is null");

                    m_CallStatistics.Canceled += 1;
                }
                else if (call.CallState == OutgoingCallState.Ringing)
                {
                    SIPRequest cancel = SipUtils.BuildCancelRequest(call.InviteRequest!, m_SipTransport!.SipChannel,
                        m_RemoteIpEndPoint, call.LastCSeqNum);
                    // Fire and forget.
                    m_SipTransport.StartClientNonInviteTransaction(cancel, m_RemoteIpEndPoint, null, 1000);
                    m_CallStatistics.Canceled += 1;
                }
                else
                {   // Its on-line so send a BYE
                    if (call.RemoteContactHeader != null && call.RemoteContactHeader.ContactURI is not null)
                    {
                        IPEndPoint remoteIpe = call.RemoteContactHeader.ContactURI.ToSIPEndPoint()!.GetIPEndPoint();
                        SIPRequest bye = SipUtils.BuildByeRequest(call.InviteRequest!, m_SipTransport!.SipChannel,
                            remoteIpe, false, call.LastCSeqNum, call.OKResponse!);
                        // Fire and forget
                        m_SipTransport.StartClientNonInviteTransaction(bye, remoteIpe, null, 1000);
                    }
                    else
                        SipLogger.LogError("Unable to terminate a call because the remote Contact header is not valid");
                }

                call.EndCall();
                if (call.CallState == OutgoingCallState.OnLine)
                    // Keep calls that were on-line but ended
                    call.CallState = OutgoingCallState.Ended;
                else
                    m_Calls.Remove(call.CallID);
            } // end foreach

        });
    }

    private volatile bool m_GeneratingCalls = false;

    private Thread? m_Thread = null;
    private volatile bool m_IsRunning = false;
    private ConcurrentQueue<Action> m_WorkQueue = new ConcurrentQueue<Action>();
    private SemaphoreSlim m_Semaphore = new SemaphoreSlim(0, int.MaxValue);

    private CallStatistics m_CallStatistics = new CallStatistics();
    private DateTime m_LastUpdate = DateTime.Now;

    private IPEndPoint m_LocalIpEndPoint = new IPEndPoint(IPAddress.Any, 5060);
    private IPAddress m_LocalIpAddress = IPAddress.Any;
    private DateTime m_LastCallRequestSent = DateTime.Now;

    private long m_FromUser = 0;

    /// <summary>
    /// Gets or sets the flag that determines whether or not this service's thread is generating calls
    /// or not. If true, then the thread is generating calls. This allows the user interface to pause and restart
    /// call generation.
    /// </summary>
    public bool GeneratingCalls
    {
        get { return m_GeneratingCalls; }
        set { m_GeneratingCalls = value; }
    }

    private const int CALL_STATISTICS_UPDATE_INTERVAL_MS = 1000;

    /// <summary>
    /// This is the service's main thread loop that generates calls.
    /// </summary>
    private void ThreadLoop()
    {
        m_LastCallRequestSent = DateTime.Now - TimeSpan.FromMilliseconds(m_Settings.CallDurationSeconds);
        m_FromUser = m_Settings.StartingFromNumber;

        while (m_IsRunning == true)
        {
            // This thread runs continuously as fast as it can in order to minimize the delay and latency
            // in generating calls and processing call responses.

            while (m_IsRunning == true && m_WorkQueue.TryDequeue(out Action? action) == true)
            {
                if (action != null)
                {
                    try
                    {
                        action();
                    }
                    catch (Exception actionException)
                    {
                        SipLogger.LogError(actionException, "Action Exception in ThreadLoop()");
                    }
                }
            } // end while

            DateTime Now = DateTime.Now;
            if (m_GeneratingCalls == true)
                DoGenerateCalls(Now);

            CheckForCallTermination();

            if ((Now - m_LastUpdate).TotalMilliseconds >= CALL_STATISTICS_UPDATE_INTERVAL_MS)
            {
                UpdateStatistics();
                UpdateCallStatistics?.Invoke(m_CallStatistics);
                m_LastUpdate = Now;
            }

        } // end while

        m_WorkQueue.Clear();
    }

    private void UpdateStatistics()
    {
        CallStatistics newStats = new CallStatistics();
        newStats.TotalCalls = m_CallStatistics.TotalCalls;
        newStats.Rejected = m_CallStatistics.Rejected;
        newStats.Failed = m_CallStatistics.Failed;
        newStats.Canceled = m_CallStatistics.Canceled;

        foreach (OutgoingCall call in m_Calls.Values)
        {
            if (call.CallState == OutgoingCallState.Ended)
                newStats.Completed += 1;

            if (call.CallState == OutgoingCallState.OnLine)
                newStats.OnLine += 1;

            if (call.CallState == OutgoingCallState.Trying || call.CallState == OutgoingCallState.Ringing)
                newStats.Ringing += 1;

            if (call.CallState == OutgoingCallState.Calling)
                newStats.Calling += 1;
        }

        m_CallStatistics = newStats;
    }

    private void DoGenerateCalls(DateTime Now)
    {
        if (m_CallStatistics.TotalCalls >= m_Settings.TotalCalls)
        {
            // Finished generating calls.
            return;
        }

        if (SimultaneousCalls() >= m_Settings.SimultaneousCalls)
            return;         // Must wait for some calls to end

        if (m_Settings.CallIntervalMs == 0)
            // Generate calls as quickly as possible
            StartNewCall();
        else
        {   // Check to see if its time to send another call request
            if ((Now - m_LastCallRequestSent).TotalMilliseconds >= m_Settings.CallIntervalMs)
            {
                StartNewCall();
                m_LastCallRequestSent = Now;
            }
        }
    }

    private int SimultaneousCalls()
    {
        int simultaneousCalls = 0;
        foreach (OutgoingCall call in m_Calls.Values)
        {
            if (call.CallState == OutgoingCallState.OnLine || call.CallState == OutgoingCallState.Calling ||
                call.CallState == OutgoingCallState.Ringing)
                simultaneousCalls += 1;
        }

        return simultaneousCalls;
    }

    private void StartNewCall()
    {
        int FreeAudioPorts = m_PortManager.GetFreePortCount(MediaTypes.Audio);
        if (FreeAudioPorts < 1)
        {   // Insufficient audio ports to start a new call.
            SipLogger.LogError("Insufficient audio ports. Cannot start a new call");
            m_GeneratingCalls = false;
            ErrorDetected?.Invoke("Insufficient audio ports. Cannot start new calls");
            return;
        }    

        SIPChannel sipChannel = m_SipTransport!.SipChannel;
        SIPURI fromSipUri = sipChannel.SIPChannelContactURI.CopyOf();
        fromSipUri.User = m_FromUser.ToString();

        SIPURI requestUri;
        if (m_Settings.UseUrnServiceSos == true)
            requestUri = SIPURI.ParseSIPURI("urn:service:sos");
        else
            requestUri = m_RemoteSipUri;

        SIPRequest invite = SIPRequest.CreateRequest(SIPMethodsEnum.INVITE, requestUri,
            m_RemoteSipUri, m_RemoteSipUri.User, fromSipUri, m_FromUser.ToString(),
            m_SipTransport.SipChannel.SIPChannelContactURI);

        // Add the Call-Info headers for the NG9-1-1 emergency-CallId and emergency-IncidentId
        SipUtils.AddEmergencyCallInfoHeaders(invite, $"osp.{Program.AppName}");

        Sdp offerSdp = Sdp.BuildOfferSdp(m_LocalIpAddress, m_SdpOfferSettings, m_Certificate);
        SipBodyBuilder bodyBuilder = new SipBodyBuilder();
        bodyBuilder.AddContent(SipLib.Body.ContentTypes.Sdp, offerSdp.ToString(), null, null);

        if (m_Settings.SendLocation == true)
        {   // Send location by-value
            string contentId = $"{m_FromUser}@{m_LocalIpEndPoint.ToString()}";
            SIPURI cidUri = SIPURI.ParseSIPURI("cid:" + contentId);
            SIPGeolocationHeader geoHeader = new SIPGeolocationHeader(cidUri);
            invite.Header.Geolocation.Add(geoHeader);
            Presence presence = GetNextPresence();
            bodyBuilder.AddContent(SipLib.Body.ContentTypes.Pidf, XmlHelper.SerializePidfToString(presence),
                contentId, null);
        }

        bodyBuilder.AttachMessageBody(invite);

        OutgoingCall call = new OutgoingCall(invite, offerSdp, new CallPortAllocations(m_PortManager));
        m_Calls.Add(call.CallID, call);
        call.CallState = OutgoingCallState.Calling;
        call.clientInviteTransaction = m_SipTransport.StartClientInvite(invite, m_RemoteIpEndPoint, OnClientInviteRequestComplete,
            OnClientInviteResponseReceived);

        m_CallStatistics.TotalCalls += 1;

        m_FromUser += 1;
    }

    private OutgoingCall? GetCall(string callID)
    {
        OutgoingCall? call = null;
        if (m_Calls.TryGetValue(callID, out call) == false)
            return null;

        return call;
    }

    private void OnClientInviteResponseReceived(SIPResponse Response, IPEndPoint RemoteEndPoint,
        SipTransactionBase Transaction)
    {
        EnqueueWork(() =>
        {
            OutgoingCall? call = GetCall(Response.Header.CallId);
            if (call == null)
                return;

            DateTime Now = DateTime.Now;
            if (Response.StatusCode >= 100 && Response.StatusCode < 200)
            {
                if (Response.Status == SIPResponseStatusCodesEnum.Trying)
                    call.CallState = OutgoingCallState.Trying;
                else if (Response.Status == SIPResponseStatusCodesEnum.Ringing || Response.Status == SIPResponseStatusCodesEnum.SessionProgress)
                {
                    call.CallState = OutgoingCallState.Ringing;
                    if (call.RingStartTime == DateTime.MinValue)
                        call.RingStartTime = Now;
                }

                if (call.InitialResponseTime == DateTime.MinValue)
                    call.InitialResponseTime = Now;
            }
        });
    }

    private void OnClientInviteRequestComplete(SIPRequest sipRequest, SIPResponse? sipResponse, IPEndPoint remoteEndPoint,
        SipTransport sipTransport, SipTransactionBase Transaction)
    {
        EnqueueWork(() =>
        {
            OutgoingCall? call = GetCall(sipRequest.Header.CallId);
            if (call == null)
                return;

            call.clientInviteTransaction = null;

            if (sipResponse == null)
            {
                m_CallStatistics.Failed += 1;
                call.EndCall();
                m_Calls.Remove(call.CallID);
                return;
            }

            if (sipResponse.StatusCode >= 400)
            {   // The call was rejected
                m_CallStatistics.Rejected += 1;
                call.EndCall();
                m_Calls.Remove(call.CallID);
            }
            else if (sipResponse.Status == SIPResponseStatusCodesEnum.Ok)
            {
                call.OKResponse = sipResponse;
                call.RemoteTag = sipResponse.Header.To?.ToTag;
                call.RemoteContactHeader = sipResponse.Header.Contact![0];
                SetCallOnLine(call, sipRequest, sipResponse, remoteEndPoint, sipTransport);
            }
        });
    }

    private void SetCallOnLine(OutgoingCall call, SIPRequest sipRequest, SIPResponse sipResponse, IPEndPoint remoteEndPoint,
        SipTransport sipTransport)
    {
        Sdp? answeredSdp = sipResponse.GetSdpContents();

        if (answeredSdp == null)
        {
            SipLogger.LogError("A call failed because the OK response from the server does not contain an SDP block.");
            call.EndCall();
            m_Calls.Remove(call.CallID);
            m_CallStatistics.Failed += 1;
            return;
        }

        MediaDescription? answeredAudioMd = answeredSdp.GetMediaType(MediaTypes.Audio);
        if (answeredAudioMd == null)
        {
            SipLogger.LogError("A call failed because the server did not provide an audio media description in the SDP of the OK response.");
            call.EndCall();
            m_Calls.Remove(call.CallID);
            m_CallStatistics.Failed += 1;
            return;
        }

        if (answeredAudioMd.Port == 0)
        {   // The called party rejected the offered audio media
            SipLogger.LogError("A call failed because the server rejected audio media.");
            call.EndCall();
            m_Calls.Remove(call.CallID);
            m_CallStatistics.Failed += 1;
            return;
        }

        call.AnsweredSdp = answeredSdp;
        (RtpChannel? rtpChannel, string? Error) = RtpChannel.CreateFromSdp(false, call.OfferedSdp,
            call.OfferedSdp.GetMediaType(MediaTypes.Audio)!, answeredSdp, answeredAudioMd, true, Program.AppName);
        if (rtpChannel == null)
        {
            SipLogger.LogError($"Failed to create a RtpChannel for a call. Reason = {Error}");
            call.EndCall();
            m_Calls.Remove(call.CallID);
            m_CallStatistics.Failed += 1;
            return;
        }

        call.rtpChannel = rtpChannel;

        DateTime Now = DateTime.Now;
        call.PickupTime = Now;
        // Its possible that the called party answered immediately with a 200 OK without sending an
        // intermediate response.
        if (call.InitialResponseTime == DateTime.MinValue)
            call.InitialResponseTime = Now;
        if (call.RingStartTime == DateTime.MinValue)
            call.RingStartTime = Now;

        rtpChannel.ReceiveStatisticsReady += call.OnRtpReceiveStaticsReady;

        IAudioEncoder? encoder = AudioMediaUtils.GetAudioEncoder(answeredAudioMd);
        if (encoder == null)
        {
            // This application only offers audio codecs that it knows how to handle. The system under
            // test answered with a codec that is not supported. This is a protocol violation and the
            // call cannot be handled so don't try to start media handling.
            SipLogger.LogError("The system under test answered the offered audio media with an unknown codec.");
            return;
        }

        IAudioDecoder? decoder = AudioMediaUtils.GetAudioDecoder(answeredAudioMd);
        if (decoder == null)
        {
            // This application only offers audio codecs that it knows how to handle. The system under
            // test answered with a codec that is not supported. This is a protocol violation and the
            // call cannot be handled so don't try to start media handling.
            SipLogger.LogError("The system under test answered the offered audio media with an unknown codec.");
            return;
        }

        // This application just uses the received audio RTP packets to calculate call quality statistics
        // so it does not attempt to send the received audio anywhere.

        call.audioSource = new AudioSource(answeredAudioMd, encoder, rtpChannel);
        FileAudioSource fileAudioSource = new FileAudioSource(m_AudioRecordingSampleData, null);
        fileAudioSource.Start();
        call.fileAudioSource = fileAudioSource;
        call.audioSource.SetAudioSampleSource(fileAudioSource);

        call.audioSource.Start();
        rtpChannel.StartListening();

        call.CallState = OutgoingCallState.OnLine;
    }


    private void OnSipRequestReceived(SIPRequest sipRequest, SIPEndPoint remoteEndPoint, SipTransport sipTransportManager)
    {
        EnqueueWork(() =>
        {
            OutgoingCall? call = GetCall(sipRequest.Header.CallId);
            IPEndPoint remoteIpe = remoteEndPoint.GetIPEndPoint();
            if (sipRequest.Method == SIPMethodsEnum.BYE)
            {
                if (call == null)
                {
                    SIPResponse response = SipUtils.BuildResponse(sipRequest, SIPResponseStatusCodesEnum.CallLegTransactionDoesNotExist,
                        "Dialog Does Not Exist", sipTransportManager.SipChannel, null);
                    sipTransportManager.StartServerNonInviteTransaction(sipRequest, remoteIpe, null, response);
                    return;
                }

                // Assume that the BYE is in-dialog
                SIPResponse okResponse = SipUtils.BuildOkToByeOrCancel(sipRequest, remoteEndPoint);
                // Just fire and forget.
                sipTransportManager.StartServerNonInviteTransaction(sipRequest, remoteEndPoint.GetIPEndPoint(),
                    null, okResponse);

                call.CallState = OutgoingCallState.Ended;
                call.EndCall();
            }
            else if (sipRequest.Method == SIPMethodsEnum.INVITE)
            {
                if (call == null || IsInDialog(sipRequest, call) == false)
                {
                    SIPResponse unknownDialog = SipUtils.BuildResponse(sipRequest, SIPResponseStatusCodesEnum.CallLegTransactionDoesNotExist,
                        "Dialog Does Not Exist", sipTransportManager.SipChannel, null);
                    sipTransportManager.StartServerInviteTransaction(sipRequest, remoteIpe, null, unknownDialog);
                    return;
                }

                // Handle a re-INVITE but only to change the destination of the audio or the
                // contact header. No need to support adding media to the call.
                call.RemoteContactHeader = sipRequest.Header.Contact![0];
                HandleReInvite(sipRequest, remoteIpe, sipTransportManager, call);
            }
            else if (sipRequest.Method == SIPMethodsEnum.ACK)
            {   // No action required
            }
            else
            {   // This application only supports a BYE request or a re-INVITE request.
                SIPResponse notAllowed = SipUtils.BuildResponse(sipRequest, SIPResponseStatusCodesEnum.MethodNotAllowed,
                    "Method Not Allowed", sipTransportManager.SipChannel, null);
                sipTransportManager.StartServerNonInviteTransaction(sipRequest, remoteIpe, null, notAllowed);
            }
        });
    }

    private void HandleReInvite(SIPRequest sipRequest, IPEndPoint remoteEndPoint, SipTransport sipTransportManager,
        OutgoingCall call)
    {
        Sdp? offeredSdp = sipRequest.GetSdpContents();
        if (offeredSdp == null)
        {   // Media must be offered in a re-INVITE request
            SIPResponse badRequest = SipUtils.BuildResponse(sipRequest, SIPResponseStatusCodesEnum.BadRequest,
                "Bad Request", sipTransportManager.SipChannel, null);
            sipTransportManager.StartServerInviteTransaction(sipRequest, remoteEndPoint, null, badRequest);
            return;
        }

        if (call.AnsweredSdp == null || call.AnsweredSdp.GetMediaType(MediaTypes.Audio) == null)
        {
            SipLogger.LogError("The system under test did not provide an audio media description in the OK response");
            return;
        }

        MediaDescription answeredAudioMd = call.AnsweredSdp.GetMediaType(MediaTypes.Audio)!;
        Sdp answerSdp = new Sdp(sipTransportManager.SipChannel.SIPChannelEndPoint.Address!, Program.AppName);
        MediaDescription offeredAudioMd = offeredSdp.GetMediaType(MediaTypes.Audio)!;

        foreach (MediaDescription offeredMd in offeredSdp.Media)
        {
            if (offeredMd.MediaType == MediaTypes.Audio)
            {
                if (MediaDescription.AreEqual(call.AnsweredSdp, answeredAudioMd, offeredSdp, offeredAudioMd) == true)
                {   // No changes to the media session are being offered. Answer with the last offered media description
                    answerSdp.Media.Add(call.OfferedSdp.GetMediaType(MediaTypes.Audio)!);
                }
                else
                {   // Changes to the audio media session are being offered.
                    // There are a number of things that the system under test could change in this case
                    // such as the codec type, media destination endpoint or encryption method.
                    
                }
            }
            else
            {   // A new media type has been offered. This application only supports audio so reject this
                // media type
                if (offeredMd.MediaType == MediaTypes.MSRP)
                    answerSdp.Media.Add(new MediaDescription(MediaTypes.MSRP, 0, new List<int>()));
                else
                    answerSdp.Media.Add(new MediaDescription(offeredMd.MediaType, 0, offeredMd.PayloadTypes));
            }
        }

        SIPResponse OkResponse = SipUtils.BuildOkToInvite(sipRequest, sipTransportManager.SipChannel, answerSdp.ToString(),
            SipLib.Body.ContentTypes.Sdp);
        OkResponse.Header.To!.ToTag = call.LocalTag;    // Fix the local tag
        sipTransportManager.StartServerInviteTransaction(sipRequest, remoteEndPoint, null, OkResponse);
    }

    private bool IsInDialog(SIPRequest sipRequest, OutgoingCall call)
    {
        if (SipUtils.IsInDialog(sipRequest) == false)
            return false;

        if (string.IsNullOrEmpty(call.RemoteTag) == true)
            return false;

        if (sipRequest.Header.To!.ToTag == call.LocalTag && sipRequest.Header.From!.FromTag == call.RemoteTag)
            return true;
        else
            return false;
    }

    private Random m_Random = new Random();

    private Presence GetNextPresence()
    {
        double LatitudeRandom = m_Random.NextDouble();
        double LongitudeRandom = m_Random.NextDouble();
        double NextLatitude = m_Settings.StartingLatitudeDegrees + LatitudeRandom * m_Settings.DeltaLatitudeDegrees;
        double NextLongitude = m_Settings.StartingLongitudeDegrees + LongitudeRandom * m_Settings.DeltaLongitudeDegrees;

        Presence presence = Presence.CreateTuplePresence(m_FromUser.ToString());
        presence.tuple.status.geopriv.LocationInfo.Point = new Point(NextLatitude, NextLongitude);

        return presence;
    }

    /// <summary>
    /// Check to see if its time to end any on-line calls
    /// </summary>
    private void CheckForCallTermination()
    {
        DateTime Now = DateTime.Now;
        foreach (OutgoingCall call in m_Calls.Values)
        {
            if (call.CallState == OutgoingCallState.OnLine)
            {
                if ((Now - call.PickupTime).TotalSeconds >= m_Settings.CallDurationSeconds)
                {
                    if (call.InviteRequest == null || m_SipTransport == null || call.RemoteContactHeader == null ||
                        call.RemoteContactHeader.ContactURI is null)
                        continue;

                    IPEndPoint remoteIpe = call.RemoteContactHeader.ContactURI.ToSIPEndPoint()!.GetIPEndPoint();
                    SIPRequest byeRequest = SipUtils.BuildByeRequest(call.InviteRequest, m_SipTransport.SipChannel,
                        remoteIpe, false, ++call.LastCSeqNum, call.OKResponse!);
                    // Fire and forget the transaction
                    m_SipTransport.StartClientNonInviteTransaction(byeRequest, remoteIpe, null, 1000);
                    call.EndCall();
                    call.CallState = OutgoingCallState.Ended;
                }
            }
        }
    }

    private void EnqueueWork(Action action)
    {
        m_WorkQueue.Enqueue(action);
        //m_Semaphore.Release();
    }


    private SipTransport? m_SipTransport = null;

    public void StopService()
    {
        if (CallManagerState != CallManagerStateEnum.Running)
        {

            return;
        }

        m_GeneratingCalls = false;
        m_IsRunning = false;
        if (m_Thread != null)
        {
            m_Thread.Join();
            m_Thread = null;
        }

        if (m_SipTransport != null)
        {
            m_SipTransport.SipRequestReceived -= OnSipRequestReceived;
            m_SipTransport.Shutdown();
            m_SipTransport = null;
        }

        m_Calls.Clear();
        CallManagerState = CallManagerStateEnum.Idle;
    }

    /// <summary>
    /// Reads the audio samples from a WAV file. The file format must be mono, 16 bits/sample linear (PCM)
    /// and the sample rate must be 8000 or 16000 samples per second.
    /// </summary>
    /// <param name="FilePath">Location of the file</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
    /// <exception cref="ArgumentException">Thrown if unable to read the file or if the file format is
    /// incorrect.</exception>
    public AudioSampleData ReadWaveFile(string FilePath)
    {
        short[] Samples;
        if (File.Exists(FilePath) == false)
        {
            throw new FileNotFoundException($"Wave file: '{FilePath}' not found");
        }

        WaveFileReader Wfr = new WaveFileReader(FilePath);
        WaveFormat waveFormat = Wfr.WaveFormat;
        if (waveFormat == null || waveFormat.Channels != 1 || (waveFormat.SampleRate != 8000 &&
            waveFormat.SampleRate != 16000) || waveFormat.BitsPerSample != 16)
        {
            throw new ArgumentException($"Invalid wave file format for file: '{FilePath}'");
        }

        byte[] buffer = new byte[Wfr.Length];
        Wfr.ReadExactly(buffer);
        Samples = new short[Wfr.SampleCount];

        MemoryStream memoryStream = new MemoryStream(buffer);
        BinaryReader binaryReader = new BinaryReader(memoryStream);
        for (long i = 0; i < Wfr.Length / 2; i++)
            Samples[i] = binaryReader.ReadInt16();

        binaryReader.Dispose();
        memoryStream.Dispose();
        return new AudioSampleData(Samples, waveFormat.SampleRate);
    }

    public CallStatisticsDetails GetCallStatisticsDetails(string callID)
    {
        CallStatisticsDetails details = new CallStatisticsDetails();
        ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        EnqueueWork(() => 
        {
            CalculateCallStatisticsDetails(callID, details);
            manualResetEvent.Set();
        });

        manualResetEvent.WaitOne();
        return details;
    }

    private void CalculateCallStatisticsDetails(string callID, CallStatisticsDetails details)
    {
        OutgoingCall? call = GetCall(callID);
        if (call == null)
        {
            details.ResultsAreValid = false;
            return;
        }

        details.ResultsAreValid = true;
        details.CallID = callID;
        details.CallStartTime = call.CallStartTime;
        RtpReceiveStatistics[] rxStats = call.ReceiveStatistics.ToArray();
        // Skip the first entry because it is sent immediately and does not contain any data.
        for (int i = 1; i < rxStats.Length; i++)
        {
            RtpReceiveStatistics temp = rxStats[i].Copy();
            temp.DroppedPackets = temp.PacketsExpected - temp.PacketsReceived;
            details.ReceiveStatistics.Add(temp);
        }
    }

    public CallQualityStatisticsSummary GetCallQualityStatistics()
    {
        CallQualityStatisticsSummary summary = new CallQualityStatisticsSummary();
        ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        
        EnqueueWork(() =>
        {
            CalculateQualityStatistics(summary);
            manualResetEvent.Set();
        });

        manualResetEvent.WaitOne();
        return summary;
    }

    private const double MAXIMUM_MOS = 4.5;     // The actual maximum MOS score is 4.4

    private void CalculateQualityStatistics(CallQualityStatisticsSummary summary)
    {
        summary.MinimumMos = MAXIMUM_MOS;

        int SumExpectedPackets = 0;

        foreach (OutgoingCall call in m_Calls.Values)
        {
            if (call.CallState == OutgoingCallState.Ended)
            {
                RtpReceiveStatistics[] rtpReceiveStatistics = call.ReceiveStatistics.ToArray();
                if (rtpReceiveStatistics.Length < 2)
                    continue;

                CallQualityStatistics callStats = new CallQualityStatistics();
                callStats.MinimumMos = MAXIMUM_MOS;
                callStats.CallID = call.CallID;
                callStats.CallStartTime = call.CallStartTime.ToString("HH:mm:ss.fffff");
                callStats.ResponseTimeMs = (int) (call.InitialResponseTime - call.CallStartTime).TotalMilliseconds;
                callStats.RingTimeMs = (int) (call.PickupTime - call.RingStartTime).TotalMilliseconds;
                callStats.CallDurationSeconds = (int) (call.CallEndTime - call.PickupTime).TotalSeconds;

                int MosCount = 0;
                int CallSumExpectedPackets = 0;

                // Skip the first entry because it is sent immediately and does not contain any packets
                for (int i = 1; i < rtpReceiveStatistics.Length; i++)
                { 
                    MosCount += 1;
                    if (rtpReceiveStatistics[i].Mos.MOS < callStats.MinimumMos)
                        callStats.MinimumMos = rtpReceiveStatistics[i].Mos.MOS;

                    callStats.AverageMos += rtpReceiveStatistics[i].Mos.MOS;
                    int PpJitterMs = rtpReceiveStatistics[i].InstantaneousJitter.Maximum - rtpReceiveStatistics[i].InstantaneousJitter.Minimum;
                    if (PpJitterMs > callStats.MaximumJitterMs)
                        callStats.MaximumJitterMs = PpJitterMs;

                    rtpReceiveStatistics[i].DroppedPackets = rtpReceiveStatistics[i].PacketsExpected -
                        rtpReceiveStatistics[i].PacketsReceived;

                    callStats.DroppedPackets += rtpReceiveStatistics[i].DroppedPackets;
                    CallSumExpectedPackets += rtpReceiveStatistics[i].PacketsExpected;
                    SumExpectedPackets += rtpReceiveStatistics[i].PacketsExpected;

                    callStats.OutOfOrderPackets += rtpReceiveStatistics[i].OutOfOrderPackets;
                    if (rtpReceiveStatistics[i].DelayInMilliseconds > callStats.MaximumDelayMs)
                        callStats.MaximumDelayMs = rtpReceiveStatistics[i].DelayInMilliseconds;
                }

                if (CallSumExpectedPackets > 0)
                    callStats.DroppedPacketsPercent = ((double)callStats.DroppedPackets * 100) / CallSumExpectedPackets;

                if (MosCount > 0)
                    callStats.AverageMos = callStats.AverageMos / MosCount;

                summary.CallStatistics.Add(callStats);

                if (callStats.MinimumMos >= 4.3)
                    summary.MosRangeCounts[CallQualityStatisticsSummary.ExcellentIndex].CallCount += 1;
                else if (callStats.MinimumMos >= 4.0 && callStats.MinimumMos < 4.3)
                    summary.MosRangeCounts[CallQualityStatisticsSummary.VeryGoodIndex].CallCount += 1;
                else if (callStats.MinimumMos >= 3.6 && callStats.MinimumMos < 4.0)
                    summary.MosRangeCounts[CallQualityStatisticsSummary.GoodIndex].CallCount += 1;
                else if (callStats.MinimumMos >= 3.1 && callStats.MinimumMos < 3.6)
                    summary.MosRangeCounts[CallQualityStatisticsSummary.FairIndex].CallCount += 1;
                else if (callStats.MinimumMos >= 2.6 && callStats.MinimumMos < 3.1)
                    summary.MosRangeCounts[CallQualityStatisticsSummary.PoorIndex].CallCount += 1;
                else if (callStats.MinimumMos < 2.6)
                    summary.MosRangeCounts[CallQualityStatisticsSummary.UnacceptableIndex].CallCount += 1;
            }
        }  // end foreach call

        // Calculate the summaries for all calls
        foreach (CallQualityStatistics callQualStats in summary.CallStatistics)
        {
            if (callQualStats.MinimumMos < summary.MinimumMos)
                summary.MinimumMos = callQualStats.MinimumMos;

            if (callQualStats.MaximumJitterMs > summary.MaximumJitterMs)
                summary.MaximumJitterMs = callQualStats.MaximumJitterMs;

            summary.DroppedPackets += callQualStats.DroppedPackets;
            summary.OutOfOrderPackets += callQualStats.OutOfOrderPackets;
            if (callQualStats.MaximumDelayMs > summary.MaximumDelayMs)
                summary.MaximumDelayMs = callQualStats.MaximumDelayMs;
        }

        if (SumExpectedPackets > 0)
            summary.DroppedPacketsPercent = ((double) summary.DroppedPackets * 100) / SumExpectedPackets;
    }

}

/// <summary>
/// Enumeration for the states of the CallManagerService
/// </summary>
public enum CallManagerStateEnum
{
    Idle,

    Running
}
