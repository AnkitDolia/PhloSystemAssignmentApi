using Newtonsoft.Json;
using PhloSystemAssignmentApi.Model;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Xml;

namespace PhloSystemAssignmentApi.Stores
{
    [ExcludeFromCodeCoverage]
    public class ProductManage : IProductManage
    {

        private readonly ILogger<ProductManage> _logger;

        public ProductManage(ILogger<ProductManage> logger)
        {
            _logger = logger;
        }

        public async Task<IList<ProductDetails>> GrtHttpProducts(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("fetching products");
                using var httpClient = new HttpClient();
                var response = await httpClient.GetFromJsonAsync<ProductResponse>(ConstantsVariables.ProductUrl, cancellationToken);

                Console.WriteLine(JsonConvert.SerializeObject(response, Newtonsoft.Json.Formatting.Indented));

                return response?.Products ?? new List<ProductDetails>();
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.StatusCode == HttpStatusCode.NoContent)
                {
                    _logger.LogWarning("API did not returned any products.");
                    return new List<ProductDetails>();
                }

                _logger.LogError(
                    $"An error occurred while retrieving the products details. StatusCode: {httpEx.StatusCode}, Message: {httpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An unhandled error occurred while retrieving the products details. {ex.Message}");
                throw;
            }
        }
    }
}