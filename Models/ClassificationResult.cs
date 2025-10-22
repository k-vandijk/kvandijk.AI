namespace ai_categorisation.Models;

public sealed class ClassificationResult
{
    public string Category { get; set; } = "";
    public double Confidence { get; set; }
    public string[] Reasons { get; set; } = [];
}