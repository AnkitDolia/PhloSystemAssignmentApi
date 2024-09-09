using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PhloSystemAssignmentApi.Model;
using PhloSystemAssignmentApi.Services;
using System.Net.Mime;
using System.Net;
using Swashbuckle.AspNetCore.Annotations;

namespace PhloSystemAssignmentApi.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            Console.WriteLine(ConstantsVariables.InitializeMessege);
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        [HttpGet("/filter")]
        [SwaggerOperation(
            Summary = "List/Filter the products",
            Description = "List/Filter the products",
            OperationId = "Product.Filter",
            Tags = [nameof(ProductController)])
        ]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IList<RequestResponse>>> GetProductDetails(
            [FromQuery][SwaggerParameter("The product parameters")] ProductRequest parameters,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _productService.ProductFilterAsync(parameters, cancellationToken);
                return Ok(response);
            }
            catch (HttpRequestException httpException)
            {
                if (httpException.StatusCode == HttpStatusCode.NoContent) return NoContent();

                return Problem(httpException.Message, null, (int?)httpException.StatusCode, ConstantsVariables.ProductApiProcessException);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message, null, (int)HttpStatusCode.InternalServerError, ConstantsVariables.ProductApiProcessException);
            }
        }
    }
}