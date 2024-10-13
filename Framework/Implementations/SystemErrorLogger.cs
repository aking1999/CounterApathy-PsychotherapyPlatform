using Database.Models;
using Database.RepositoryImplementations;
using Framework.Helpers.ExtensionMethods;
using Framework.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Framework.Implementations
{
    public class SystemErrorLogger : ISystemErrorLogger
    {
        private readonly UnitOfWork _context;

        public SystemErrorLogger()
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        public void SaveError(string errorMessage, string project, string @class, string method)
        {
            throw new NotImplementedException();
        }

        public void SaveError(Exception e, string project, string @class, string method)
        {
            try
            {
                _context.ErrorLogs.Insert(new ErrorLogs
                {
                    Id = Guid.NewGuid().ToString(),
                    UserIdOrAnonymous = "System",
                    AreaOrProject = project?.TakeMax(64),
                    ControllerOrClass = @class?.TakeMax(64),
                    ActionOrMethod = method?.TakeMax(64),
                    Description = e.GetRootException()?.Message?.TakeMax(1024),
                    Source = e.Source?.TakeMax(512), //mozda treba ex.GetRootException().Source, isto i za ostale
                    StackTrace = e.StackTrace?.TakeMax(2048), //mozda treba ex.GetRootException().StackTrace, isto i za ostale
                    StackTraceFrameMethodName = new StackTrace(e).GetFrame(0)?.GetMethod()?.Name?.TakeMax(512),
                    StackTraceExecutingAssemblyName = new StackTrace(e).GetFrames()?
                                                                       .Select(f => f?.GetMethod())?
                                                                       .FirstOrDefault(m => m?.Module?.Assembly == Assembly.GetExecutingAssembly())?
                                                                       .Name?
                                                                       .TakeMax(512),
                    TargetSiteName = e.TargetSite?.Name?.TakeMax(512),
                    TargetSiteReflectedTypeFullName = e.TargetSite?.ReflectedType?.FullName?.TakeMax(512),
                    ErrorDateTime = DateTime.UtcNow,
                    Fixed = false
                });
            }
            catch (Exception ex)
            {
                _context.ErrorLogs.Insert(new ErrorLogs
                {
                    Id = Guid.NewGuid().ToString(),
                    UserIdOrAnonymous = "System",
                    AreaOrProject = "Framework",
                    ControllerOrClass = "SystemErrorLogger",
                    ActionOrMethod = "SaveError",
                    Description = ex.GetRootException()?.Message?.TakeMax(1024), //ovde mislim da GetRootException() ne treba da stoji jer je ovo vec Root (ex), ispitati kasnije.
                    Source = ex.Source?.TakeMax(512), //mozda treba ex.GetRootException().Source, isto i za ostale
                    StackTrace = e.StackTrace?.TakeMax(2048), //mozda treba ex.GetRootException().StackTrace, isto i za ostale
                    StackTraceFrameMethodName = new StackTrace(ex).GetFrame(0)?.GetMethod()?.Name?.TakeMax(512),
                    StackTraceExecutingAssemblyName = new StackTrace(ex).GetFrames()?
                                                                        .Select(f => f?.GetMethod())?
                                                                        .FirstOrDefault(m => m?.Module?.Assembly == Assembly.GetExecutingAssembly())?
                                                                        .Name?
                                                                        .TakeMax(512),
                    TargetSiteName = e.TargetSite?.Name?.TakeMax(512),
                    TargetSiteReflectedTypeFullName = e.TargetSite?.ReflectedType?.FullName?.TakeMax(512),
                    ErrorDateTime = DateTime.UtcNow,
                    Fixed = false
                });
            }
            finally
            {
                _context.Save();
            }
        }

        public async Task SaveErrorAsync(string errorMessage, string project, string @class, string method)
        {
            try
            {
                _context.ErrorLogs.Insert(new ErrorLogs
                {
                    Id = Guid.NewGuid().ToString(),
                    UserIdOrAnonymous = "System",
                    AreaOrProject = project?.TakeMax(64),
                    ControllerOrClass = @class?.TakeMax(64),
                    ActionOrMethod = method?.TakeMax(64),
                    Description = errorMessage?.TakeMax(1024),
                    ErrorDateTime = DateTime.UtcNow,
                    Fixed = false
                });
            }
            catch (Exception e)
            {
                _context.ErrorLogs.Insert(new ErrorLogs
                {
                    Id = Guid.NewGuid().ToString(),
                    UserIdOrAnonymous = "System",
                    AreaOrProject = "Framework",
                    ControllerOrClass = "SystemErrorLogger",
                    ActionOrMethod = "SaveErrorAsync",
                    Description = e.GetRootException()?.Message?.TakeMax(1024), //ovde mislim da GetRootException() ne treba da stoji jer je ovo vec Root (ex), ispitati kasnije.
                    Source = e.Source?.TakeMax(512), //mozda treba ex.GetRootException().Source, isto i za ostale
                    StackTrace = e.StackTrace?.TakeMax(2048), //mozda treba ex.GetRootException().StackTrace, isto i za ostale
                    StackTraceFrameMethodName = new StackTrace(e).GetFrame(0)?.GetMethod()?.Name?.TakeMax(512),
                    StackTraceExecutingAssemblyName = new StackTrace(e).GetFrames()?
                                                                       .Select(f => f?.GetMethod())?
                                                                       .FirstOrDefault(m => m?.Module?.Assembly == Assembly.GetExecutingAssembly())?
                                                                       .Name?
                                                                       .TakeMax(512),
                    TargetSiteName = e.TargetSite?.Name?.TakeMax(512),
                    TargetSiteReflectedTypeFullName = e.TargetSite?.ReflectedType?.FullName?.TakeMax(512),
                    ErrorDateTime = DateTime.UtcNow,
                    Fixed = false
                });
            }
            finally
            {
                await _context.SaveAsync();
            }
        }

        public async Task SaveErrorAsync(Exception e, string project, string @class, string method)
        {
            try
            {
                _context.ErrorLogs.Insert(new ErrorLogs
                {
                    Id = Guid.NewGuid().ToString(),
                    UserIdOrAnonymous = "System",
                    AreaOrProject = project?.TakeMax(64),
                    ControllerOrClass = @class?.TakeMax(64),
                    ActionOrMethod = method?.TakeMax(64),
                    Description = e.GetRootException()?.Message?.TakeMax(1024),
                    Source = e.Source?.TakeMax(512), //mozda treba ex.GetRootException().Source, isto i za ostale
                    StackTrace = e.StackTrace?.TakeMax(2048), //mozda treba ex.GetRootException().StackTrace, isto i za ostale
                    StackTraceFrameMethodName = new StackTrace(e).GetFrame(0)?.GetMethod()?.Name?.TakeMax(512),
                    StackTraceExecutingAssemblyName = new StackTrace(e).GetFrames()?
                                                                       .Select(f => f?.GetMethod())?
                                                                       .FirstOrDefault(m => m?.Module?.Assembly == Assembly.GetExecutingAssembly())?
                                                                       .Name?
                                                                       .TakeMax(512),
                    TargetSiteName = e.TargetSite?.Name?.TakeMax(512),
                    TargetSiteReflectedTypeFullName = e.TargetSite?.ReflectedType?.FullName?.TakeMax(512),
                    ErrorDateTime = DateTime.UtcNow,
                    Fixed = false
                });
            }
            catch (Exception ex)
            {
                _context.ErrorLogs.Insert(new ErrorLogs
                {
                    Id = Guid.NewGuid().ToString(),
                    UserIdOrAnonymous = "System",
                    AreaOrProject = "Framework",
                    ControllerOrClass = "SystemErrorLogger",
                    ActionOrMethod = "SaveErrorAsync",
                    Description = ex.GetRootException()?.Message?.TakeMax(1024), //ovde mislim da GetRootException() ne treba da stoji jer je ovo vec Root (ex), ispitati kasnije.
                    Source = ex.Source?.TakeMax(512), //mozda treba ex.GetRootException().Source, isto i za ostale
                    StackTrace = e.StackTrace?.TakeMax(2048), //mozda treba ex.GetRootException().StackTrace, isto i za ostale
                    StackTraceFrameMethodName = new StackTrace(ex).GetFrame(0)?.GetMethod()?.Name?.TakeMax(512),
                    StackTraceExecutingAssemblyName = new StackTrace(ex).GetFrames()?
                                                                        .Select(f => f?.GetMethod())?
                                                                        .FirstOrDefault(m => m?.Module?.Assembly == Assembly.GetExecutingAssembly())?
                                                                        .Name?
                                                                        .TakeMax(512),
                    TargetSiteName = e.TargetSite?.Name?.TakeMax(512),
                    TargetSiteReflectedTypeFullName = e.TargetSite?.ReflectedType?.FullName?.TakeMax(512),
                    ErrorDateTime = DateTime.UtcNow,
                    Fixed = false
                });
            }
            finally
            {
                await _context.SaveAsync();
            }
        }
    }
}
