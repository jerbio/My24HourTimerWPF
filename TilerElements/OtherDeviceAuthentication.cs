using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using TilerElements;

namespace TilerElements
{
    public class OtherDeviceAuthentication
    {
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public TilerUser User{ get; set; }
        [Key]
        public string ClientId  { get; set; }
        public string Secret{ get; set; }
        public long Start { get; set; } = DateTime.UtcNow.toJSMilliseconds();
        public long Expiration{ get; set; }
        public bool IsActive { get; set; } = false;
        public string Device { get; set; }
        public string DeviceId { get; set; }

        public static string generateClientSecret ()
        {
            RandomNumberGenerator cryptoRandomDataGenerator = new RNGCryptoServiceProvider();
            byte[] buffer = new byte[512];
            cryptoRandomDataGenerator.GetBytes(buffer);
            string uniq = Convert.ToBase64String(buffer);
            return uniq;
        }

        public static string encryptSecret (string input)
        {
            string uniqueSalt = "TilerIsBeingSafe###$$$$ButIfYouTryToGuessYouWillBeDestroyed";
            input += uniqueSalt;
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);

            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)

            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();

            
        }
    }
}