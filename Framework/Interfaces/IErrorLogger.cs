using System;
using System.Threading.Tasks;

namespace Framework.Interfaces
{
    public interface IErrorLogger
    {
        void SaveError(string errorMessage, string area, string controller, string action);
        Task SaveErrorAsync(string errorMessage, string area, string controller, string action);
        void SaveError(Exception e, string area, string controller, string action);
        Task SaveErrorAsync(Exception e, string area, string controller, string action);
    }
}
