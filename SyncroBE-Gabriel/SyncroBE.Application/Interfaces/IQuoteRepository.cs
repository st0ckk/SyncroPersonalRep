using SyncroBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Application.Interfaces
{
    public interface IQuoteRepository
    {
        //Traiga todas las cotizaciones, incluido los detalles
        Task<IEnumerable<Quote>> GetAllAsync();
    }
}
