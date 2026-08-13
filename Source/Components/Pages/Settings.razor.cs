/////////////////////////////////////////////////////////////////////////////////////
//  File:   Settings.razor.cs                                       1 Jun 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace CallLoadTester.Components.Pages;

/// <summary>
/// Model/Controller class for the Settings Razor page of the CallLoadTester application.
/// </summary>
public partial class Settings
{

    private CallLoadTesterSettings TempSettings { get; set; }

    private bool ShowError { get; set; } = false;

    private string ErrorMessage { get; set; } = string.Empty;

    private bool ShowSuccess { get; set; } = false;
    private string SuccessMessage { get; set; } = string.Empty;

    /// <summary>
    /// Constructor
    /// </summary>
    public Settings()
    {
        TempSettings = CallLoadTesterSettings.GetSettings();
    }

    protected override async Task OnInitializedAsync()
    {
    }


    /// <summary>
    /// Called when the user clicks on the Submit button and initial input validation was successfully
    /// completed by the DataAnnotationsValidator component.
    /// </summary>
    private void OnSubmit()
    {
        // Validate the settings
        string? strError = CallLoadTesterSettings.ValidateSettings(TempSettings);
        if (strError != null)
        {
            ErrorMessage = strError;
            ShowError = true;
            ShowSuccess = false;
            return;
        }
        else
        {
            ShowError = false;
            SuccessMessage = $"Settings saved at {DateTime.Now.ToShortTimeString()}";
            ShowSuccess = true;
            CallLoadTesterSettings.SaveSettings(TempSettings);
        }
    }
}
