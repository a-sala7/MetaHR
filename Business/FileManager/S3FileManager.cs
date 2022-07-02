using Amazon;
using Amazon.S3;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3.Model;
using System.Net;
using Models.Responses;

namespace Business.FileManager
{
    public class S3FileManager : IFileManager
    {
        private readonly AmazonS3Client _client;
        public S3FileManager()
        {
            _client = new AmazonS3Client(RegionEndpoint.EUCentral1);
        }
        private static void ValidateFolderName(string folderName)
        {
            if(folderName.Length > 100)
            {
                throw new ArgumentException("Folder name can't be longer than 100 characters.");
            }
            string allowed = "qwertyuiopasdfghjklzxcvbnm1234567890-_";
            if(folderName.Any(c => allowed.Contains(c) == false))
            {
                throw new ArgumentException("Invalid folder name.");
            }
        }
        public async Task<string> UploadFile(string fileName, Stream stream, string contentType, string folder = "")
        {
            ValidateFolderName(folder);
            string key;
            if (string.IsNullOrWhiteSpace(folder)) { key = fileName; }
            else { key = folder + "/" + fileName; }

            var putRequest = new PutObjectRequest
            {
                Key = key,
                BucketName = "metahrdev",
                InputStream = stream,
                ContentType = contentType
            };

            var resp = await _client.PutObjectAsync(putRequest);
            if(resp.HttpStatusCode == HttpStatusCode.OK)
            {
                return @"https://metahrdev.s3.eu-central-1.amazonaws.com/" + key;
            }
            throw new Exception("UPLOAD FAILED! STATUS CODE: " + resp.HttpStatusCode.ToString());
        }

        public async Task DeleteFile(string fileName, string folder = "")
        {
            ValidateFolderName(folder);
            string key;
            if (string.IsNullOrWhiteSpace(folder)) { key = fileName; }
            else { key = folder + "/" + fileName; }

            try
            {
                var metadataRequest = new GetObjectMetadataRequest
                {
                    BucketName = "metahrdev",
                    Key = key,
                };
                var metadataResponse = await _client.GetObjectMetadataAsync(metadataRequest);
                var code = metadataResponse.HttpStatusCode;
                if (code != HttpStatusCode.OK)
                {
                    throw new Exception(
                        "DELETION FAILED: GET OBJECT METADATA FAILED!! STATUS CODE: " 
                        + code.ToString()
                        );
                }
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception(
                    "DELETION FAILED: GET OBJECT METADATA FAILED!! STATUS CODE: " 
                    + ex.StatusCode.ToString()
                    );
            }
            

            var delRequest = new DeleteObjectRequest
            {
                Key = key,
                BucketName = "metahrdev"
            };

            await _client.DeleteObjectAsync(delRequest);
        }

        public async Task<string> GetPreSignedURL(string fileName, string folder)
        {
            var key = folder + "/" + fileName;
            var req = new GetPreSignedUrlRequest()
            {
                Key = key,
                BucketName = "metahrdev",
                Expires = DateTime.UtcNow.AddMinutes(10)
            };
            return _client.GetPreSignedURL(req);
        }
    }
}
