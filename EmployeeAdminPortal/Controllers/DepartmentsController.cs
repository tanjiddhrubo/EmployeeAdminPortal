using AutoMapper;
using EmployeeAdminPortal.API.Models.Entities;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminPortal.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> GetAllDepartments()
        {
            var departmentEntities = await _departmentRepository.GetAllAsync();
            var departmentDtos = _mapper.Map<List<DepartmentDto>>(departmentEntities);
            return Ok(departmentDtos);
        }

        [HttpGet]
        [Route("{id:guid}")]
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

        [HttpPost]
        public async Task<IActionResult> AddDepartment(AddDepartmentDto addDepartmentDto)
        {
            var departmentEntity = _mapper.Map<Department>(addDepartmentDto);
            var newDepartment = await _departmentRepository.AddAsync(departmentEntity);
            var newDepartmentDto = _mapper.Map<DepartmentDto>(newDepartment);

            return CreatedAtAction(nameof(GetDepartmentById), new { id = newDepartmentDto.Id }, newDepartmentDto);
        }

        [HttpPut]
        [Route("{id:guid}")]
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

        [HttpDelete]
        [Route("{id:guid}")]
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