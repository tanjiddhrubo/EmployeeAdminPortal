using AutoMapper;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.API.Models.Entities;
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminPortal.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;

        public EmployeesController(IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employeeEntities = await _employeeRepository.GetAllAsync();

            var employeeDtos = _mapper.Map<List<EmployeeDto>>(employeeEntities);

            return Ok(employeeDtos);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            var employeeEntity = await _employeeRepository.GetByIdAsync(id);

            if (employeeEntity is null)
            {
                return NotFound();
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employeeEntity);

            return Ok(employeeDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(AddEmployeeDto addEmployeeDto)
        {
            var employeeEntity = _mapper.Map<Employee>(addEmployeeDto);

            var newEmployee = await _employeeRepository.AddAsync(employeeEntity);

            var newEmployeeDto = _mapper.Map<EmployeeDto>(newEmployee);

            return CreatedAtAction(nameof(GetEmployeeById), new { id = newEmployeeDto.Id }, newEmployeeDto);
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> UpdateEmployee([FromRoute] Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            var employeeEntity = _mapper.Map<Employee>(updateEmployeeDto);

            var updatedEmployee = await _employeeRepository.UpdateAsync(id, employeeEntity);

            if (updatedEmployee is null)
            {
                return NotFound();
            }

            var updatedEmployeeDto = _mapper.Map<EmployeeDto>(updatedEmployee);

            return Ok(updatedEmployeeDto);
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var employee = await _employeeRepository.DeleteAsync(id);

            if (employee is null)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}