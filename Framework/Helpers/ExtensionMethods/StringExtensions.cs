using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.Helpers.ExtensionMethods
{
    public enum AllowedSpecialCharacters
    {
        None,
        ForEmail,
        ForAbout,
        ForAddress
    }

    public static class StringExtensions
    {
        public static string TakeMax(this string str, int count)
        {
            return !string.IsNullOrWhiteSpace(str) ? string.Concat(str?.Take(count)) : str;
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            string allowedCharacters = "#.-' ";
            StringBuilder sb = new StringBuilder();
            foreach (char c in !string.IsNullOrWhiteSpace(str) ? str.Trim() : string.Empty)
            {
                if (char.IsLetterOrDigit(c) || allowedCharacters.Contains(c))
                    sb.Append(c);
            }

            return sb.ToString().Any(c => char.IsLetterOrDigit(c)) ? sb.ToString() : string.Empty;
        }

        public static string RemoveSpecialCharacters(this string str, AllowedSpecialCharacters allowedType)
        {
            string allowedCharacters = string.Empty;

            switch (allowedType)
            {
                case AllowedSpecialCharacters.ForAbout:
                    {
                        allowedCharacters = ".,:;\"-()!?*&'/| ";
                        break;
                    }
                case AllowedSpecialCharacters.ForAddress:
                    {
                        allowedCharacters = ".,#-()&'/ ";
                        break;
                    }
                case AllowedSpecialCharacters.ForEmail:
                    {
                        allowedCharacters = "@.-_";
                        break;
                    }
                case AllowedSpecialCharacters.None:
                    {
                        allowedCharacters = string.Empty;
                        break;
                    }
                default:
                    {
                        allowedCharacters = ".-' ";
                        break;
                    }
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in !string.IsNullOrWhiteSpace(str) ? str.Trim() : string.Empty)
            {
                if (char.IsLetterOrDigit(c) || allowedCharacters.Contains(c))
                    sb.Append(c);
            }

            if(allowedType == AllowedSpecialCharacters.ForEmail) return sb.ToString().Any(c => char.IsLetterOrDigit(c)) ? sb.ToString().ToLower() : null;
            return sb.ToString().Any(c => char.IsLetterOrDigit(c)) ? sb.ToString() : null;
        }

        public static bool IsAnonymousOrUnauthorized(this string str)
        {
            return str.ToLower() == "anonymous" || str.ToLower() == "unauthorized";
        }
    }
}
