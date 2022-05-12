using Common.Constants;
using Models.Responses;

namespace MetaHR_API.Utility
{
    public static class FileVerifier
    {
        public static CommandResult VerifyImage(IFormFile file)
        {
            if (file.Length > Sizes.MaxImageSizeBytes)
            {
                var sizeInMB = (int)(Sizes.MaxImageSizeBytes / (1024 * 1024));
                return CommandResult.GetErrorResult($"File must be less than {sizeInMB}MB");
            }

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext == ".jpg" || ext == ".jpeg")
            {
                if (FileSignatureVerifier.IsJpeg(file.OpenReadStream(), file.Length) == false)
                {
                    return CommandResult.GetErrorResult("Not a valid .JPEG image.");
                }
            }
            else if (ext == ".png")
            {
                if (FileSignatureVerifier.IsPng(file.OpenReadStream(), file.Length) == false)
                {
                    return CommandResult.GetErrorResult("Not a valid .PNG image.");
                }
            }
            else
            {
                return CommandResult.GetErrorResult("Must be a .JPEG or .PNG image.");
            }

            return CommandResult.SuccessResult;
        }

        public static CommandResult VerifyPdf(IFormFile file)
        {
            if (file.Length > Sizes.MaxPdfSizeBytes)
            {
                var sizeInMB = (int)(Sizes.MaxPdfSizeBytes / (1024 * 1024));
                return CommandResult.GetErrorResult($"File must be less than {sizeInMB}MB");
            }

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext == ".pdf")
            {
                if (FileSignatureVerifier.IsPdf(file.OpenReadStream(), file.Length) == false)
                {
                    return CommandResult.GetErrorResult("Not a valid .PDF file.");
                }
            }
            else
            {
                return CommandResult.GetErrorResult("Must be a .PDF file.");
            }

            return CommandResult.SuccessResult;
        }
    }
}
