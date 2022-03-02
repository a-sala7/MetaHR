using AutoMapper;
using Models.DTOs;
using DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Commands.JobPostings;

namespace Business.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, ApplicationUserDTO>();
            CreateMap<JobPosting, JobPostingDTO>();

            CreateMap<AddJobPostingCommand, JobPosting>();
            CreateMap<UpdateJobPostingCommand, JobPosting>();
        }
    }
}
