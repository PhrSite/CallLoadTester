/////////////////////////////////////////////////////////////////////////////////////
//  File:   CallDetails.razor.cs                                    21 Aug 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace CallLoadTester.Components.Pages;

public partial class CallDetails
{
    private CallManagerService m_CallManager;

    private CallStatisticsDetails m_CallStatisticsDetails = new CallStatisticsDetails();

    public CallDetails(CallManagerService callManagerService)
    {
        m_CallManager = callManagerService;
        string callID = CallID;

    }

    protected override void OnInitialized()
    {
        m_CallStatisticsDetails = m_CallManager.GetCallStatisticsDetails(CallID);

    }

}
