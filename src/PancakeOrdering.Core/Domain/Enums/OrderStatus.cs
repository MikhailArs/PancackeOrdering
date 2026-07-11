using System;
using System.Collections.Generic;
using System.Text;

namespace PancakeOrdering.Core.Domain.Enums
{
    internal enum OrderStatus
    {
        Draft,
        Confirmed,
        Preparing,
        Prepared,
        OutForDelivery,
        Delivered,
        Archived,
        Cancelled
    }
}
