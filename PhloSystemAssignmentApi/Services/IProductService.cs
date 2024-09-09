using PhloSystemAssignmentApi.Model;

namespace PhloSystemAssignmentApi.Services
{
    public interface IProductService
    {
        public Task<RequestResponse> ProductFilterAsync(ProductRequest parameters,
           CancellationToken cancellationToken);
    }
}
