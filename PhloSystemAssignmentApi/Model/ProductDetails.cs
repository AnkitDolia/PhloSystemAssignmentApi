using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics.CodeAnalysis;

namespace PhloSystemAssignmentApi.Model
{
    [ExcludeFromCodeCoverage]
    public class ProductDetails
    {
        [SwaggerSchema("The product title", ReadOnly = true)]
        public string? Title { get; set; }

        [SwaggerSchema("The product price", ReadOnly = true)]
        public int Price { get; set; }

        [SwaggerSchema("The list of available size for this product", ReadOnly = true)]
        public string[] Sizes { get; set; } = { };

        [SwaggerSchema("The product description", ReadOnly = true)]
        public string? Description { get; set; }
    }
}