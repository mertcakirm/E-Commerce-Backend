using Microsoft.AspNetCore.Http;

namespace eCommerce.Application.DTOs;

public class UploadProductImageRequest
{
    public IFormFile File { get; set; }
}