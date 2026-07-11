using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Core.Domain.States
{
    internal class DeliveringState : StateBase
    {
        public static DeliveringState Instance { get; } = new();

        private DeliveringState()
        {
        }

        public override OrderStatus Status => OrderStatus.OutForDelivery;

        public override bool CanChangeAddress => false;
        public override bool CanModifyPancakes => false;

        public override Result ValidateEntry(Order order) => Result.Success();
    }
}
