namespace Auth.Api.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public List<string>? AccountNumbers { get; set; }
    }
}
