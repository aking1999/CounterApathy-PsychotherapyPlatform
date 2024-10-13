using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Interfaces
{
    public interface ISystemErrorLogger
    {
        void SaveError(string errorMessage, string project, string @class, string method);
        Task SaveErrorAsync(string errorMessage, string project, string @class, string method);
        void SaveError(Exception e, string project, string @class, string method);
        Task SaveErrorAsync(Exception e, string project, string @class, string method);
    }
}
