using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Helpers.ExtensionMethods
{
    public static class ExceptionExtensions
    {
        public static Exception GetRootException(this Exception e)
        {
            return e.InnerException == null ? e : e.InnerException.GetRootException();
        }
    }
}
