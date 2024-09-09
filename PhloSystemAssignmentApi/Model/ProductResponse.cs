using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics.CodeAnalysis;

namespace PhloSystemAssignmentApi.Model
{
    [ExcludeFromCodeCoverage]
    public class ProductResponse
    {
        [SwaggerSchema("The product list", ReadOnly = true)]
        public IList<ProductDetails>? Products { get; set; }

        [SwaggerSchema("The Product API keys", ReadOnly = true)]
        public ProductKey? ApiKeys { get; set; }
    }
}