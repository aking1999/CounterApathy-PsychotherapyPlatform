using Framework.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework.Helpers.ExtensionMethods
{
    public static class SessionExtensionMethods
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default : JsonConvert.DeserializeObject<T>(value);
        }

        public static Toast GetToast(this ISession session)
        {
            var value = session.GetString("toast");

            return value == null ? default : JsonConvert.DeserializeObject<Toast>(value);
        }

        public static void SetToast(this ISession session, string header, string body, string severity)
        {
            session.SetObjectAsJson("toast", new Toast(header, body, severity));
        }

        public static bool HasToast(this ISession session)
        {
            return session.Keys.Contains("toast");
        }

        public static void RemoveToastFromKeys(this ISession session)
        {
            session.Remove("toast");
        }

        public static double? GetDouble(this ISession session, string key)
        {
            var doubleNumber = session.Get(key);
            if (doubleNumber == null) return null;
            return BitConverter.ToDouble(doubleNumber, 0);
        }

        public static void SetDouble(this ISession session, string key, double value)
        {
            session.Set(key, BitConverter.GetBytes(value));
        }

        public static bool GetBoolean(this ISession session, string key)
        {
            var data = session.Get(key);
            if (data == null)
            {
                return false;
            }
            return BitConverter.ToBoolean(data, 0);
        }

        public static void SetBoolean(this ISession session, string key, bool value)
        {
            session.Set(key, BitConverter.GetBytes(value));
        }
    }
}
