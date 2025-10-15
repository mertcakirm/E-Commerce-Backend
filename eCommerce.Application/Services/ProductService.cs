using eCommerce.Application.Interfaces;
using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using System.Net;
using eCommerce.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace eCommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IProductRepository _productRepository;
        private readonly UserValidator _userValidator;
        private readonly IAuditLogService _auditLogService;

        public ProductService(IGenericRepository<Product> productRepo, IProductRepository productRepository, UserValidator userValidator, IAuditLogService auditLogService)
        {
            _productRepo = productRepo;
            _productRepository = productRepository;
            _userValidator = userValidator;
            _auditLogService = auditLogService;
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
                Price = p.Price,
                PriceWithDiscount = p.Price * (1 - (p.DiscountRate / 100m)),
                CategoryIds = p.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                CategoryNames = p.ProductCategories.Select(pc => pc.Category.Name).ToList(),
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

        var pagedResult = new PagedResult<ProductResponseDto>(productDtos, totalCount, pageNumber, pageSize);
        return ServiceResult<PagedResult<ProductResponseDto>>.Success(pagedResult);
        }

        public async Task<ServiceResult<PagedResult<ProductResponseDto>>> GetAllProductsAdminAsync(int pageNumber, int pageSize, string token, string? searchTerm = null)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<PagedResult<ProductResponseDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var products = await _productRepository.GetAllWithDetailsAdminAsync(searchTerm);
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
                    SaleCount = p.SaleCount,
                    Price = p.Price,
                    IsDeleted = p.IsDeleted,
                    PriceWithDiscount = p.Price * (1 - (p.DiscountRate / 100m)),
                    CategoryIds = p.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                    CategoryNames = p.ProductCategories.Select(pc => pc.Category.Name).ToList(),
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

            var pagedResult = new PagedResult<ProductResponseDto>(productDtos, totalCount, pageNumber, pageSize);
            return ServiceResult<PagedResult<ProductResponseDto>>.Success(pagedResult);
        }

        public async Task<ServiceResult<PagedResult<ProductResponseDto>>> GetProductByCategoryAsync(string categoryName, int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var products = await _productRepository.GetAllWithDetailsAsync();
            products = products.Where(p => p.ProductCategories.Any(pc => pc.Category.Name == categoryName)).ToList();

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
                    Price = p.Price,
                    PriceWithDiscount = p.Price * (1 - (p.DiscountRate / 100m)),
                    CategoryIds = p.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                    CategoryNames = p.ProductCategories.Select(pc => pc.Category.Name).ToList(),
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

            var pagedResult = new PagedResult<ProductResponseDto>(productDtos, totalCount, pageNumber, pageSize);
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
                Price = product.Price,
                PriceWithDiscount = product.Price * (1 - (product.DiscountRate / 100m)),
                CategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                CategoryNames = product.ProductCategories.Select(pc => pc.Category.Name).ToList(),
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
        
        public async Task<ServiceResult<ProductResponseDto>> GetProductByIdAdminAsync(int id,string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<ProductResponseDto>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
            
            var product = await _productRepository.GetByIdWithDetailsAdminAsync(id);
            if (product == null)
                return ServiceResult<ProductResponseDto>.Fail("Ürün bulunamadı", HttpStatusCode.NotFound);

            var dto = new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                AverageRating = product.AverageRating,
                DiscountRate = product.DiscountRate,
                Price = product.Price,
                PriceWithDiscount = product.Price * (1 - (product.DiscountRate / 100m)),
                CategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                CategoryNames = product.ProductCategories.Select(pc => pc.Category.Name).ToList(),
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

        public async Task<ServiceResult<List<ProductResponseDto>>> GetLowStockProductsAsync(int limit, string token)
        {
            if (string.IsNullOrEmpty(token))
                return ServiceResult<List<ProductResponseDto>>.Fail("Token bulunamadı", HttpStatusCode.Unauthorized);

            var isAdmin = await _userValidator.IsAdminAsync(token);
            var user = await _userValidator.ValidateAsync(token);

            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<List<ProductResponseDto>>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var lowStockProducts = await _productRepository.GetProductsWithLowStockAsync(limit);

            var productDtos = lowStockProducts
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Variants = p.Variants.Select(v => new ProductVariantResponseDto
                    {
                        Id = v.Id,
                        Size = v.Size,
                        Stock = v.Stock,
                    }).OrderBy(v => v.Id).ToList(),
                }).ToList();

            return ServiceResult<List<ProductResponseDto>>.Success(productDtos);
        }

        public async Task<ServiceResult<ProductDto>> CreateProductAsync(ProductCreateDto dto, string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<ProductDto>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            // --- Resimleri kaydet ---
            var imageEntities = new List<ProductImage>();
            if (dto.Images != null && dto.Images.Count > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in dto.Images)
                {
                    if (file.Length <= 0) continue;

                    var extension = Path.GetExtension(file.FileName);
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    imageEntities.Add(new ProductImage
                    {
                        ImageUrl = $"/images/products/{fileName}",
                        IsMain = false
                    });
                }

                if (imageEntities.Count > 0)
                    imageEntities[0].IsMain = true;
            }

            // --- Ürün oluştur ---
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                Price = dto.Price,
                DiscountRate = 0,
                AverageRating = 0,
                Variants = dto.Variants.Select(v => new ProductVariant
                {
                    Size = v.Size,
                    Stock = v.Stock
                }).ToList(),
                Images = imageEntities,
                ProductCategories = dto.CategoryIds.Select(cid => new ProductCategory
                {
                    CategoryId = cid
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
                Variants = product.Variants.Select(v => new ProductVariantDto
                {
                    Size = v.Size,
                    Stock = v.Stock
                }).ToList(),
                Images = product.Images.Select(i => new ProductImageDto
                {
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain
                }).ToList(),
                CategoryIds = product.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                CategoryNames = product.ProductCategories.Select(pc => pc.Category?.Name ?? "").ToList()
            };

            await _auditLogService.LogAsync(
                userId: null,
                action: "CreateProduct",
                entityName: "Product",
                entityId: product.Id,
                details: $"Ürün oluşturuldu: {product.Id}"
            );

            return ServiceResult<ProductDto>.SuccessAsCreated(resultDto, $"/api/products/{product.Id}");
        }

        public async Task<ServiceResult<ProductDto>> UpdateProductAsync(int id, UpdateProductDto dto, string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);

            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<ProductDto>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var existing = await _productRepository.GetByIdWithDetailsAsync(id);
            if (existing == null)
                return ServiceResult<ProductDto>.Fail("Ürün bulunamadı", HttpStatusCode.NotFound);

            // --- Ürün bilgilerini güncelle ---
            existing.Name = dto.Name;
            existing.Price = dto.Price;
            existing.BasePrice = dto.BasePrice;
            existing.Description = dto.Description;

            // Çoklu kategori güncelleme
            existing.ProductCategories.Clear();
            existing.ProductCategories = dto.CategoryIds.Select(cid => new ProductCategory
            {
                CategoryId = cid
            }).ToList();

            await _productRepo.UpdateAsync(existing);
            await _productRepo.SaveChangesAsync();

            var updatedDto = new ProductDto
            {
                Name = existing.Name,
                Description = existing.Description,
                BasePrice = existing.BasePrice,
                Price = existing.Price,
                Variants = existing.Variants.Select(v => new ProductVariantDto
                {
                    Size = v.Size,
                    Stock = v.Stock
                }).ToList(),
                Images = existing.Images.Select(i => new ProductImageDto
                {
                    ImageUrl = i.ImageUrl,
                    IsMain = i.IsMain
                }).ToList(),
                CategoryIds = existing.ProductCategories.Select(pc => pc.CategoryId).ToList(),
                CategoryNames = existing.ProductCategories.Select(pc => pc.Category?.Name ?? "").ToList()
            };

            await _auditLogService.LogAsync(
                userId: null,
                action: "UpdateProduct",
                entityName: "Product",
                entityId: id,
                details: $"Ürün güncellendi: {id}"
            );

            return ServiceResult<ProductDto>.Success(updatedDto);
        }
        public async Task<ServiceResult> DeleteProductAsync(int id, string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
            
            if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            await _productRepository.ToggleProductActivity(id);
            await _productRepo.SaveChangesAsync();
            await _auditLogService.LogAsync(
                userId: null,
                action: "ToggleProduct",
                entityName: "Product",
                entityId: id,
                details: $"Ürün aktiflik durumu güncellendi: {id}"
            );

            return ServiceResult.NoContent();
        }

        public async Task<ServiceResult> DeleteImageAsync(int id, string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
            
            if (isAdmin.IsFail || !isAdmin.Data) return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            try
            {
                await _productRepository.DeleteImageAsync(id);
                await _auditLogService.LogAsync(
                    userId: null,
                    action: "DeleteProductImage",
                    entityName: "ProductImage",
                    entityId: id,
                    details: $"Ürün görseli silindi: {id}"
                );
                return ServiceResult.Success(status: HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail($"Silme işlemi sırasında hata oluştu: {ex.Message}", HttpStatusCode.InternalServerError);
            }
        }
        
        public async Task<ServiceResult> DiscountProduct(string token, int productId, int discountRate)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
        
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return ServiceResult.Fail("Ürün bulunamadı!", HttpStatusCode.NotFound);

            await _productRepository.DiscountProductAsync(productId, discountRate);
            await _auditLogService.LogAsync(
                userId: null,
                action: "DiscountProduct",
                entityName: "Product",
                entityId: productId,
                details: $"Ürüne indirim yapıldı: {productId}"
            );
            return ServiceResult.Success(status: HttpStatusCode.OK);
        }

        public async Task<ServiceResult> AddStock(int productId, string newSize, int quantity, string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
        
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
            
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return ServiceResult.Fail("Ürün bulunamadı!", HttpStatusCode.NotFound);

            await _productRepository.AddStockAsync(productId, newSize, quantity);
            await _auditLogService.LogAsync(
                userId: null,
                action: "AddStock",
                entityName: "ProductVariant",
                entityId: productId,
                details: $"Ürüne stok eklendi: {productId}"
            );
            return ServiceResult.Success(status: HttpStatusCode.OK);
        }
        
        public async Task<ServiceResult> RemoveStock(int variantId, string token)
        {
            var isAdmin = await _userValidator.IsAdminAsync(token);
        
            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);
            
            await _productRepository.RemoveStockAsync(variantId);
            await _auditLogService.LogAsync(
                userId: null,
                action: "RemoveStock",
                entityName: "ProductVariant",
                entityId: variantId,
                details: $"Üründen stok kaldırıldı: {variantId}"
            );
            return ServiceResult.Success(status: HttpStatusCode.OK);
        }
        
        public async Task<ServiceResult<ProductImage>> AddProductImageAsync(ProductImage image, string token)
        {
            if (string.IsNullOrEmpty(token))
                return ServiceResult<ProductImage>.Fail("Token bulunamadı", HttpStatusCode.Unauthorized);

            var isAdmin = await _userValidator.IsAdminAsync(token);

            if (isAdmin.IsFail || !isAdmin.Data)
                return ServiceResult<ProductImage>.Fail("Yetkisiz giriş!", HttpStatusCode.Forbidden);

            var savedImage = await _productRepository.AddImageAsync(image);

            await _auditLogService.LogAsync(
                userId: null,
                action: "AddImages",
                entityName: "ProductImage",
                entityId: savedImage.Id,
                details: $"Ürüne resim eklendi: {savedImage.Id}"
            );

            return ServiceResult<ProductImage>.Success(savedImage);
        }
        
    }
}