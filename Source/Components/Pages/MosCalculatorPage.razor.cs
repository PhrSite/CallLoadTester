/////////////////////////////////////////////////////////////////////////////////////
//  File:   MosCalculatorPage.razor.cs                              6 Sep 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

namespace CallLoadTester.Components.Pages;
using SipLib.Rtp;

/// <summary>
/// Model/Controller class for the MOS Calculator page
/// </summary>
public partial class MosCalculatorPage
{
    private MosCalculatorModel mosCalculatorModel { get; set; } = new MosCalculatorModel();

    private bool mosCalculated { get; set; } = false;

    private void OnSubmit()
    {
        MeanOpinionScore Mos = new MeanOpinionScore(mosCalculatorModel.PacketLoss, mosCalculatorModel.Jitter, 0); ;
        mosCalculatorModel.Result = Mos.MOS;
        mosCalculated = true;
    }

    private void OnChange()
    {
        //mosCalculated = false;
        //StateHasChanged();
    }
}
