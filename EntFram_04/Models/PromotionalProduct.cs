namespace EntFram_04.Models;

public class PromotionalProduct 
{ 
    public int Id { get; set; } 
    public string Name { get; set; } 
    public int CategoryId { get; set; }
    public int CountryId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}