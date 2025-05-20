using WebApplication5.Dto;

public class OrderDto
{
    public int Id { get; set; }
    public int VisitId { get; set; }
    public string OrderRef { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderLineDto> OrderLines { get; set; }
}