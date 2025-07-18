using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Messages.DTOs
{
    public class PixCreationRequestedEvent
    {
        public Guid UserId { get; set; }
        public string PixKey { get; set; }
        public string Tipo { get; set; }
        public string? Bank { get; set; }
        public string? Account { get; set; }
    }
}
