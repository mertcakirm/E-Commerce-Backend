using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using System.Net;
using eCommerce.Application.DTOs;

namespace eCommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _productRepo;

        public ProductService(IGenericRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<ServiceResult<List<Product>>> GetAllProductsAsync()
        {
            var products = (await _productRepo.GetAllAsync()).ToList();
            return ServiceResult<List<Product>>.Success(products);
        }

        public async Task<ServiceResult<Product>> GetProductByIdAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null)
                return ServiceResult<Product>.Fail("Ürün bulunamadı", HttpStatusCode.NotFound);

            return ServiceResult<Product>.Success(product);
        }

        public async Task<ServiceResult<Product>> CreateProductAsync(ProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                Variants = dto.Variants.Select(v => new ProductVariant
                {
                    Size = v.Size,
                    Stock = v.Stock,
                    CostPrice = v.CostPrice
                }).ToList(),
                Images = dto.Images.Select(i => new ProductImage
                {
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain
                }).ToList()
            };

            await _productRepo.AddAsync(product);
            await _productRepo.SaveChangesAsync();

            return ServiceResult<Product>.SuccessAsCreated(product, $"/api/products/{product.Id}");
        }

        public async Task<ServiceResult<Product>> UpdateProductAsync(int id, Product updatedProduct)
        {
            var existing = await _productRepo.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult<Product>.Fail("Ürün bulunamadı", HttpStatusCode.NotFound);

            existing.Name = updatedProduct.Name;
            existing.Price = updatedProduct.Price;
            existing.BasePrice = updatedProduct.BasePrice;

            if (updatedProduct.Variants != null)
            {
                foreach (var variant in updatedProduct.Variants)
                {
                    var existingVariant = existing.Variants.FirstOrDefault(v => v.Id == variant.Id);
                    if (existingVariant != null)
                    {
                        existingVariant.Size = variant.Size;
                        existingVariant.Stock = variant.Stock;
                        existingVariant.CostPrice = variant.CostPrice;
                    }
                    else
                    {
                        // Yeni varyasyon ekle
                        variant.Product = existing;
                        existing.Variants.Add(variant);
                    }
                }
            }

            _productRepo.Update(existing);
            await _productRepo.SaveChangesAsync();

            return ServiceResult<Product>.Success(existing);
        }

        public async Task<ServiceResult> DeleteProductAsync(int id)
        {
            var existing = await _productRepo.GetByIdAsync(id);
            if (existing == null)
                return ServiceResult.Fail("Ürün bulunamadı", HttpStatusCode.NotFound);

            _productRepo.Remove(existing);
            await _productRepo.SaveChangesAsync();

            return ServiceResult.NoContent();
        }
    }
}