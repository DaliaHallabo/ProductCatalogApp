using ProductCatalogApp.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalogApp.Application.IRepository
{
    public interface IProductService
    {
        IEnumerable<Product> GetProductsForCurrentTime();
        IEnumerable<Product> GetAllProducts();
        List<Category> GetCategories();
        List<Product> GetProductsByCategory(int? categoryId);

        Product GetProductById(int id);
        void AddProduct(Product product);
        //void UpdateProduct(Product product);
        void DeleteProduct(int id);
        void UpdateProductWithLogging(Product product, string? adminUserId);
    }
}
