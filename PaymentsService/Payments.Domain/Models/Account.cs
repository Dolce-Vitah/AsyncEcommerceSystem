using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Domain.Models
{
    public class Account
    {
        public Guid ID { get; private set; }
        public Guid UserId { get; private set; }
        private Account() { }
        public Account(Guid userId)
        {
            ID = Guid.NewGuid();
            UserId = userId;
        }
    }
}
