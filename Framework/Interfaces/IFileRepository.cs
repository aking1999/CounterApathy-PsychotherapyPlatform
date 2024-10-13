using Database.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Framework.Interfaces
{
    public interface IFileRepository
    {
        string UserImagesPath { get; }
        string DefaultProfilePhotoPath { get; }
        string GetUserProfilePhotoPathOrDefaultPhotoPath(UserManager<CustomClient> userManager, IWebHostEnvironment environment, string userId);
        string GetPaymentTypeLogo(IWebHostEnvironment environment, string paymentTypeId);
        bool ContentImageExists(IWebHostEnvironment environment, string imageName);
        bool UserImageExists(IWebHostEnvironment environment, string imageName);
        bool ContentImagesFolderExists(IWebHostEnvironment environment);
        bool UserImagesFolderExists(IWebHostEnvironment environment);
        string GetContentImagePath(IWebHostEnvironment environment, string imageName);
        bool AddUserImagesFolder(IWebHostEnvironment environment);
        Task<string> CreateUserImageAsync(IWebHostEnvironment environment, IFormFile file);
        Task<string> CreateUserImageAsync(IWebHostEnvironment environment, string imageNamePrefix, IFormFile file);
        bool DeleteUserImage(IWebHostEnvironment environment, string imageName);
        bool LogsFolderExists();
        bool AddLogsFolder();
        bool EmailLogsFolderExists();
        bool AddEmailLogsFolder();
        bool PrepareToRun(IWebHostEnvironment environment);
    }
}
