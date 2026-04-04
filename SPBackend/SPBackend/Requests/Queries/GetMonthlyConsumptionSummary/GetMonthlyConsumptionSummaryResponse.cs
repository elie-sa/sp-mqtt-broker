namespace SPBackend.Requests.Queries.GetMonthlyConsumptionSummary;

public class GetMonthlyConsumptionSummaryResponse
{
    public double TotalConsumptionThisMonth { get; set; }
    public double? DifferenceFromLastMonth { get; set; }
    public double TotalCostThisMonth { get; set; }
    public double? CostDifferenceFromLastMonth { get; set; }
}
