/////////////////////////////////////////////////////////////////////////////////////
//  File:   CallQualityStatisticsSummary.cs                         14 Jul 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace CallLoadTester;

/// <summary>
/// Model class for displaying the call quality statistics for completed calls.
/// </summary>
public class CallQualityStatisticsSummary
{

    /// <summary>
    /// Warn the user if the minimum MOS score is below this threshold.
    /// </summary>
    public const double MOS_WARNING_THRESHOLD = 3.6;

    /// <summary>
    /// Minimum Mean Opinion Score (MOS) calculated for all calls
    /// </summary>
    public double MinimumMos { get; set; } = 0.0;

    /// <summary>
    /// Maximum value of the smoothed packet jitter in milliseconds for all calls.
    /// </summary>
    public int MaximumJitterMs { get; set; } = 0;

    /// <summary>
    /// Total number of dropped packets detected for all calls.
    /// </summary>
    public int DroppedPackets { get; set; } = 0;

    /// <summary>
    /// Percentage of expected packets that were dropped for all calls.
    /// </summary>
    public double DroppedPacketsPercent { get; set; } = 0;

    /// <summary>
    /// Total number of out of order packets detected for all calls.
    /// </summary>
    public int OutOfOrderPackets { get; set; } = 0;

    /// <summary>
    /// Maximum network delay in milliseconds that was detected for all calls.
    /// </summary>
    public int MaximumDelayMs { get; set; } = 0;

    /// <summary>
    /// Contains the call quality statistics for all completed calls.
    /// </summary>
    public List<CallQualityStatistics> CallStatistics {  get; set; } = new List<CallQualityStatistics>();

    /// <summary>
    /// Index to the Excellent column in the MosRangeCounts list
    /// </summary>
    public const int ExcellentIndex = 0;
    /// <summary>
    /// Index to the Very Good column in the MosRangeCounts list
    /// </summary>
    public const int VeryGoodIndex = 1;
    /// <summary>
    /// Index to the Good column in the MosRangeCounts list
    /// </summary>
    public const int GoodIndex = 2;
    /// <summary>
    /// Index to the Fair column in the MosRangeCounts list
    /// </summary>
    public const int FairIndex = 3;
    /// <summary>
    /// Index to the Poor column in the MosRangeCounts list
    /// </summary>
    public const int PoorIndex = 4;
    /// <summary>
    /// Index to the Unaccetptable column in the MosRangeCounts list
    /// </summary>
    public const int UnacceptableIndex = 5;

    /// <summary>
    /// This table shows the number of calls in each range of MOS values.
    /// </summary>
    public List<MosRangeRowType> MosRangeCounts = new List<MosRangeRowType>()
    {
        new MosRangeRowType() { Quality = "Excellent", MosRange = "MOS >= 4.3", CallCount = 0, Class = "text-bg-success" },
        new MosRangeRowType() { Quality = "Very Good", MosRange = "4.0 =< MOS < 4.3", CallCount = 0 , Class = "text-bg-success"},
        new MosRangeRowType() { Quality = "Good", MosRange = "3.6 =< MOS < 4.0", CallCount = 0, Class = "text-bg-success" },
        new MosRangeRowType() { Quality = "Fair", MosRange = "3.1 =< MOS < 3.6", CallCount = 0, Class = "text-bg-warning" },
        new MosRangeRowType() { Quality = "Poor", MosRange = "2.6 =< MOS < 3.1", CallCount= 0, Class = "text-bg-warning" },
        new MosRangeRowType() { Quality = "Unacceptable", MosRange = "MOS < 2.6", CallCount = 0, Class = "text-bg-danger" }
    };
}

/// <summary>
/// Class for displaying the number of calls in each MOS range in the MosRangeCounts list of CallQualityStatisticsSummary class.
/// </summary>
public class MosRangeRowType
{
    /// <summary>
    /// Describes the audio quality (Excellent, Very Good, etc.) of the calls that have a MOS value in this range
    /// </summary>
    public string Quality { get; set; } = string.Empty;

    /// <summary>
    /// Text that describes the lower and upper range of MOS values
    /// </summary>
    public string MosRange {  get; set; }  = string.Empty;

    /// <summary>
    /// Number of calls that fall within this MOS value range
    /// </summary>
    public int CallCount { get; set; } = 0;

    /// <summary>
    /// Specifes the Cascading Style Sheet (CSS) class to use to specify the color of the CallCount value. Must
    /// be one of the Bootstrap 5 class macros for text background color such as "text-bg-success, "text-bg-warning, etc.
    /// </summary>
    public string Class { get; set; } = string.Empty;
}
