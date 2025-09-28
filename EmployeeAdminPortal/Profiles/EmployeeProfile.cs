using AutoMapper;
using EmployeeAdminPortal.API.Models.Entities;
using EmployeeAdminPortal.Models;

namespace EmployeeAdminPortal.Profiles
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<AddEmployeeDto, Employee>().ReverseMap();
            CreateMap<UpdateEmployeeDto, Employee>().ReverseMap();

            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.DepartmentName,
                           opt => opt.MapFrom(src => src.Department.Name))
                .ForMember(dest => dest.DesignationName,
                           opt => opt.MapFrom(src => src.Designation.Name))
                .ReverseMap();

            CreateMap<Department, DepartmentDto>().ReverseMap();
            CreateMap<AddDepartmentDto, Department>().ReverseMap();
            CreateMap<UpdateDepartmentDto, Department>().ReverseMap();

            CreateMap<Designation, DesignationDto>().ReverseMap();
            CreateMap<AddDesignationDto, Designation>().ReverseMap();
            CreateMap<UpdateDesignationDto, Designation>().ReverseMap();
        }
    }
}