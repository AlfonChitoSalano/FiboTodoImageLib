using Microsoft.AspNetCore.Components.Forms;
using PocCommon.Models;
using System.Net.Http.Headers;

namespace WebImageLibPoc.Infra
{
    public class ImageService : RestServiceBase, IImageService
    {
        private const long FileMaxSizeInBytes = 5 * 1024 * 1024; // 5 MB

        private const string UploadToBlobAndSaveImageModel = "UploadImageFileAsync";
        private readonly string _uploadToBlobAndSaveImageModelEndpoint;

        private const string SaveImageModel = "SaveImageModelAsync";
        private readonly string _saveImageModelEndpoint;

        private const string GetAllImageModels = "GetAllImageFilesAsync";
        private readonly string _getImageModelsEndpoint;

        private const string DeleteImageModel = "DeleteImageFileAsync/{0}";
        private readonly string _deleteImageModelEndpoint;

        public ImageService(IConfiguration configuration) : base(configuration)
        {
            _uploadToBlobAndSaveImageModelEndpoint = $"{BaseFunctionApi}{UploadToBlobAndSaveImageModel}";
            _saveImageModelEndpoint = $"{BaseFunctionApi}{SaveImageModel}";
            _getImageModelsEndpoint = $"{BaseFunctionApi}{GetAllImageModels}";
            _deleteImageModelEndpoint = $"{BaseFunctionApi}{DeleteImageModel}";
        }

        public async Task<bool> DeleteImageModelAsync(string id)
            => await DeleteRemoteAsync<bool>(string.Format(_deleteImageModelEndpoint, id));

        public async Task<IEnumerable<ImageModel>> GetAllImageModelsAsync()
        {
            var response = await GetRemoteAsync<IEnumerable<ImageModel>>(_getImageModelsEndpoint);
            return response ?? new List<ImageModel>();
        }

        public async Task<ImageModel> UploadToBlobAndSaveImageModelAsync(IBrowserFile file, string fileName)
        {
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file.OpenReadStream(FileMaxSizeInBytes));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(content: fileContent, name: "\"file\"", fileName: fileName);
            var response = await PostRemoteAsync<ImageModel>(_uploadToBlobAndSaveImageModelEndpoint, content);
            return response;
        }

        public async Task<ImageModel> SaveImageModelAsync(ImageModel imageModel)
        {
            var response = await PostRemoteAsync<ImageModel>(_saveImageModelEndpoint, CastToStringContent<ImageModel>(imageModel));
            return response;
        }
    }
}