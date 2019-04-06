using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace myPace.Shared.Helpers
{
    public static class DtoHelper
    {
        public static async Task<BaseEntityDto<T>> LoadDto<T>(T entity, IHttpClientService httpClientService,string controllerName) where T:BasePersonEntity
        {
            var getMedia = await httpClientService.GetListTAsync<Media>($"{controllerName}/getmedia?id={entity.Id}");
            BaseEntityDto<T> entityDto = new BaseEntityDto<T>(entity);
            var latestProfileMedia = getMedia?.Where(x => x.Type == Core.Helper.MediaTypeEnum.Photo).OrderByDescending(x => x.UploadDate).FirstOrDefault();
            var path = await FileHelpers.GetFilePath(httpClientService, controllerName, entity, latestProfileMedia);
            entityDto.ProfileImage = path == null ? null : new BitmapImage(new Uri(path ?? ""));
            entityDto.Age = (DateTime.Now - entity.DOB.DateTime).Days / 365;
            return entityDto;
        }
        public static async Task<BaseEntityDto<T>> LoadDtoForBase<T>(T entity, IHttpClientService httpClientService, string controllerName) where T : BaseEntity
        {
            var getMedia = await httpClientService.GetListTAsync<Media>($"{controllerName}/getmedia?id={entity.Id}");
            BaseEntityDto<T> entityDto = new BaseEntityDto<T>(entity);
            var latestProfileMedia = getMedia?.Where(x => x.Type == Core.Helper.MediaTypeEnum.Photo).OrderByDescending(x => x.UploadDate).FirstOrDefault();
            var path = await FileHelpers.GetFilePath(httpClientService, controllerName, entity, latestProfileMedia);
            entityDto.ProfileImage = path == null ? null : new BitmapImage(new Uri(path ?? ""));
            return entityDto;
        }
        public static IEnumerable<Project> MapProjectsDtoToProjects(List<ProjectDto> projectDtos)
        {
            foreach(var projectDto in projectDtos)
            {
                yield return projectDto.Project;
            }
        }
        public static IEnumerable<ProjectDto> MapProjectsToProjectDto(List<Project> projects)
        {
            foreach (var project in projects)
            {
                yield return new ProjectDto(project);
            }
        }
        public static IEnumerable<BaseEntityDto<T>> MapBaseEntitiesToBaseEntitieDtos<T>(List<T> entities) where T : BaseEntity
        {
            foreach (var entity in entities)
            {
                yield return new BaseEntityDto<T>(entity);
            }
        }
        public static IEnumerable<T> MapBaseEntityDtosToBaseEntities<T>(List<BaseEntityDto<T>> entityDtos) where T : BaseEntity
        {
            foreach (var entity in entityDtos)
            {
                yield return entity.Entity;
            }
        }
        public static IEnumerable<BaseEntityMainDto<T>> MapBaseEntityToBaseEntityMainDto<T>(List<BaseEntityDto<T>> entityDtos) where T : BaseEntity
        {
            foreach (var entity in entityDtos)
            {
                yield return new BaseEntityMainDto<T>(entity.Entity);
            }
        }
        public static IEnumerable<BaseEntityDto<T>> MapBaseEntityMainDtoToBaseEntityDto<T>(List<BaseEntityMainDto<T>> entityDtos) where T : BaseEntity
        {
            foreach (var entity in entityDtos)
            {
                yield return new BaseEntityDto<T>(entity.Entity);
            }
        }

        public static IEnumerable<BaseEntityDto<T>> LoadEntityWithImages<T>(IEnumerable<BaseEntityMainDto<T>> entitesToLoad, IEnumerable<BaseEntityDto<T>> allEntities) where T : BaseEntity
        {
            foreach(var entity in entitesToLoad)
            {
                foreach(var item in allEntities)
                {
                    if(entity.Entity.Id == item.Entity.Id)
                    {
                        yield return item;
                        break;
                    }
                }
            }
        }
    }
}
