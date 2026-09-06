/////////////////////////////////////////////////////////////////////////////////////
//  File:   MosCalculatorModel.cs                                   6 Sep 26 PHR
/////////////////////////////////////////////////////////////////////////////////////

using System.ComponentModel.DataAnnotations;

namespace CallLoadTester;

/// <summary>
/// Model class for the MOS Calculator page
/// </summary>
public class MosCalculatorModel
{
    /// <summary>
    /// Jitter in milliseconds
    /// </summary>
    [Required(ErrorMessage = "Jitter is required")]
    [Range(0, 1000)]
    public int Jitter { get; set; } = 0;

    /// <summary>
    /// Packet loss in percent
    /// </summary>
    [Required(ErrorMessage = "Packet Loss is required")]
    [Range(0, 100)]
    public int PacketLoss { get; set; } = 0;

    /// <summary>
    /// Output
    /// </summary>
    public double Result { get; set; } = 0;
}
