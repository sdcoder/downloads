using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace LightStreamWeb.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsHmacValid(this object item, string hmac, string hmacKey)
        {
            string serializedItem = JsonConvert.SerializeObject(item);
            byte[] itemBytes = new byte[serializedItem.Length * sizeof(char)];

            Buffer.BlockCopy(serializedItem.ToCharArray(), 0, itemBytes, 0, itemBytes.Length);

            using (var hmac256 = new HMACSHA256(Encoding.UTF8.GetBytes(hmacKey)))
            {
                var computedHash = hmac256.ComputeHash(itemBytes);
                var createSignature = BitConverter.ToString(computedHash).Replace("-", string.Empty).ToLower();

                return hmac == createSignature;
            }
        }
    }
}