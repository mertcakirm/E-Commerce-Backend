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
        private readonly IProductRepository _productRepository;

        public ProductService(IGenericRepository<Product> productRepo, IProductRepository productRepository)
        {
            _productRepo = productRepo;
            _productRepository = productRepository;
        }

        public async Task<ServiceResult<List<ProductResponseDto>>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllWithDetailsAsync();

            var productDtos = products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                BasePrice = p.BasePrice,
                Price = p.Price,
                CategoryId = p.CategoryId,
                Variants = p.Variants.Select(v => new ProductVariantResponseDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    Stock = v.Stock,
                    CostPrice = v.CostPrice
                }).ToList(),
                Images = p.Images.Select(i => new ProductImageResponseDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain
                }).ToList()
            }).ToList();

            return ServiceResult<List<ProductResponseDto>>.Success(productDtos);
        }

        public async Task<ServiceResult<ProductResponseDto>> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(id);
            if (product == null)
                return ServiceResult<ProductResponseDto>.Fail("Ürün bulunamadı", HttpStatusCode.NotFound);

            var dto = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                BasePrice = product.BasePrice,
                Price = product.Price,
                CategoryId = product.CategoryId,
                Variants = product.Variants.Select(v => new ProductVariantResponseDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    Stock = v.Stock,
                    CostPrice = v.CostPrice
                }).ToList(),
                Images = product.Images.Select(i => new ProductImageResponseDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain
                }).ToList()
            };

            return ServiceResult<ProductResponseDto>.Success(dto);
        }
        public async Task<ServiceResult<ProductDto>> CreateProductAsync(ProductDto dto)
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

            var resultDto = new ProductDto
            {
                Name = product.Name,
                Description = product.Description,
                BasePrice = product.BasePrice,
                Price = product.Price,
                CategoryId = product.CategoryId,
                Variants = product.Variants.Select(v => new ProductVariantDto
                {
                    Size = v.Size,
                    Stock = v.Stock,
                    CostPrice = v.CostPrice
                }).ToList(),
                Images = product.Images.Select(i => new ProductImageDto
                {
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain
                }).ToList()
            };

            return ServiceResult<ProductDto>.SuccessAsCreated(resultDto, $"/api/products/{product.Id}");
        }

        public async Task<ServiceResult<ProductDto>> UpdateProductAsync(int id, ProductDto dto)
        {
            var existing = await _productRepository.GetByIdWithDetailsAsync(id); 
            if (existing == null)
                return ServiceResult<ProductDto>.Fail("Ürün bulunamadı", HttpStatusCode.NotFound);

            existing.Name = dto.Name;
            existing.Price = dto.Price;
            existing.BasePrice = dto.BasePrice;
            existing.Description = dto.Description;
            existing.CategoryId = dto.CategoryId;

            existing.Variants.Clear();
            foreach (var variantDto in dto.Variants)
            {
                existing.Variants.Add(new ProductVariant
                {
                    Size = variantDto.Size,
                    Stock = variantDto.Stock,
                    CostPrice = variantDto.CostPrice,
                    ProductId = existing.Id
                });
            }

            existing.Images.Clear();
            foreach (var imageDto in dto.Images)
            {
                existing.Images.Add(new ProductImage
                {
                    ImageUrl = imageDto.ImageUrl,
                    IsMain = imageDto.IsMain,
                    ProductId = existing.Id
                });
            }

            _productRepo.Update(existing);
            await _productRepo.SaveChangesAsync();

            var updatedDto = new ProductDto
            {
                Name = existing.Name,
                Description = existing.Description,
                BasePrice = existing.BasePrice,
                Price = existing.Price,
                CategoryId = existing.CategoryId,
                Variants = existing.Variants.Select(v => new ProductVariantDto
                {
                    Size = v.Size,
                    Stock = v.Stock,
                    CostPrice = v.CostPrice
                }).ToList(),
                Images = existing.Images.Select(i => new ProductImageDto
                {
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain
                }).ToList()
            };

            return ServiceResult<ProductDto>.Success(updatedDto);
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