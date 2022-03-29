using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Moveness.Services
{
    public interface IBlobService
    {
        Task<string> UploadImageUser(IFormFile file);
        Task<string> UploadImageTeam(IFormFile file);
    }

    public class BlobService : IBlobService
    {
        public const string UsersContainer = "users";
        public const string TeamsContainer = "teams";

        private readonly IConfiguration _configuration;

        public BlobService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadImageUser(IFormFile file)
        {
            var container = GetCloudBlobContainer(UsersContainer);
            
            if (file is null || file.Length <= 0 || string.IsNullOrEmpty(file.FileName))
                return new Uri(container.Uri, "users/default-user.jpg").ToString();

            var guid = Guid.NewGuid().ToString();
            var extension = file.FileName.Split(".").LastOrDefault();

            if (extension == null)
                return new Uri(container.Uri, "users/default-user.jpg").ToString();

            // Get a reference to the blob address, then upload the file to the blob.
            var cloudBlockBlob = container.GetBlobClient($"{guid}.{extension}");

            // Pass in memory stream directly
            using var stream = file.OpenReadStream();
            await cloudBlockBlob.UploadAsync(stream);

            return cloudBlockBlob.Uri.ToString();
        }

        public async Task<string> UploadImageTeam(IFormFile file)
        {
            var container = GetCloudBlobContainer(TeamsContainer);

            if (file is null || file.Length <= 0 || string.IsNullOrEmpty(file.FileName))
                return null;

            var guid = Guid.NewGuid().ToString();
            var extension = file.FileName.Split(".").LastOrDefault();

            if (extension == null)
                return null;

            // Get a reference to the blob address, then upload the file to the blob.
            var cloudBlockBlob = container.GetBlobClient($"{guid}.{extension}");

            // Pass in memory stream directly
            using var stream = file.OpenReadStream();
            await cloudBlockBlob.UploadAsync(stream);
            return cloudBlockBlob.Uri.ToString();
        }

        private BlobContainerClient GetCloudBlobContainer(string containerReference)
        {
            var storageConnectionString = _configuration["ConnectionStrings:Storage:Blob"];
            var containerClient = new BlobContainerClient(storageConnectionString, containerReference);
            return containerClient;
        }
    }
}
