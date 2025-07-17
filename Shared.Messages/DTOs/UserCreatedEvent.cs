using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messages.DTOs
{
    public class UserCreatedEvent
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string ContaCorrente { get; set; }
        public string ContaPoupanca { get; set; }
    }
}
