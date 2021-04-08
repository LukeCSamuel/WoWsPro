using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WoWsPro.Client.Utils
{
    public static class HttpExtensions {
        static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
            ReferenceHandler = ReferenceHandler.Preserve
        };

        public static Task<T> GetAsAsync<T> (this HttpClient http, string path)
        {
            return http.GetFromJsonAsync<T>(path, jsonOptions);
        }

        public static Task<HttpResponseMessage> PostAsAsync<T> (this HttpClient http, string path, T value)
        {
            return http.PostAsJsonAsync(path, value, jsonOptions);
        }

        public static Task<HttpResponseMessage> PutAsAsync<T> (this HttpClient http, string path, T value)
        {
            return http.PostAsJsonAsync(path, value, jsonOptions);
        }

        public static Task<T> ReadAsAsync<T> (this HttpContent content)
        {
            return content.ReadFromJsonAsync<T>(jsonOptions);
        }
    }
}