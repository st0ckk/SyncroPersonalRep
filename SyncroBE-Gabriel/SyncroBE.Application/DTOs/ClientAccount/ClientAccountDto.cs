using SyncroBE.Application.DTOs.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.DTOs.ClientAccount
{
    public class ClientAccountDto
    {
        public int ClientAccountId { get; set; }
        public string ClientId { get; set; }
        public string CustomerName { get; set; }
        public int UserId { get; set; }
        public string ClientAccountNumber { get; set; }
        public DateTime ClientAccountOpeningDate { get; set; }
        public decimal ClientAccountCreditLimit { get; set; }
        public decimal ClientAccountInterestRate { get; set; }
        public decimal ClientAccountCurrentBalance { get; set; }
        public string ClientAccountStatus { get; set; }
        public string ClientAccountConditions { get; set; }
        public List<ClientAccountMovementDto>? Movements { get; set; }
        public ClientDto? Client { get; set; }
    }

    public class ClientAccountMovementDto
    {
        public int ClientAccountMovementId { get; set; }
        public DateTime ClientAccountMovementDate { get; set; }
        public string ClientAccountMovementDescription { get; set; }
        public decimal ClientAccountMovementAmount { get; set; }
        public decimal ClientAccountMovementOldBalance { get; set; }
        public decimal ClientAccountMovementNewBalance { get; set; }
        public string ClientAccountMovementType { get; set; }
    }
}
