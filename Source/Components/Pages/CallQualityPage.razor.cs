/////////////////////////////////////////////////////////////////////////////////////
//  File:   CallQualityPage.cs                                      14 Jun 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace CallLoadTester.Components.Pages;

public partial class CallQualityPage
{
    private CallManagerService m_CallManager;
    private CallQualityStatisticsSummary m_Summary = new CallQualityStatisticsSummary();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="callManagerService">Injected CallManagerService. This service is a singleton for
    /// the application and is shared among all users of this page.</param>
    public CallQualityPage(CallManagerService callManagerService)
    {
        m_CallManager = callManagerService;
        if (m_CallManager.CallManagerState == CallManagerStateEnum.Running)
            m_Summary = m_CallManager.GetCallQualityStatistics();
    }

    public string GetMosTextBackground(double Mos)
    {
        string TextBg = "text-bg-success";
        if (Mos >= 4.3)
            TextBg = "text-bg-success";
        else if (Mos >= 4.0 && Mos < 4.3)
            TextBg = "text-bg-success";
        else if (Mos >= 3.6 && Mos < 4.0)
            TextBg = "text-bg-success";
        else if (Mos >= 3.1 && Mos < 3.6)
            TextBg = "text-bg-warning";
        else if (Mos >= 2.6 && Mos < 3.1)
            TextBg = "text-bg-warning";
        else if (Mos < 2.6)
            TextBg = "text-bg-danger";

        return TextBg;
    }

    protected override void OnInitialized()
    {
    }
}
