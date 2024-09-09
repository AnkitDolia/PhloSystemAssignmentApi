namespace PhloSystemAssignmentApi.Model;
    using Swashbuckle.AspNetCore.Annotations;


    public class ProductRequest
    {
        [SwaggerSchema("The maximum product size to filter on", ReadOnly = true)]
        public int? Maxprice { get; set; }

        [SwaggerSchema("The minimum product size to filter on", ReadOnly = true)]
        public int? Minprice { get; set; }

        [SwaggerSchema("The product size to filter on", ReadOnly = true)]
        public string? Size { get; set; }

        [SwaggerSchema("The comma separated list of keywords to highlight in the product description", ReadOnly = true)]
        public string? Hightlight { get; set; }
    }