using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.ClientAccount
{
    public class ClientAccountCreateUpdateDto
    {
        public string ClientId { get; set; }
        public decimal ClientAccountCreditLimit { get; set; }
        public decimal ClientAccountInterestRate { get; set; }
        public string ClientAccountStatus { get; set; }
        public string ClientAccountConditions { get; set; }
    }
}
