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
using Models.Commands.Employees;
using Models.Commands.Notes;
using Models.Commands.Attendances;
using Models.Commands.JobApplications;

namespace Business.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<JobPosting, JobPostingDTO>();
            CreateMap<Department, DepartmentDTO>();
            CreateMap<Attendance, AttendanceDTO>();
            CreateMap<JobApplicationNote, JobApplicationNoteDTO>();

            CreateMap<CreateJobPostingCommand, JobPosting>();
            CreateMap<UpdateJobPostingCommand, JobPosting>();
            CreateMap<CreateDepartmentCommand, Department>();
            CreateMap<UpdateDepartmentCommand, Department>();
            CreateMap<CreateEmployeeCommand, Employee>();
            CreateMap<UpdateEmployeeCommand, Employee>();
            CreateMap<CreateEmployeeNoteCommand, EmployeeNote>();
            CreateMap<CreateAttendanceCommand, Attendance>();
            CreateMap<CreateJobApplicationCommand, JobApplication>();
            CreateMap<CreateJobApplicationNoteCommand, JobApplicationNote>();
        }
    }
}
