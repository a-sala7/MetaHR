using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.FileManager
{
    public interface IFileManager
    {
        Task<string> UploadFile(string fileName, Stream stream, string contentType, string folder);
        Task DeleteFile(string fileName, string folder);
        Task<string> GetPreSignedURL(string fileName, string folder);
    }
}
