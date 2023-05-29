using Microsoft.AspNetCore.Components.Forms;
using PocCommon.Models;

namespace WebImageLibPoc.Infra
{
    public interface IImageService
    {
        Task<IEnumerable<ImageModel>> GetAllImageModelsAsync();
        Task<ImageModel> SaveImageModelAsync(ImageModel imageModel);
        Task<ImageModel> UploadToBlobAndSaveImageModelAsync(IBrowserFile file, string fileName);
        Task<bool> DeleteImageModelAsync(string id);
    }
}
