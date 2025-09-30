using EmployeeAdminPortal.Models; // Assuming DepartmentDto and DesignationDto are here
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires a valid JWT token
    public class LookupController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IDesignationRepository _designationRepository;
        private readonly IMapper _mapper;

        public LookupController(IDepartmentRepository departmentRepository, IDesignationRepository designationRepository, IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _mapper = mapper;
        }

        // GET: api/lookup/departments
        [HttpGet]
        [Route("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _departmentRepository.GetAllAsync();
            // Map the Entity objects to the DTOs
            var departmentDtos = _mapper.Map<List<DepartmentDto>>(departments);

            return Ok(departmentDtos);
        }

        // GET: api/lookup/designations
        [HttpGet]
        [Route("designations")]
        public async Task<IActionResult> GetDesignations()
        {
            var designations = await _designationRepository.GetAllAsync();
            // Map the Entity objects to the DTOs
            var designationDtos = _mapper.Map<List<DesignationDto>>(designations);

            return Ok(designationDtos);
        }
    }
}