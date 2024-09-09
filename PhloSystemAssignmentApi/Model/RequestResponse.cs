namespace PhloSystemAssignmentApi.Model;
    using Swashbuckle.AspNetCore.Annotations;


/// <summary>Initializes a new instance of the <see cref="RequestResponse" /> class.</summary>
/// <param name="products">The products.</param>
public class RequestResponse(IEnumerable<ProductDetails> products)
{

    /// <summary>Initializes a new instance of the <see cref="RequestResponse" /> class.</summary>
    /// <param name="products">The products.</param>
    /// <param name="priceRange">The price range.</param>
    /// <param name="sizes">The sizes.</param>
    /// <param name="keywords">The keywords.</param>
    public RequestResponse(IEnumerable<ProductDetails> products, (int min, int max) priceRange, 
            string[] sizes, string[] keywords) : this(products)
        {
            Filter.MinPrice = priceRange.min;
            Filter.MaxPrice = priceRange.max;
            Filter.Sizes = sizes;
            Filter.Keywords = keywords;
        }

    [SwaggerSchema("The product list", ReadOnly = true)]
    public IEnumerable<ProductDetails> Products { get; set; } = products;

    [SwaggerSchema("The options available to filter the products", ReadOnly = true)]
    public ProductFilter Filter { get; set; } = new ProductFilter();
}
