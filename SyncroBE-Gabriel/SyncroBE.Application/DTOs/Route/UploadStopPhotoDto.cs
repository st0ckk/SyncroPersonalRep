using Microsoft.AspNetCore.Http;

namespace SyncroBE.Application.DTOs.Route
{
    public class UploadStopPhotoDto
    {
        public IFormFile Photo { get; set; } = null!;
    }
}