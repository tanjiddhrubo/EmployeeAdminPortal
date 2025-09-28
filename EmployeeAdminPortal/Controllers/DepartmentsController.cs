using AutoMapper;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.API.Models.Entities; // Corrected entity namespace
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminPortal.Controllers
{
    // ?? NO Class-Level Authorize ??: We apply specific roles/authorization per method
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;

        public DepartmentsController(IDepartmentRepository departmentRepository, IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _mapper = mapper;
        }

        // ?? Authorization for READ (GET) ??: Allows any authenticated user (Admin OR User)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllDepartments()
        {
            var departmentEntities = await _departmentRepository.GetAllAsync();
            var departmentDtos = _mapper.Map<List<DepartmentDto>>(departmentEntities);
            return Ok(departmentDtos);
        }

        // ?? Authorization for READ by ID (GET) ??: Allows any authenticated user
        [HttpGet]
        [Route("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetDepartmentById(Guid id)
        {
            var departmentEntity = await _departmentRepository.GetByIdAsync(id);

            if (departmentEntity is null)
            {
                return NotFound();
            }

            var departmentDto = _mapper.Map<DepartmentDto>(departmentEntity);
            return Ok(departmentDto);
        }

        // ?? Authorization for WRITE (POST) ??: Only Admins can add
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddDepartment(AddDepartmentDto addDepartmentDto)
        {
            var departmentEntity = _mapper.Map<Department>(addDepartmentDto);
            var newDepartment = await _departmentRepository.AddAsync(departmentEntity);
            var newDepartmentDto = _mapper.Map<DepartmentDto>(newDepartment);

            return CreatedAtAction(nameof(GetDepartmentById), new { id = newDepartmentDto.Id }, newDepartmentDto);
        }

        // ?? Authorization for WRITE (PUT) ??: Only Admins can update
        [HttpPut]
        [Route("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDepartment([FromRoute] Guid id, UpdateDepartmentDto updateDepartmentDto)
        {
            var departmentEntity = _mapper.Map<Department>(updateDepartmentDto);
            var updatedDepartment = await _departmentRepository.UpdateAsync(id, departmentEntity);

            if (updatedDepartment is null)
            {
                return NotFound();
            }

            var updatedDepartmentDto = _mapper.Map<DepartmentDto>(updatedDepartment);
            return Ok(updatedDepartmentDto);
        }

        // ?? Authorization for WRITE (DELETE) ??: Only Admins can delete
        [HttpDelete]
        [Route("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var department = await _departmentRepository.DeleteAsync(id);

            if (department is null)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}