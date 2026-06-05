namespace EntFram_04.Models;

public class Buyer
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; }
    public string Email { get; set; }
    public int CityId { get; set; }
}