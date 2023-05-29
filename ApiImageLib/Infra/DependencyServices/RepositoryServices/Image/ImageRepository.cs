using PocCommon.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageLib.Infra
{
    public class ImageRepository : IImageRepository
    {
        private static readonly List<ImageModel> ImageModels = new();

        public Task<ImageModel> SaveImageAsync(ImageModel imageModel)
        {
            var found = ImageModels.FirstOrDefault(x => x.Id == imageModel.Id);

            if (found == null)
            {
                ImageModels.Add(imageModel);
                return Task.FromResult(imageModel);
            }

            ImageModels.Remove(found);
            ImageModels.Add(imageModel);
            return Task.FromResult(imageModel);
        }

        public Task<List<ImageModel>> GetImagesAsync()
            => Task.FromResult(ImageModels);

        public Task<ImageModel> GetImageByIdAsync(string id)
            => Task.FromResult(ImageModels.FirstOrDefault(i => i.Id == id));

        public Task<bool> DeleteImageByIdAsync(string id)
        {
            var found = ImageModels.FirstOrDefault(x => x.Id == id);

            if (found == null)
            {
                return Task.FromResult(false);
            }

            ImageModels.Remove(found);
            return Task.FromResult(true);
        }
    }
}