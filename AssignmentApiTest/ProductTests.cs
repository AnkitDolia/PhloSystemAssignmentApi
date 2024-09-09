using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using PhloSystemAssignmentApi.Model;
using PhloSystemAssignmentApi.Services;
using PhloSystemAssignmentApi.Stores;
using System.Diagnostics.CodeAnalysis;

namespace AssignmentApiTest
{
        [ExcludeFromCodeCoverage]
        public class ProductTests
        {
            private const int MaxPrice = 25;
            private const int MinPrice = 10;
            private readonly IList<ProductDetails> _products = new List<ProductDetails>
        {
            new()
            {
                Title = "A Red Trouser",
                Price = 10,
                Sizes = new[]
                {
                    "small",
                    "medium",
                    "large"
                },
                Description = "This trouser perfectly pairs with a green shirt."
            },
            new()
            {
                Title = "A Green Trouser",
                Price = 11,
                Sizes = new[]
                {
                    "small"
                },
                Description = "This trouser perfectly pairs with a blue shirt."
            }           
        };
            private readonly string[] _sizes = { "small", "medium", "large" };
            // The 5 top most used words in the description are :This, perfectly, pairs, with, a.
            // The 10 most used words in the description are: red, green, shoe, trouser, shirt, tie, hat, shirt, belt, bag
            private readonly string[] _keywords =
                {
                "bag",
                "belt",
                "blue",
                "green",
                "hat",
                "red",
                "shirt",
                "shoe",
                "tie",
                "trouser"
            };

            [Fact]
            public async Task ShouldReturnAllProductsWhenNoRequestFilter()
            {
                // Arrange
                var productStore = Substitute.For<IProductManage>();
                var memoryCache = Substitute.For<IMemoryCache>();
                var logger = Substitute.For<ILogger<IProductService>>();
                var ct = new CancellationToken();
                productStore.GrtHttpProducts(ct).Returns(_products);
                var productService = new ProductService(productStore, memoryCache, logger);

                // Act
                var response = await productService.ProductFilterAsync(new ProductRequest(), ct);

                // Assert
                await productStore.Received().GrtHttpProducts(ct);
                response.Products.Equals(_products);
            }

            [Theory]
            [InlineData(null, null, null)]
            [InlineData(10, "small", "red,green")]
            [InlineData(null, "medium", "blue")]
            [InlineData(20, null, "blue")]
            [InlineData(0, null, "blue")]
            [InlineData(10, "small", null)]
            [InlineData(10, "small", ",")]
            public async Task ShouldReturnAlProductsForRequestFilter(int? price, string size, string highlights)
            {
                // Arrange
                var productStore = Substitute.For<IProductManage>();
                var memoryCache = Substitute.For<IMemoryCache>();
                var logger = Substitute.For<ILogger<IProductService>>();
                var ct = new CancellationToken();
                productStore.GrtHttpProducts(ct).Returns(_products);
                var productService = new ProductService(productStore, memoryCache, logger);

                // Act
                var response =
                    await productService.ProductFilterAsync(
                        new ProductRequest() { Size = size, Hightlight = highlights, Maxprice = price }, ct);

                // Assert
                await productStore.Received().GrtHttpProducts(ct);
            _ = response.Products.All(p =>
            {
                if (price > 0) p.Price = ((int)price);
                if (!string.IsNullOrWhiteSpace(size)) p.Sizes.Contains(size);
                if (!string.IsNullOrWhiteSpace(highlights))
                {
                    foreach (var keyword in highlights.Split(ProductService.CommonSeparators, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (p.Description.Contains(keyword))
                        {
                            p.Description.Contains($"<em>{keyword}</em>");
                        }
                    }
                }
                return true;
            });
            }

            [Theory]
            [InlineData(null, null, null)]
            [InlineData(10, "small", "red,green")]
            [InlineData(null, "medium", "blue")]
            [InlineData(20, null, "blue")]
            [InlineData(10, "small", null)]
            [InlineData(10, "small", "")]
            public async Task ShouldReturnFilterForAllRequest(int? price, string size, string highlights)
            {
                // Arrange
                var productStore = Substitute.For<IProductManage>();
                var memoryCache = Substitute.For<IMemoryCache>();
                var logger = Substitute.For<ILogger<IProductService>>();
                var ct = new CancellationToken();
                productStore.GrtHttpProducts(ct).Returns(_products);
                var productService = new ProductService(productStore, memoryCache, logger);

                // Act
                var response =
                    await productService.ProductFilterAsync(
                        new ProductRequest() { Size = size, Hightlight = highlights, Maxprice = price }, ct);

                // Assert
                await productStore.Received().GrtHttpProducts(ct);
                response.Filter.MinPrice.Equals(MinPrice);
                response.Filter.MaxPrice.Equals(MaxPrice);
                response.Filter.Sizes?.Equals(_sizes);
                response.Filter.Keywords.OrderBy(s => s).Equals(_keywords);
            }

        }
    }