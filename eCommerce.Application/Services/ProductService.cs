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
        private readonly UserValidator _userValidator;

        public ProductService(IGenericRepository<Product> productRepo, IProductRepository productRepository, UserValidator userValidator)
        {
            _productRepo = productRepo;
            _productRepository = productRepository;
            _userValidator = userValidator;
        }

        public async Task<ServiceResult<PagedResult<ProductResponseDto>>> GetAllProductsAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var products = await _productRepository.GetAllWithDetailsAsync();

            var totalCount = products.Count();

            var productDtos = products
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    AverageRating = p.AverageRating,
                    Description = p.Description,
                    DiscountRate = p.DiscountRate,
                    Price = p.Price * (1 - (p.DiscountRate / 100m )),
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Variants = p.Variants.Select(v => new ProductVariantResponseDto
                    {
                        Id = v.Id,
                        Size = v.Size,
                        Stock = v.Stock,
                    }).OrderBy(v => v.Id).ToList(),
                    Images = p.Images.Select(i => new ProductImageResponseDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain
                    }).OrderBy(i => i.Id).ToList()
                }).ToList();

            var pagedResult = new PagedResult<ProductResponseDto>(
                productDtos,
                totalCount,
                pageNumber,
                pageSize
            );

            return ServiceResult<PagedResult<ProductResponseDto>>.Success(pagedResult);
        }
        
        public async Task<ServiceResult<PagedResult<ProductResponseDto>>> GetProductByCategoryAsync(string categoryName,int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var products = await _productRepository.GetProductByCategory(categoryName);

            var totalCount = products.Count();

            var productDtos = products
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    AverageRating = p.AverageRating,
                    DiscountRate = p.DiscountRate,
                    Price = p.Price * (1 - (p.DiscountRate / 100m )),
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Variants = p.Variants.Select(v => new ProductVariantResponseDto
                    {
                        Id = v.Id,
                        Size = v.Size,
                        Stock = v.Stock,
                    }).OrderBy(v => v.Id).ToList(),
                    Images = p.Images.Select(i => new ProductImageResponseDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsMain = i.IsMain
                    }).OrderBy(i => i.Id).ToList()
                }).ToList();

            var pagedResult = new PagedResult<ProductResponseDto>(
                productDtos,
                totalCount,
                pageNumber,
                pageSize
            );

            return ServiceResult<PagedResult<ProductResponseDto>>.Success(pagedResult);
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
                AverageRating = product.AverageRating,
                DiscountRate = product.DiscountRate,
                Price = product.Price * (1 - (product.DiscountRate / 100m)),
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                Variants = product.Variants.Select(v => new ProductVariantResponseDto
                {
                    Id = v.Id,
                    Size = v.Size,
                    Stock = v.Stock,
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
        
        public async Task<ServiceResult<ProductDto>> CreateProductAsync(ProductDto dto, string token)
{
            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult<ProductDto>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                Price = dto.Price,
                DiscountRate = 0,
                AverageRating = 0,
                CategoryId = dto.CategoryId,
                Variants = dto.Variants.Select(v => new ProductVariant
                {
                    Size = v.Size,
                    Stock = v.Stock,
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
                }).ToList(),
                Images = product.Images.Select(i => new ProductImageDto
                {
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain
                }).ToList()
            };

            return ServiceResult<ProductDto>.SuccessAsCreated(resultDto, $"/api/products/{product.Id}");
        }

        public async Task<ServiceResult<ProductDto>> UpdateProductAsync(int id, ProductDto dto, string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult<ProductDto>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var existing = await _productRepository.GetByIdWithDetailsAsync(id); 
            if (existing == null) return ServiceResult<ProductDto>.Fail("Ürün bulunamadı", HttpStatusCode.NotFound);

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

            await _productRepo.UpdateAsync(existing);
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
                }).ToList(),
                Images = existing.Images.Select(i => new ProductImageDto
                {
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain
                }).ToList()
            };

            return ServiceResult<ProductDto>.Success(updatedDto);
        }

        public async Task<ServiceResult> DeleteProductAsync(int id, string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var existing = await _productRepo.GetByIdAsync(id);
            if (existing == null) return ServiceResult.Fail("Ürün bulunamadı", HttpStatusCode.NotFound);

            await _productRepo.RemoveAsync(existing);
            await _productRepo.SaveChangesAsync();

            return ServiceResult.NoContent();
        }

        public async Task<ServiceResult> DeleteImageAsync(int id, string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            try
            {
                await _productRepository.DeleteImageAsync(id);
                return ServiceResult.Success(status: HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail($"Silme işlemi sırasında hata oluştu: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }
    }
}