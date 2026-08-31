/////////////////////////////////////////////////////////////////////////////////////
//  File:   OutgoingCall.cs                                         11 Jun 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

using SipLib.Collections;
using SipLib.Core;
using SipLib.Media;
using SipLib.Rtp;
using SipLib.Sdp;
using SipLib.Transactions;

namespace CallLoadTester;

/// <summary>
/// This is a container class that contains all of the data about a single call.
/// </summary>
internal class OutgoingCall
{
    /// <summary>
    /// Local tag from the From header.
    /// </summary>
    public string LocalTag = string.Empty;

    /// <summary>
    /// Remote tag from the To header received in the OK response. Valid only if a 200 OK response has been
    /// received.
    /// </summary>
    public string? RemoteTag = null;

    /// <summary>
    /// Last CSeq header value that was sent. Not valid if 0.
    /// </summary>
    public int LastCSeqNum = 0;

    /// <summary>
    /// Invite request that was sent by this applicaiton.
    /// </summary>
    public SIPRequest? InviteRequest = null;

    /// <summary>
    /// OK response that was received by tye system under test.
    /// </summary>
    public SIPResponse? OKResponse = null;

    /// <summary>
    /// Value of the Call-ID header from the INVITE request.
    /// </summary>
    public string CallID = string.Empty;

    /// <summary>
    /// Contact header received from the system under test in the OK response.
    /// </summary>
    public SIPContactHeader? RemoteContactHeader = null;

    /// <summary>
    /// Set when the initial INVITE request is sent. Used to cancel the transaction for the INVITE if the
    /// user ends the call before the system under test answers the call. Set to null when the INVITE transaction
    /// is completed.
    /// </summary>
    public ClientInviteTransaction? clientInviteTransaction = null;

    /// <summary>
    /// Stores the current call state.
    /// </summary>
    public OutgoingCallState CallState = OutgoingCallState.Idle;

    /// <summary>
    /// Time that the initial INVITE was sent.
    /// </summary>
    public DateTime CallStartTime = DateTime.Now;

    /// <summary>
    /// Time that the remote endpoint picked up the call.
    /// </summary>
    public DateTime PickupTime = DateTime.MinValue;

    /// <summary>
    /// Time that the first initial response was received or the time that the 200 OK response was
    /// received if no initial response was received.
    /// </summary>
    public DateTime InitialResponseTime = DateTime.MinValue;
    
    /// <summary>
    /// Time that a 180 Rining or 183 Session Progress response was received.
    /// </summary>
    public DateTime RingStartTime = DateTime.MinValue;

    /// <summary>
    /// Time that the call ended.
    /// </summary>
    public DateTime CallEndTime = DateTime.Now;

    /// <summary>
    /// This is the RtpChannel used for sending and receiving audio data. It is set when the system under test
    /// answers the call.
    /// </summary>
    public RtpChannel? rtpChannel = null;

    /// <summary>
    /// This is used for storing the UDP ports used for audio media. This call object uses it to de-allocate
    /// the audio ports that it used when the call ends.
    /// </summary>
    private CallPortAllocations m_PortAllocations;


    /// <summary>
    /// Stores the Sdp object that was sent with the INVITE request.
    /// </summary>
    public Sdp OfferedSdp;

    /// <summary>
    /// Stores the Sdp object that was sent by the system under test in the OK response.
    /// </summary>
    public Sdp? AnsweredSdp;

    /// <summary>
    /// Stores the AudioSource being used by this call.
    /// </summary>
    public AudioSource? audioSource = null;

    /// <summary>
    /// Stores the FileAudioSource being used by this call to send audio samples from a WAVE file.
    /// </summary>
    public FileAudioSource? fileAudioSource = null;

    /// <summary>
    /// Stores the RTP statistics calculated by the RtpChannel for this call. Each item contains the call quality
    /// statistics for 5 seconds worth of received audio RTP packets.
    /// </summary>
    public ThreadSafeGenericList<RtpReceiveStatistics> ReceiveStatistics = new ThreadSafeGenericList<RtpReceiveStatistics>();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="invite">INVITE request that the CallManagerService built and will send to the system under test.</param>
    /// <param name="offeredSdp">Sdp object that was sent with the INIVTE request.</param>
    /// <param name="callPortAllocations">Used to allocate and de-allocate media UDP ports with a configured
    /// port range.</param>
    public OutgoingCall(SIPRequest invite, Sdp offeredSdp, CallPortAllocations callPortAllocations)
    {
        InviteRequest = invite;
        OfferedSdp = offeredSdp;

        m_PortAllocations = callPortAllocations;
        m_PortAllocations.AddAllocatedPortsFromSdp(offeredSdp);

        LocalTag = invite.Header.From!.FromTag!;
        LastCSeqNum = invite.Header.CSeq;
        CallID = invite.Header.CallId;
        CallState = OutgoingCallState.Calling;

    }

    /// <summary>
    /// Call when the call ends. Frees all resources used by this call object and de-allocates any
    /// UDP ports used for media.
    /// </summary>
    public void EndCall()
    {
        CallEndTime = DateTime.Now;
        if (rtpChannel != null)
        {
            rtpChannel.ReceiveStatisticsReady -= OnRtpReceiveStaticsReady;
            rtpChannel.Shutdown();
            rtpChannel = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource = null;
        }

        if (fileAudioSource != null)
        {
            fileAudioSource.Stop();
            fileAudioSource = null;
        }

        m_PortAllocations.FreeAllocatedPorts();
    }

    /// <summary>
    /// Event handler for the RtpReceiveStatisticsReady event that is fired by this call's RtpChannel.
    /// </summary>
    /// <param name="statistics">New RTP statistics.</param>
    /// <param name="rtpChannel">RtpChannel that fired this event.</param>
    public void OnRtpReceiveStaticsReady(RtpReceiveStatistics statistics, RtpChannel rtpChannel)
    {
        ReceiveStatistics.Add(statistics);
    }
}

internal enum OutgoingCallState
{
    Idle,
    /// <summary>
    /// An INVITE request has been sent but an interim response has not been received yet.
    /// </summary>
    Calling,
    /// <summary>
    /// A 100 Trying response has been received
    /// </summary>
    Trying,
    /// <summary>
    /// A 180 Ringing or a 183 response has been received.
    /// </summary>
    Ringing,
    /// <summary>
    /// The INVITE transaction failed
    /// </summary>
    Failed,
    /// <summary>
    /// The call was rejected by the remote endpoing
    /// </summary>
    Rejected,
    /// <summary>
    /// The call is currently on-line
    /// </summary>
    OnLine,
    /// <summary>
    /// The call was ended by this application or the remote endpoint
    /// </summary>
    Ended,
}

