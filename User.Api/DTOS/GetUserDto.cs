namespace Bank.Api.DTOS;

public class GetUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string CPF { get; set; }
    public string AccountNumber { get; set; }
}