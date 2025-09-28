using AutoMapper;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.API.Models.Entities; // Corrected entity namespace
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization; // ?? NEW: Required for [Authorize]
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminPortal.Controllers
{
    // No class-level [Authorize] attribute here; we apply it per method
    [Route("api/[controller]")]
    [ApiController]
    public class DesignationsController : ControllerBase
    {
        private readonly IDesignationRepository _designationRepository;
        private readonly IMapper _mapper;

        public DesignationsController(IDesignationRepository designationRepository, IMapper mapper)
        {
            _designationRepository = designationRepository;
            _mapper = mapper;
        }

        // ?? Authorization for READ (GET) ??: Allows any authenticated user (Admin or User)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllDesignations()
        {
            var designationEntities = await _designationRepository.GetAllAsync();
            var designationDtos = _mapper.Map<List<DesignationDto>>(designationEntities);
            return Ok(designationDtos);
        }

        // ?? Authorization for READ by ID (GET) ??: Allows any authenticated user
        [HttpGet]
        [Route("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetDesignationById(Guid id)
        {
            var designationEntity = await _designationRepository.GetByIdAsync(id);

            if (designationEntity is null)
            {
                return NotFound();
            }

            var designationDto = _mapper.Map<DesignationDto>(designationEntity);
            return Ok(designationDto);
        }

        // ?? Authorization for WRITE (POST) ??: Only Admins can add
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddDesignation(AddDesignationDto addDesignationDto)
        {
            var designationEntity = _mapper.Map<Designation>(addDesignationDto);
            var newDesignation = await _designationRepository.AddAsync(designationEntity);
            var newDesignationDto = _mapper.Map<DesignationDto>(newDesignation);

            return CreatedAtAction(nameof(GetDesignationById), new { id = newDesignationDto.Id }, newDesignationDto);
        }

        // ?? Authorization for WRITE (PUT) ??: Only Admins can update
        [HttpPut]
        [Route("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDesignation([FromRoute] Guid id, UpdateDesignationDto updateDesignationDto)
        {
            var designationEntity = _mapper.Map<Designation>(updateDesignationDto);
            var updatedDesignation = await _designationRepository.UpdateAsync(id, designationEntity);

            if (updatedDesignation is null)
            {
                return NotFound();
            }

            var updatedDesignationDto = _mapper.Map<DesignationDto>(updatedDesignation);
            return Ok(updatedDesignationDto);
        }

        // ?? Authorization for WRITE (DELETE) ??: Only Admins can delete
        [HttpDelete]
        [Route("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDesignation(Guid id)
        {
            var designation = await _designationRepository.DeleteAsync(id);

            if (designation is null)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}