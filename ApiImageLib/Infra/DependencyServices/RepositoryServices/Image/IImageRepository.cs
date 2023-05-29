using PocCommon.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageLib.Infra
{
    public interface IImageRepository
    {
        Task<ImageModel> SaveImageAsync(ImageModel imageModel);
        Task<List<ImageModel>> GetImagesAsync();
        Task<ImageModel> GetImageByIdAsync(string id);
        Task<bool> DeleteImageByIdAsync(string id);
    }
}