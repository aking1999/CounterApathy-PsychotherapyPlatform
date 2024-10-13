using System;
using System.Collections.Generic;

namespace Database.Models
{
    public partial class ErrorLogs
    {
        public string Id { get; set; }
        public string UserIdOrAnonymous { get; set; }
        public string AreaOrProject { get; set; }
        public string ControllerOrClass { get; set; }
        public string ActionOrMethod { get; set; }
        public string Description { get; set; }
        public DateTime? ErrorDateTime { get; set; }
        public string StackTraceFrameMethodName { get; set; }
        public string StackTraceExecutingAssemblyName { get; set; }
        public string TargetSiteName { get; set; }
        public string TargetSiteReflectedTypeFullName { get; set; }
        public string StackTrace { get; set; }
        public bool? Fixed { get; set; }
        public string Source { get; set; }
    }
}
