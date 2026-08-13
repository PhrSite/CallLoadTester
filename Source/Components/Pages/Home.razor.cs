/////////////////////////////////////////////////////////////////////////////////////
//  File:   Home.razor.cs                                           8 Jun 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace CallLoadTester.Components.Pages;

/// <summary>
/// Model/Controller for the Home razor page of the CallLoadTester application.
/// </summary>
public partial class Home
{
    private CallManagerService m_CallManager;

    private CallStatistics CurrentStatistics { get; set; } = new CallStatistics();

    private string FailedClass { get; set; } = "bg-danger";

    private bool ShowError { get; set; } = false;
    private string ErrorMessage { get; set; } = string.Empty;

    private CallLoadTesterSettings m_Settings;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="callManagerService">Injected CallManagerService. This service is a singleton for
    /// the application and is shared among all users of this page.</param>
    public Home(CallManagerService callManagerService)
    {
        m_CallManager = callManagerService;
        m_Settings = CallLoadTesterSettings.GetSettings();
    }

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;

        m_CallManager.ErrorDetected += OnErrorDetected;
        m_CallManager.UpdateCallStatistics += OnUpdateStatistics;

        if (m_CallManager.CallManagerState == CallManagerStateEnum.Running)
        {
            StartButtonLabel = "Stop Service";
            ServiceStarted = true;
        }
        else
        {
            StartButtonLabel = "Start Service";
            ServiceStarted = false;
        }

        if (m_CallManager.GeneratingCalls == true)
        {
            StartCallsButtonLabel = "Stop Calls";
        }
        else
        {
            StartCallsButtonLabel = "Start Calls";
        }
    }

    private void OnUpdateStatistics(CallStatistics callStatistics)
    {
        InvokeAsync(() => 
        { 
            CurrentStatistics = callStatistics;
            StateHasChanged();
        });
    }

    private void OnErrorDetected(string errorMessage)
    {
        InvokeAsync(() => 
        {
            m_CallManager.GeneratingCalls = false;
            StartCallsButtonLabel = "Start Calls";
            ErrorMessage = errorMessage;
            ShowError = true;
        });
    }

    /// <summary>
    /// Called when the user navigates away from this page
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {

        // Un-hook the CallManagerService events
        m_CallManager.ErrorDetected -= OnErrorDetected;
        m_CallManager.UpdateCallStatistics -= OnUpdateStatistics;


    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

    private string StartButtonLabel = "Start Service";
    private bool ServiceStarted = false;

    private async void OnStartService()
    {
        if (ServiceStarted == false)
        {
            ServiceStarted = true;
            StartButtonLabel = "Stop Service";
            StateHasChanged();
            await m_CallManager.StartService(m_Settings);
        }
        else
        {
            ServiceStarted = false;
            StartButtonLabel = "Start Service";
            StartCallsButtonLabel = "Start Calls";
            StateHasChanged();
            m_CallManager.StopService();
        }
    }

    private string StartCallsButtonLabel = "Start Calls";

    private void OnStartCalls()
    {
        if (m_CallManager.CallManagerState != CallManagerStateEnum.Running)
        {
            ErrorMessage = "Click on Start Service first.";
            ShowError = true;
            return;
        }

        if (m_CallManager.GeneratingCalls == false)
        {
            m_CallManager.GeneratingCalls = true;
            StartCallsButtonLabel = "Stop Calls";
            StateHasChanged();
        }
        else
        {
            m_CallManager.GeneratingCalls = false;
            StartCallsButtonLabel = "Start Calls";
            StateHasChanged();
        }
    }

    private void OnEndCalls()
    {
        if (m_CallManager.CallManagerState != CallManagerStateEnum.Running)
        {
            ErrorMessage = "Calls cannot be ended because the Call Manager Service is not running.";
            ShowError = true;
            return;
        }

        m_CallManager.EndCalls();
    }

    private void OnDismissError()
    {
        ShowError = false;
    }
}

