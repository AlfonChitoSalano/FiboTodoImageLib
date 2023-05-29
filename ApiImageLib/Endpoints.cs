using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageLib.Infra;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PocCommon.Helpers;
using PocCommon.Models;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ImageLib
{
    public class Endpoints
    {
        private const string ContainerImageDirectory = "images/";

        private readonly IImageRepository _imageRepository;
        private readonly BlobContainerClient _blobContainerClient;

        public Endpoints(IImageRepository imageRepository)
        {
            var containerName = GetEnvironmentVariable("BlobStorageContainerName");
            var azureConnectionString = GetEnvironmentVariable("StorageAccountConnectionString");
            _blobContainerClient = new BlobContainerClient(azureConnectionString, containerName);
            _imageRepository = imageRepository;
        }

        [FunctionName(nameof(UploadImageFileAsync))]
        public async Task<IActionResult> UploadImageFileAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Initiate uploading file...");
            var formCollection = await req.ReadFormAsync();
            var file = formCollection.Files[0];
            var fileUrl = await UploadFileToBlobStorageAsync(file);

            var imageModel = new ImageModel
            {
                Id = Guid.NewGuid().ToString(),
                Title = file.FileName,
                Category = file.ContentType,
                Description = $"Item {file.FileName}",
                Url = fileUrl
            };

            log.LogInformation($"Saving image file '{imageModel.Title}' to db...");
            var imageModelResponse = await _imageRepository.SaveImageAsync(imageModel);
            return new OkObjectResult(imageModelResponse);
        }

        [FunctionName(nameof(SaveImageModelAsync))]
        public async Task<IActionResult> SaveImageModelAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<ImageModel>(requestBody);
            log.LogInformation($"Saving image file '{data.Title}' to db...");
            var imageModelResponse = await _imageRepository.SaveImageAsync(data);
            return new OkObjectResult(imageModelResponse);
        }

        [FunctionName(nameof(GetAllImageFilesAsync))]
        public async Task<IActionResult> GetAllImageFilesAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"Getting all image files...");
            var imageModelResponse = await _imageRepository.GetImagesAsync();
            return new OkObjectResult(imageModelResponse);
        }

        [FunctionName(nameof(DeleteImageFileAsync))]
        public async Task<IActionResult> DeleteImageFileAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = $"{nameof(DeleteImageFileAsync)}/{{id}}")]
            HttpRequest req,
            ILogger log,
            string id)
        {
            var found = await _imageRepository.GetImageByIdAsync(id);

            if (found == null)
            {
                log.LogWarning($"Image file with id '{id}' is not found!");
                return new OkObjectResult(false);
            }

            if (string.IsNullOrEmpty(found.Url))
            {
                log.LogWarning($"Image file with id '{id}' has no URL!");
                return new OkObjectResult(false);
            }

            var fileName = found.Url[(found.Url.LastIndexOf('/') + 1)..];
            var blob = _blobContainerClient.GetBlobClient($"{ContainerImageDirectory}{fileName}");
            var success = await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

            if (!success) return new OkObjectResult(false);
            log.LogInformation($"Successfully deleted file '{fileName}' from Blob Storage!");
            log.LogInformation($"Deleting image file with id {id} from the db...");
            await _imageRepository.DeleteImageByIdAsync(id);
            return new OkObjectResult(true);
        }

        [FunctionName(nameof(GetHeartBeat))]
        public static IActionResult GetHeartBeat(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
           ILogger log)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0.0";
            return new OkObjectResult(
                $"Hello I'm alive. My api version is {version}.");
        }

        private async Task<string> UploadFileToBlobStorageAsync(IFormFile file)
        {
            if (file.Length <= 0)
            {
                throw new NullReferenceException($"The file should not be empty at {nameof(Endpoints)}.{nameof(UploadFileToBlobStorageAsync)}");
            }

            var createResponse = await _blobContainerClient.CreateIfNotExistsAsync();

            if (createResponse != null && createResponse.GetRawResponse().Status == 201)
            {
                await _blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob);
            }

            var fileExtension = file.FileName[file.FileName.LastIndexOf('.')..].ToLower();
            var fileName = $"{ContainerImageDirectory}{DateHelpers.GetCurrentEpochTime()}{fileExtension}";
            var blob = _blobContainerClient.GetBlobClient(fileName);
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            await using var fileStream = file.OpenReadStream();
            await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = file.ContentType });
            return blob.Uri.ToString();
        }

        private static string GetEnvironmentVariable(string key)
            => Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
    }
}