using User.Api.CamposEnum;

namespace User.Api.DTOS
{
    public class CreateAccountUserDto
    {
        public string Name { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
