using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Domain.ValueObjects
{
    public enum OrderStatus
    {
        Pending,
        Paid,
        Failed
    }
}
