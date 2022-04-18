using AutoMapper;
using Models.DTOs;
using DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Commands.JobPostings;
using Models.Commands.Departments;

namespace Business.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, ApplicationUserDTO>();
            CreateMap<JobPosting, JobPostingDTO>();
            CreateMap<Department, DepartmentDTO>();

            CreateMap<CreateJobPostingCommand, JobPosting>();
            CreateMap<UpdateJobPostingCommand, JobPosting>();
            CreateMap<CreateDepartmentCommand, Department>();
            CreateMap<UpdateDepartmentCommand, Department>();
        }
    }
}
