using PancakeOrdering.Core.Common.Results;

namespace PancakeOrdering.Core.Domain.Orders
{
    internal sealed record DeliveryAddress
    {
        public DeliveryAddress(string? street, string? city, string? country)
        {
            Street = street ?? string.Empty;
            City = city ?? string.Empty;
            Country = country ?? string.Empty;
        }

        public string Street { get; }

        public string City { get; }

        public string Country { get; }

        private Result InternalValidate()
        {
            return string.IsNullOrEmpty(Street) ||
                   string.IsNullOrEmpty(City) ||
                   string.IsNullOrEmpty(Country)
                ? Result.Failure(ErrorCode.InvalidDeliveryAddress)
                : Result.Success();
        }

        internal static Result Validate(DeliveryAddress? address)
        {
            return address?.InternalValidate() ?? Result.Failure(ErrorCode.InvalidDeliveryAddress);
        }
    }
}
