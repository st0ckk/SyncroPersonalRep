using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class QuoteDetail
    {
        public int QuoteDetailId { get; set; }
        public int QuoteId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        //Este data annotation se pone para decirle a EF que la BD calculara el valor para este campo
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal LineTotal { get; set; }
        public Quote Quote { get; set; }
        public Product Product { get; set; }
    }
}
