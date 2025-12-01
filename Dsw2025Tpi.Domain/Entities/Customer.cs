using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Customer : EntityBase
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public List<Order> Orders { get; set; }
    }
}