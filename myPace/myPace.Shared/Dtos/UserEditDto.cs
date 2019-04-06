using myPace.Core.Dtos;
using myPace.Core.Entities;
using myPace.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace myPace.Shared.Dtos
{
    public class UserEditDto
    {
        public TypeEnum Type { get; set; }
        public UserAuthDto ConnectedUser { get; set; }
        public bool? IsNew { get; set; }
        public BaseEntity Entity { get; set; }
        public UserEditDto(BaseEntity entity)
        {
            Entity = entity;
        }
    }
}
