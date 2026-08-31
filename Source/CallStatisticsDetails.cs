/////////////////////////////////////////////////////////////////////////////////////
//  File:   CallStatisticsDetails.cs                                21 Aug 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

using SipLib.Rtp;

namespace CallLoadTester;

public class CallStatisticsDetails
{
    public string CallID { get; set; } = string.Empty;

    public bool ResultsAreValid { get; set; } = false;

    public DateTime CallStartTime { get; set; } = DateTime.MinValue;


    public List<RtpReceiveStatistics> ReceiveStatistics = new List<RtpReceiveStatistics>();


}
