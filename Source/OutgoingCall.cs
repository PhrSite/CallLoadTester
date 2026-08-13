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
    public string LocalTag = string.Empty;
    public string? RemoteTag = null;
    public int LastCSeqNum = 0;

    public SIPRequest? InviteRequest = null;
    public SIPResponse? OKResponse = null;
    public string CallID = string.Empty;

    public SIPContactHeader? RemoteContactHeader = null;

    public ClientInviteTransaction? clientInviteTransaction = null;

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

    public RtpChannel? rtpChannel = null;

    public CallPortAllocations PortAllocations;

    public Sdp OfferedSdp;
    public Sdp? AnsweredSdp;

    public AudioSource? audioSource = null;
    public FileAudioSource? fileAudioSource = null;

    public ThreadSafeGenericList<RtpReceiveStatistics> ReceiveStatistics = new ThreadSafeGenericList<RtpReceiveStatistics>();

    public OutgoingCall(SIPRequest invite, Sdp offeredSdp, CallPortAllocations callPortAllocations)
    {
        InviteRequest = invite;
        OfferedSdp = offeredSdp;
        PortAllocations = callPortAllocations;
        LocalTag = invite.Header.From!.FromTag!;
        LastCSeqNum = invite.Header.CSeq;
        CallID = invite.Header.CallId;
        CallState = OutgoingCallState.Calling;

    }

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

        PortAllocations.FreeAllocatedPorts();

    }

    /// <summary>
    /// Event handler for the RtpReceiveStatisticsReady event that is fired by this call's RtpChannel.
    /// </summary>
    /// <param name="statistics">New RTP statistics.</param>
    /// <param name="rtpChannel">RtpChannel that fired this event.</param>
    public void OnRtpReceiveStaticsReady(RtpReceiveStatistics statistics, RtpChannel rtpChannel)
    {
        ReceiveStatistics.Add(statistics);

        // For debug only
        if (statistics.PacketsReceived > 0)
        {
            //Console.WriteLine($"Time = {DateTime.Now.ToString("HH:mm:ss.ffff")}, MOS = {statistics.Mos.MOS.ToString("F2")}, " +
            //    $"Delay ms = {statistics.DelayInMilliseconds} " +
            //    $"DroppedPackets = {statistics.DroppedPackets}, Min Jitter ms = {statistics.SmoothedJitter.Minimum}, " +
            //    $"Avg Jitter ms = {statistics.SmoothedJitter.Average}, Max Jitter ms = {statistics.SmoothedJitter.Maximum}");
        }
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

