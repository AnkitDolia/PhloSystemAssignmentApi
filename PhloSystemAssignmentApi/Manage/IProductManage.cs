using PhloSystemAssignmentApi.Model;

namespace PhloSystemAssignmentApi.Stores
{
    public interface IProductManage
    {
        /// <summary>Reals all.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     <br />
        /// </returns>
        public Task<IList<ProductDetails>> GrtHttpProducts(CancellationToken cancellationToken);
    }
}
