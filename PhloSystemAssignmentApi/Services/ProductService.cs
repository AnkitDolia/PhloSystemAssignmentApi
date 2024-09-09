using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using PhloSystemAssignmentApi.Model;
using PhloSystemAssignmentApi.Stores;
using System.Text;
using System.Xml;

namespace PhloSystemAssignmentApi.Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger<IProductService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IProductManage _productStore;
        public static readonly char[] CommonSeparators = { ' ', '.', ';', ',' };

        /// <summary>Initializes a new instance of the <see cref="ProductService" /> class.</summary>
        /// <param name="productStore">The product store.</param>
        /// <param name="cache"></param>
        /// <param name="logger"></param>
        public ProductService(IProductManage productStore, IMemoryCache cache, ILogger<IProductService> logger)
        {
            _logger?.LogInformation(
                $"Initializing ProductService {(productStore == null ? "without" : "with")} ProductStore");
            _productStore = productStore ?? throw new ArgumentNullException(nameof(productStore));
            _cache = cache;
            _logger = logger;
        }

       
        public async Task<RequestResponse> ProductFilterAsync(ProductRequest parameters,
            CancellationToken cancellationToken = default)
        {
            // ReSharper disable once MethodHasAsyncOverload
            _logger?.LogInformation(
                $"Received product filter request : {JsonConvert.SerializeObject(parameters, Newtonsoft.Json.Formatting.Indented)}");

            var products = await GetProductsAsync(cancellationToken);
            parameters.Minprice ??= 0;
            var priceRange = parameters.Maxprice > 0 && parameters.Minprice > 0 ? GetPriceRange(products, min: (int?)parameters?.Minprice, (int?)parameters?.Maxprice) : (0, 0);
            var sizes = GetSizes(products);
            var keywords = GetKeywords(products);

            var filteredProducts = ApplyFilter(products, parameters ?? new ProductRequest());

            return new RequestResponse(filteredProducts, priceRange, sizes, keywords);
        }

        /// <summary>Gets the products.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     <br />
        /// </returns>
        private async Task<IList<ProductDetails>> GetProductsAsync(CancellationToken cancellationToken)
        {
            _ = _cache.TryGetValue("Products", out IList<ProductDetails> products);
            if (products != null) return products;

            products = await _productStore.GrtHttpProducts(cancellationToken);
            _cache.Set("Products", products);
            return products;
        }

        /// <summary>Gets the price range.</summary>
        /// <param name="products">The products.</param>
        /// <returns>
        ///     <br />
        /// </returns>
        private (int min, int max) GetPriceRange(IList<ProductDetails> products, int? min, int? max)
        {
            if (products == null) return (0, 0);
            if (_cache.TryGetValue("ProductPriceRange", out (int min, int max) priceRange) && min > 0 && max > 0)
            {
                return priceRange;
            }
        
            priceRange = (min ?? 0, max ?? 0);
            _cache.Set("ProductPriceRange", priceRange);
            return priceRange;
        }

        /// <summary>Gets the sizes.</summary>
        /// <param name="products">The products.</param>
        /// <returns>
        ///     <br />
        /// </returns>
        private string[] GetSizes(IList<ProductDetails> products)
        {
            if (products == null) return Array.Empty<string>();

            _ = _cache.TryGetValue("ProductSizes", out string[] productSize);
            if (productSize != null) return productSize;

            productSize = products?.SelectMany(p => p.Sizes)?.Distinct()?.ToArray() ?? [];
            _cache.Set("ProductSizes", productSize);
            return productSize;
        }

        /// <summary>Gets the keywords.</summary>
        /// <param name="products">The products.</param>
        /// <returns>
        /// List the 10 most used keywords except the top 5.
        /// </returns>
        private string[] GetKeywords(IList<ProductDetails> products)
        {
            if (products == null) return [];

            _cache.TryGetValue("ProductKeywords", out string[] productKeywords);
            if (productKeywords != null) return productKeywords;

            productKeywords = products
                .SelectMany(p => GetDescriptionWords(p.Description, CommonSeparators))
                .GroupBy(s => s)
                .Select(g => new
                {
                    KeyField = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(wg => wg.Count)
                .ThenBy(og => og.KeyField)
                .Skip(5)
                .Take(10)
                .Select(r => r.KeyField)
                .OrderBy(s => s)
                .ToArray();
            _cache.Set("ProductKeywords", productKeywords);
            return productKeywords;
        }

        /// Splits the given description into words using the specified separators and returns an enumerable collection of the words.
        /// </summary>
        /// <param name="description">The description to split into words.</param>wh        /// <param name="separators">The characters used to separate the words in the description.</param>qu        /// <returns>An enumerable collection of the words in the description.</returns>
        private IEnumerable<string> GetDescriptionWords(string description, params char[] separators) =>
            description.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Filters the products based on the given parameters.
        /// </summary>
        /// <param name="parameters">The product /// r parameters.</param>="/// <param name="cancellationToken">The cancellation token.</param>        /// <returns>A task that represents the asynchronous operation. The task result contains the filtered products and additional information.</returns>
        /// <returns></returns>
        private IEnumerable<ProductDetails> ApplyFilter(IEnumerable<ProductDetails> products, ProductRequest request)
        {
            var filteredProducts = request.Maxprice switch
            {
                null when string.IsNullOrWhiteSpace(request.Size) => products,

                <= 0 when string.IsNullOrWhiteSpace(request.Size) => products,

                not null when string.IsNullOrWhiteSpace(request.Size) => products.Where(p => p.Price <= request.Maxprice)
                    .AsParallel(),

                null when !string.IsNullOrWhiteSpace(request.Size) => products
                    .Where(p => p.Sizes.Contains(request.Size)).AsParallel(),

                <= 0 when !string.IsNullOrWhiteSpace(request.Size) => products
                    .Where(p => p.Sizes.Contains(request.Size)).AsParallel(),

                _ => products.Where(p => p.Price <= request.Maxprice && p.Sizes.Contains(request.Size)).AsParallel()
            };

            return ApplyHighlights(filteredProducts.ToList(), request?.Hightlight ?? string.Empty);
        }

        /// <summary>
        /// Applies the highlights to the product description.
        /// </summary>
        /// <param name="products">The products.</param>
        /// <param name="highLights">The high lights.</param>
        /// <returns></returns>
        private IEnumerable<ProductDetails> ApplyHighlights(List<ProductDetails> products, string highLights)
        {
            if (string.IsNullOrWhiteSpace(highLights)) return products;

            var keywords = highLights.Split(CommonSeparators, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (keywords.Count <= 0) return products;

            return products.Select(p =>
            {
                var productresp = new ProductDetails()
                {
                    Price = p.Price,
                    Sizes = p.Sizes,
                    Title = p.Title,
                    Description = p.Description
                };

                keywords.ForEach(highLights => productresp.Description = new StringBuilder(productresp.Description)
                    .Replace(highLights, $"<em>{highLights}</em>")
                    .ToString()
                );
                return productresp;
            });
        }
    }
}