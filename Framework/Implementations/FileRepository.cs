using Database.Models;
using Framework.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Framework.Helpers;
using System.Collections.Generic;
using Framework.Providers;
using Framework.Helpers.ExtensionMethods;

namespace Framework.Implementations
{
    public class FileRepository : IFileRepository
    {
        private const string PROJECT = "Framework";
        private const string CLASS = "FileRepository";

        private const string DEFAULT_USER_IMAGE = "default-user-image.png";
        public string DefaultProfilePhotoPath { get; }
        public string UserImagesPath { get; }
        public string ContentImagesPath { get; }
        public string LogsPath { get; }
        public string EmailLogsPath { get; }
        private ISystemErrorLogger _systemErrors { get; }

        public FileRepository()
        {
            UserImagesPath = @"/images/user-images/";
            ContentImagesPath = @"/images/content-images/";
            DefaultProfilePhotoPath = Path.Combine(ContentImagesPath, DEFAULT_USER_IMAGE);
            LogsPath = @"/Logs/";
            EmailLogsPath = Helper.CombinePaths(LogsPath, @"/EmailLogs/");
            _systemErrors = new SystemErrorLogger();
        }

        public string GetUserProfilePhotoPathOrDefaultPhotoPath(UserManager<CustomClient> userManager, IWebHostEnvironment environment, string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return DefaultProfilePhotoPath;

                if(userId.IsAnonymousOrUnauthorized())
                    return DefaultProfilePhotoPath;

                var user = userManager.FindByIdAsync(userId).Result;

                if (user != null && UserImageExists(environment, user.ProfilePhoto))
                    return Path.Combine(UserImagesPath, user.ProfilePhoto);

                return DefaultProfilePhotoPath;
            }
            catch (Exception)
            {
                return DefaultProfilePhotoPath;
            }
        }

        public string GetPaymentTypeLogo(IWebHostEnvironment environment, string paymentTypeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentTypeId))
                    return null;

                var paymentTypes = new List<PaymentTypes>
                {
                    PaymentTypesProvider.PaymentCards,
                    PaymentTypesProvider.PayPal,
                    PaymentTypesProvider.BankTransfer,
                    PaymentTypesProvider.Posta,
                    PaymentTypesProvider.CounterApathyPayment,
                    PaymentTypesProvider.USDCoin,
                    PaymentTypesProvider.USDTether
                };

                var paymentType = paymentTypes.SingleOrDefault(type => type.Id == paymentTypeId);

                if (paymentType == default)
                    return null;

                if(ContentImagesFolderExists(environment) && ContentImageExists(environment, paymentType.Logo))
                {
                    return Helper.CombinePaths(ContentImagesPath, paymentType.Logo);
                }

                return null;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "GetPaymentTypeLogo");
                return null;
            }
        }

        public bool ContentImageExists(IWebHostEnvironment environment, string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return false;

            return new FileInfo(Path.Combine(environment.WebRootPath, @"images/content-images/" + imageName)).Exists;
        }

        public bool UserImageExists(IWebHostEnvironment environment, string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName))
                return false;

            return new FileInfo(Path.Combine(environment.WebRootPath, @"images/user-images/" + imageName)).Exists;
        }

        public bool ContentImagesFolderExists(IWebHostEnvironment environment)
        {
            try
            {
                return Directory.Exists(Helper.CombinePaths(environment.WebRootPath, ContentImagesPath));
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "ContentImagesFolderExists");
                return false;
            }
        }

        public bool UserImagesFolderExists(IWebHostEnvironment environment)
        {
            try
            {
                return Directory.Exists(Helper.CombinePaths(environment.WebRootPath, UserImagesPath));
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "UserImagesFolderExists");
                return false;
            }
        }

        public string GetContentImagePath(IWebHostEnvironment environment, string imageName)
        {
            if (ContentImagesFolderExists(environment) && ContentImageExists(environment, imageName))
            {
                return Helper.CombinePaths(ContentImagesPath, imageName);
            }

            return null;
        }

        public bool AddUserImagesFolder(IWebHostEnvironment environment)
        {
            try
            {
                if (UserImagesFolderExists(environment))
                    return true;

                Directory.CreateDirectory(Helper.CombinePaths(environment.WebRootPath, UserImagesPath));
                return UserImagesFolderExists(environment) ? true : throw new Exception($"Folder '{UserImagesPath}' not created.");
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddUserImagesFolder");
                return false;
            }

        }

        public async Task<string> CreateUserImageAsync(IWebHostEnvironment environment, IFormFile file)
        {
            if (UserImageExists(environment, file.FileName))
                return file.FileName;

            string uniquieFileNameWithExtension = Helper.GenerateNumbersId() +
                                                  Path.GetExtension(file.FileName);

            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            if (uniquieFileNameWithExtension.Any(invalidFileNameChars.Contains))
            {
                foreach (var c in invalidFileNameChars)
                {
                    uniquieFileNameWithExtension = uniquieFileNameWithExtension.Replace(c, '-');
                }
            }

            var saveToPath = Path.Combine(environment.WebRootPath, @"/images/user-images") + $@"/{uniquieFileNameWithExtension}";

            var invalidPathChars = Path.GetInvalidPathChars();
            if (saveToPath.Any(invalidPathChars.Contains))
            {
                foreach (var c in invalidPathChars)
                {
                    saveToPath = saveToPath.Replace(c, '-');
                }
            }

            using (var stream = File.Create(environment.ContentRootPath + @"/wwwroot" + saveToPath))
            {
                await file.CopyToAsync(stream);
                await stream.FlushAsync();
            }

            return uniquieFileNameWithExtension;
        }

        public async Task<string> CreateUserImageAsync(IWebHostEnvironment environment, string imageNamePrefix, IFormFile file)
        {
            if (UserImageExists(environment, file.FileName))
                return file.FileName;

            string uniquieFileNameWithExtension = imageNamePrefix + "-" + Helper.GenerateNumbersId() +
                                      Path.GetExtension(file.FileName);

            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            if (uniquieFileNameWithExtension.Any(invalidFileNameChars.Contains))
            {
                foreach (var c in invalidFileNameChars)
                {
                    uniquieFileNameWithExtension = uniquieFileNameWithExtension.Replace(c, '-');
                }
            }

            var saveToPath = Path.Combine(environment.WebRootPath, @"/images/user-images") + $@"/{uniquieFileNameWithExtension}";

            var invalidPathChars = Path.GetInvalidPathChars();
            if (saveToPath.Any(invalidPathChars.Contains))
            {
                foreach (var c in invalidPathChars)
                {
                    saveToPath = saveToPath.Replace(c, '-');
                }
            }

            using (var stream = File.Create(environment.ContentRootPath + @"/wwwroot" + saveToPath))
            {
                await file.CopyToAsync(stream);
                await stream.FlushAsync();
            }

            return uniquieFileNameWithExtension;
        }

        public bool DeleteUserImage(IWebHostEnvironment environment, string imageName)
        {
            if (UserImageExists(environment, imageName))
            {
                new FileInfo(Helper.CombinePaths(environment.WebRootPath, @"images/user-images/" + imageName)).Delete();

                return true;
            }

            return false;
        }

        public bool LogsFolderExists()
        {
            try
            {
                return Directory.Exists(Helper.CombinePaths(Directory.GetCurrentDirectory(), LogsPath));
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "LogsFolderExists");
                return false;
            }

        }

        public bool AddLogsFolder()
        {
            try
            {
                if (LogsFolderExists())
                    return true;

                Directory.CreateDirectory(Helper.CombinePaths(Directory.GetCurrentDirectory(), LogsPath));
                return LogsFolderExists() ? true : throw new Exception($"Folder '{LogsPath}' not created.");
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddLogsFolder");
                return false;
            }
        }

        public bool EmailLogsFolderExists()
        {
            try
            {
                var q = EmailLogsPath;
                var w = Helper.CombinePaths(Directory.GetCurrentDirectory(), EmailLogsPath);
                var e = Directory.Exists(Helper.CombinePaths(Directory.GetCurrentDirectory(), EmailLogsPath));
                return e;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "EmailLogsFolderExists");
                return false;
            }
        }

        public bool AddEmailLogsFolder()
        {
            try
            {
                if (EmailLogsFolderExists())
                    return true;

                Directory.CreateDirectory(Helper.CombinePaths(Directory.GetCurrentDirectory(), EmailLogsPath));
                return EmailLogsFolderExists() ? true : throw new Exception($"Folder '{EmailLogsPath}' not created.");
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "AddEmailLogsFolder");
                return false;
            }
        }

        public bool PrepareToRun(IWebHostEnvironment environment)
        {
            try
            {
                //Logs must be before EmailLogs because EmailLogs are inside Logs folder.
                var logsFolderAdded = AddLogsFolder();
                var emailLogsFolderAdded = AddEmailLogsFolder();
                var userImagesFolderAdded = AddUserImagesFolder(environment);

                var failed = string.Empty;

                if (!logsFolderAdded) failed += $"Folder '{LogsPath}' not created.|";
                if (!emailLogsFolderAdded) failed += $"Folder '{EmailLogsPath}' not created.";
                if (!userImagesFolderAdded) failed += $"Folder '{UserImagesPath}' not created.";

                if (!string.IsNullOrEmpty(failed)) throw new Exception(failed);

                return true;
            }
            catch (Exception e)
            {
                _systemErrors.SaveError(e, PROJECT, CLASS, "PrepareToRun");
                return false;
            }
        }
    }
}
