using System;
using System.Collections.Generic;
using System.Text;

namespace PancakeOrdering.Core.Domain.Enums
{
    internal enum OrderAction
    {
        Confirm,
        Cancel,
        StartPreparation,
        CompletePreparation,
        Dispatch,
        Deliver,
        Archive
    }
}
