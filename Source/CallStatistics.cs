/////////////////////////////////////////////////////////////////////////////////////
//  File:   CallStatistics.cs                                       9 Jun 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace CallLoadTester;

/// <summary>
/// Class for passing the current call statistics for display in the Home page.
/// </summary>
public class CallStatistics
{
    /// <summary>
    /// Total number of call attempts.
    /// </summary>
    public int TotalCalls { get; set; }

    /// <summary>
    /// Number of calls that have been completed successfully.
    /// </summary>
    public int Completed { get; set; }

    /// <summary>
    /// Number of calls that are currently on-line.
    /// </summary>
    public int OnLine { get; set; }

    /// <summary>
    /// Number of calls that are currently ringing or trying.
    /// </summary>
    public int Ringing { get; set; }

    /// <summary>
    /// Number of calls that have been rejected.
    /// </summary>
    public int Rejected { get; set; }

    /// <summary>
    /// Number of calls attempts that have failed.
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// Number of calls for which an INVITE request has been sent but no response has been received yet.
    /// </summary>
    public int Calling { get; set; }

    /// <summary>
    /// Number of calls that have been canceled by the user of this application before they were answered
    /// by the remote endpoint.
    /// </summary>
    public int Canceled { get; set; }
}
