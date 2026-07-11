using PancakeOrdering.Core.Domain.Enums;
using PancakeOrdering.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Core.Domain.States
{
    internal class PreparingState : StateBase
    {
        public static PreparingState Instance { get; } = new();

        private PreparingState()
        {
        }

        public override OrderStatus Status => OrderStatus.Preparing;

        public override bool CanChangeAddress => false;
        public override bool CanModifyPancakes => false;
        public override bool CanCancel => false;
        public override bool CanArchive => false;

        public override Result ValidateEntry(Order order)
            => Result.Success();
    }
}
