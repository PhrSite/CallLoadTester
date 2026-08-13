/////////////////////////////////////////////////////////////////////////////////////
//  File:   CallQualityStatistics.cs                                14 Jul 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace CallLoadTester;

/// <summary>
/// Model class for displaying call quality statistics for a single call.
/// </summary>
public class CallQualityStatistics
{
    /// <summary>
    /// Call-ID header value of the call.
    /// </summary>
    public string CallID { get; set; } = string.Empty;

    /// <summary>
    /// Formatted call start time of day.
    /// </summary>
    public string CallStartTime { get; set; } = string.Empty;

    /// <summary>
    /// Time delay from the time that the INVITE was sent until the first intermediate or final response
    /// was received in milliseconds.
    /// </summary>
    public int ResponseTimeMs { get; set; } = 0;

    /// <summary>
    /// Interval in milliseconds between the Ringing response and the time that the call was answered.
    /// </summary>
    public int RingTimeMs { get; set; } = 0;

    /// <summary>
    /// Length of time in seconds that the call was on-line.
    /// </summary>
    public int CallDurationSeconds { get; set; } = 0;

    /// <summary>
    /// Minimum Mean Opinion Score (MOS) calculated during the call.
    /// </summary>
    public double MinimumMos { get; set;  } = 0.0;

    /// <summary>
    /// Average Mean Opinion Score (MOS) calculated during the call.
    /// </summary>
    public double AverageMos {  get; set; } = 0.0;

    /// <summary>
    /// Maximum value of the smoothed packet jitter in milliseconds detected during the call.
    /// </summary>
    public int MaximumJitterMs { get; set; } = 0;

    /// <summary>
    /// Total number of dropped packets detected during the call.
    /// </summary>
    public int DroppedPackets { get; set; } = 0;

    /// <summary>
    /// Total number of out of order packets detected during the call.
    /// </summary>
    public int OutOfOrderPackets { get; set; } = 0;

    /// <summary>
    /// Maximum network delay in milliseconds that was detected during the call.
    /// </summary>
    public int MaximumDelayMs { get; set; } = 0;

}
