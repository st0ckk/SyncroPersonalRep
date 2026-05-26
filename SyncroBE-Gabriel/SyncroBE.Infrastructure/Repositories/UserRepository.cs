using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.Interfaces;
using SyncroBE.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncroBE.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SyncroDbContext _context;

        public UserRepository(SyncroDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetById(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }
    }
}
