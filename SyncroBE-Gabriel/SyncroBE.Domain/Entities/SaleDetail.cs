using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Domain.Entities
{
    public class SaleDetail
    {
        public int SaleDetailId { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        //Este data annotation se pone para decirle a EF que la BD calculara el valor para este campo
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)] 
        public decimal LineTotal { get; set; }
    }
}
