using Microsoft.AspNetCore.Http.Internal;
using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Core.Helpers;
using myPace.Core.Model;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using myPace.ViewModels;
using Newtonsoft.Json;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace myPace.Shared.Helpers
{
    public static class FileHelpers
    {
        public static string GetFileType(string format)
        {
            format = format.ToLower();
            if (format == ".jpg" || format == ".jpeg" || format == ".png" || format == ".gif")
            {
                return "image";
            }
            else if (format == ".webm" || format == ".mp4" || format == ".mpg" || format == ".MPEG")
            {
                return "video";
            }
            else if (format == ".mp3" || format == ".m4a" || format == ".aac" || format == ".oga")
            {
                return "audio";
            }
            else
            {
                return "doc";
            }
        }
        public static async Task<string> UplaodFileAsync(IHttpClientService httpClientService, BaseEntity entity, string controller, FileData fileToUpload, string fileDescription ="")
        {
            MessageDialog messageDialog = new MessageDialog("");
            try
            {
                var fileData = fileToUpload;

                using (Stream stream = fileData.GetStream())
                {
                    ContentDispositionHeaderValue header = new ContentDispositionHeaderValue("form-data")
                    {
                        FileName = fileData.FilePath,
                        Name = fileData.FileName
                    };
                    header.ToString();
                    using (var memoryStream = new MemoryStream())
                    {

                        stream.CopyTo(memoryStream);
                        var formFile = new FormFile(memoryStream, 0, memoryStream.Length, fileData.FileName, fileData.FilePath);

                        var format = Path.GetExtension(fileData.FilePath);
                        var fileModel = new UploadFile()
                        {
                            Name = fileData.FileName,
                            File = formFile,
                            Type = GetFileType(format),
                            ContentType = $"{GetFileType(format)}/{format.Remove(0, 1)}",
                            ContentDisposition = header.ToString(),
                            Description = fileDescription,
                            SaveTo = controller
                        };
                        var result = await httpClientService.PostFileAsync(fileModel, $"{controller}/uploadfile/{entity.Id}");
                        if (result.IsSuccessStatusCode)
                        {
                            var mediaResponse = JsonConvert.DeserializeObject<Media>(await result.Content.ReadAsStringAsync());
                            var path = await httpClientService.GetBasicAsync($"{controller}/geturi?id={entity.Id}&mediaId={mediaResponse.Id}");
                            return path;
                        }
                        throw new Exception("Error while uploading file:" + await result.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while uploading file: " + ex.Message);
            }
        }
       
        public static async Task<string> GetFilePath(IHttpClientService httpClientService,string controller, BaseEntity entity, Media media)
        {
            var path = await httpClientService.GetBasicAsync($"{controller}/geturi?id={entity?.Id}&mediaId={media?.Id}");
            return path;
        }
        public static void ProggressBarVisible(bool visible, StackPanel panel, ProgressRing progress)
        {
            panel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            progress.IsActive = visible;
        }
        public static IEnumerable<GenderEnum> GetGenderEnums()
        {
            return Enum.GetValues(typeof(GenderEnum)).Cast<GenderEnum>();
        }
        public static async Task<ObservableCollection<SchoolDto>> GetSchoolDtos(IHttpClientService httpClientService)
        {
            var schoolsDto = new ObservableCollection<SchoolDto>();
            var getSchools = await httpClientService.GetListTAsync<School>("school/getall");
            foreach (var school in getSchools)
            {
                schoolsDto.Add(new SchoolDto(school));
            }
            return schoolsDto;
        }
        
    }
}
