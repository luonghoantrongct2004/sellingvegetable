using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FruityFresh.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FruityFresh.Helper
{
    public static class Utilities
    {
        public static int PAGE_SIZE = 20;

        private static IWebHostEnvironment _webHostEnvironment;
        public static void InitialHostEnv(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public static void CreateIfMissing(string path)
        {
            bool folderExists = Directory.Exists(path);
            if (!folderExists)
            {
                Directory.CreateDirectory(path);
            }
        }
        public static string ToTitleCase(string str)
        {
            string result = str;
            if(!string.IsNullOrEmpty(result))
            {
                var words = str.Split(' ');
                for(int i = 0; i < words.Length; i++)
                {
                    var s = words[i];
                    if(s.Length > 0)
                    {
                        words[i] = s[0].ToString().ToUpper() + s.Substring(1);
                    }
                }
                result = string.Join(" ", words);
            }
            return result;
        }
        public static bool IsInteger(string str)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            try
            {
                if (String.IsNullOrWhiteSpace(str))
                {
                    return false;
                }
                if (!regex.IsMatch(str))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string GetRandomKey(int length = 5)
        {
            string pattern = @"123456789zxcvbnmasdfghjklqwertyuiop[]{}:~!@#$%^&*()+";
            Random rd = new Random();
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < length ; i++)
            {
                stringBuilder.Append(pattern[rd.Next(0,pattern.Length)]);
            }
            return stringBuilder.ToString();
        }
        public static string SEOUrl(string url)
        {
                var result = url.ToLower().Trim();
                result = Regex.Replace(result, "áàạảãâấầậẫăắằặẳẵ", "a");
                result = Regex.Replace(result, "éèẹẻẽêếềệểễ", "e");
                result = Regex.Replace(result, "óòọỏõôốồộổỗơớờợởỡ", "o");
                result = Regex.Replace(result, "úùụủũưứừựửữ", "u");
                result = Regex.Replace(result, "íìịỉĩ", "i");
                result = Regex.Replace(result, "ýỳỵỷỹ", "y");
                result = Regex.Replace(result, "đ", "d");
                result = Regex.Replace(result, "[^a-z0-9-]", "");
                result = Regex.Replace(result, "(-)+", "-");
                return result;
        }

        public static async Task<string> UploadFile(IFormFile fileThumb, string sDirectory, string newname, IWebHostEnvironment webHostEnvironment)
        {
            try
            {
                if (fileThumb != null && fileThumb.Length > 0)
                {
                    if (string.IsNullOrEmpty(newname)) newname = fileThumb.FileName;

                    // Đường dẫn vật lý trên máy chủ IIS ảo
                    string physicalPath = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot", "Images", sDirectory);

                    Directory.CreateDirectory(physicalPath);

                    string pathFile = Path.Combine(physicalPath, newname);

                    var supportedTypes = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var fileExt = Path.GetExtension(fileThumb.FileName).ToLower();

                    if (!supportedTypes.Contains(fileExt))
                    {
                        return null;
                    }

                    using (var stream = new FileStream(pathFile, FileMode.Create))
                    {
                        await fileThumb.CopyToAsync(stream);
                    }

                    string relativePath = Path.Combine("\\Images", sDirectory, newname); // Đường dẫn ảo cho tệp vừa lưu
                    return relativePath;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                return null;
            }
        }



    }
}
