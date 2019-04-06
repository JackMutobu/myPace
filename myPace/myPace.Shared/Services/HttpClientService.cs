using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Core.Helpers;
using myPace.Core.Model;
using myPace.Interfaces;
using myPace.Shared.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace myPace.Services
{
    public class HttpClientService:IHttpClientService
    {
        private HttpClient _httpClient;
        private HttpClientHandler _httpClientHandler;

        public HttpClientService()
        {
#if __ANDROID__

            _httpClient = new HttpClient();
#else
             _httpClientHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };
             _httpClient = new HttpClient(_httpClientHandler);
#endif
            var localHost = "https://localhost:44346/api/";
            var azureOnline = "https://mypaceapi.azurewebsites.net/api/";
            var onlineHost = "http://afrisofttech-001-site12.btempurl.com/api/";
            _httpClient.BaseAddress = new Uri(onlineHost);
        }
        public HttpClient HttpClient
        {
            get { return _httpClient; }
            set { _httpClient = value; }
        }
        public  void SetAuthorizationHeaderToken(HttpClient client,string token)
        {
            client.DefaultRequestHeaders.Authorization = new  AuthenticationHeaderValue("Bearer",token);
        }
        public async Task<string> GetDataAsync()
        {
            try
            {
                var result = await _httpClient.GetStringAsync("users/getall");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> GetBasicAsync(string url)
        {
            try
            {
                var result = await _httpClient.GetStringAsync(url);
                return result;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
        public async Task<UserAuthDto> AuthenticateUser(UserDto userDto)
        {
            var data = JsonConvert.SerializeObject(userDto);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("users/authenticate", content);
            var result = JsonConvert.DeserializeObject<UserAuthDto>(response.Content.ReadAsStringAsync().Result);
            if(response.IsSuccessStatusCode)
            {
                return result;
            }
            throw new AppException(response.Content.ReadAsStringAsync().Result);
        }
        public async Task<HttpResponseMessage> PostBasicAsync(object content, string uri)
        {
            HttpResponseMessage reslult = default;
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                {
                    var json = JsonConvert.SerializeObject(content);
                    using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        request.Content = stringContent;
                        var response = await _httpClient
                            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                            .ConfigureAwait(false);
                        reslult = response;
                        response.EnsureSuccessStatusCode();
                        
                        return response;
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
        }
        public async Task<HttpResponseMessage> PutBasicAsync(object content, string uri)
        {
            var jsonContent = JsonConvert.SerializeObject(content);
            HttpContent httpContent = new StringContent(jsonContent);
            httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var response = await _httpClient.PutAsync(uri, httpContent);
            return response;
        }
        public async Task<HttpResponseMessage> DeleteAsync(string uri)
        {
            var response = await _httpClient.DeleteAsync(uri);
            return response;
        }
        public async Task<HttpResponseMessage> PostFileAsync(UploadFile model,string url)
        {
            byte[] data;
            if(model.File == null)
            {
                throw new ArgumentNullException(nameof(model),"File is null");
            }
            using (var br = new BinaryReader(model.File.OpenReadStream()))
                data = br.ReadBytes((int)model.File.OpenReadStream().Length);
            ByteArrayContent bytes = new ByteArrayContent(data);
            MultipartFormDataContent multiContent = new MultipartFormDataContent
            {
                { new StringContent(model.Name), "Name" },
                { new StringContent(model.Type), "Type" },
                { new StringContent(model.SaveTo), "SaveTo" },
                { new StringContent(model.Description),"Description" }
            };
            multiContent.Add(new StreamContent(model.File.OpenReadStream())
            {
                Headers =
                    {
                        ContentLength = model.File.Length,
                        ContentType = new MediaTypeHeaderValue(model.ContentType)
                    }
            },"File", model.File.FileName.Trim('"'));
            var response = await _httpClient.PostAsync(url, multiContent);
            return response;
        }
        public async Task<Stream> GetFileFromAsync(string url)
        {
            var response = await _httpClient.GetStreamAsync(url);
            return response;
        }
        public async Task<T> GetTAsync<T>(string url) where T: BaseEntity
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                var result = await JsonReaderHelper.DeserializeObjectFromStreamAsync<T>(response);
                return result;
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message);
                return null;
            }
        }
        public async Task<List<T>> GetListTAsync<T>(string url) where T : BaseEntity
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                var result = await JsonReaderHelper.DeserializeObjectFromStreamAsync<List<T>>(response);
                return result;
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.Message);
                return null;
            }
        }
    }
}
    
