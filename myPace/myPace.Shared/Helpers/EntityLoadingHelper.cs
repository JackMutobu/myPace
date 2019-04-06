using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Interfaces;
using myPace.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myPace.Shared.Helpers
{
    public static class EntityLoadingHelper
    {
        public static async Task<List<ProjectDto>> LoadProjectsAsync(List<Volunteer> volunteers, IHttpClientService httpClientService,List<ProjectDto> loadedProjects, UserAuthDto connectedUser = null)
        {
            var projects = new List<ProjectDto>();
            foreach (var volunteer in volunteers)
            {
                foreach (var volunteerProject in volunteer?.VolunteerProjects?.OrderByDescending(i => i.ProjectId))
                {
                    var project = await httpClientService.GetTAsync<Project>($"project/getbyid/{volunteerProject.ProjectId}");
                    if (loadedProjects?.Any(p => p?.Project?.Id == project?.Id) == true || projects?.Any(p => p?.Project?.Id == project?.Id) == true)
                        continue;
                    projects.Add(new ProjectDto(project)
                    {
                        ConnectedUser = connectedUser
                    });
                }
            }
            return projects;
        }
    }
}
