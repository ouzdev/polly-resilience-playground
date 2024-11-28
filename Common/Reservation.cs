namespace Common;

public class Reservation
{
    public Guid Id { get; set; }
    public string ConfirmationNo { get; set; }
    public DateTime ArrivalDate { get; set; }
    public DateTime DepartureDate { get; set; }
}