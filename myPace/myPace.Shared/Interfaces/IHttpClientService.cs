using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Core.Model;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;

namespace myPace.Interfaces
{
    public interface IHttpClientService
    {
        HttpClient HttpClient { get; set; }
        Task<string> GetDataAsync();
        Task<UserAuthDto> AuthenticateUser(UserDto userDto);
        void SetAuthorizationHeaderToken(HttpClient client, string token);
        Task<HttpResponseMessage> PostBasicAsync(object content, string uri);
        Task<HttpResponseMessage> PutBasicAsync(object content, string uri);
        Task<HttpResponseMessage> PostFileAsync(UploadFile model, string url);
        Task<HttpResponseMessage> DeleteAsync(string uri);
        Task<string> GetBasicAsync(string url);
        Task<Stream> GetFileFromAsync(string url);
        Task<T> GetTAsync<T>(string url) where T: BaseEntity;
        Task<List<T>> GetListTAsync<T>(string url) where T : BaseEntity;
    }
}
