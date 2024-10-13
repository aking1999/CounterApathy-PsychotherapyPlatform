using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Framework.Helpers
{
    public static class Helper
    {
        /// <summary>
        /// Generates unique ID with numbers only with length of 12 digits.
        /// </summary>
        /// <returns></returns>
        public static string GenerateNumbersId()
        {
            //BigInteger l_retval = 0;
            //byte[] ba = Guid.NewGuid().ToByteArray();
            //int i = ba.Count();
            //foreach (byte b in ba)
            //{
            //    l_retval += b * BigInteger.Pow(256, --i);
            //}

            //return l_retval;

            return string.Concat(BigInteger.Abs(BigInteger.Parse(Guid.NewGuid().ToString().Replace("-", ""), NumberStyles.AllowHexSpecifier)).ToString().Take(12));
        }

        public static string[] GetObjectIconAndNameAndColor(string iconAndNameAndColorConcatenated)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(iconAndNameAndColorConcatenated))
                {
                    //array[0] -> fa icon
                    //array[1] -> name
                    //array[2] -> color
                    return iconAndNameAndColorConcatenated.Split('|');
                }
                return new string[] { "", "", "" };
            }
            catch (Exception)
            {
                return new string[] { "", "", "" };
            }
        }

        public static string[] GetObjectIconAndNameAndColorAndDataOption(string iconAndNameAndColorConcatenated)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(iconAndNameAndColorConcatenated))
                {
                    //array[0] -> fa icon
                    //array[1] -> name
                    //array[2] -> color
                    //array[3] -> data-option
                    return iconAndNameAndColorConcatenated.Split('|');
                }
                return new string[] { "", "", "", "" };
            }
            catch (Exception)
            {
                return new string[] { "", "", "", "" };
            }
        }

        public static T GetAttributeFrom<T>(this object instance, string propertyName) where T : Attribute
        {
            var attrType = typeof(T);
            var property = instance.GetType().GetProperty(propertyName);
            return (T)property.GetCustomAttributes(attrType, false).FirstOrDefault();
        }

        public static string CombinePaths(string path1, string path2)
        {
            if (Path.IsPathRooted(path2))
            {
                path2 = path2.TrimStart(Path.DirectorySeparatorChar);
                path2 = path2.TrimStart(Path.AltDirectorySeparatorChar);
            }

            return Path.Combine(path1, path2);
        }
    }
}
