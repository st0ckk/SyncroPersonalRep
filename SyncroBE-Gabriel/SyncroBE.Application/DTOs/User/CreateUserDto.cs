using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.User
{
    public class CreateUserDto
    {
        public required string UserName { get; set; }
        public string? UserLastname { get; set; }
        public required string UserEmail { get; set; }
        public required string Password { get; set; }
        public required string UserRole { get; set; }
        public string? Telefono { get; set; }
        public string? TelefonoPersonal { get; set; }
    }
}


