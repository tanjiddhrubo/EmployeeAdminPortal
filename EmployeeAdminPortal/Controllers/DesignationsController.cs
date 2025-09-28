using AutoMapper;
using EmployeeAdminPortal.API.Models.Entities;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminPortal.Controllers
{
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

        [HttpGet]
        public async Task<IActionResult> GetAllDesignations()
        {
            var designationEntities = await _designationRepository.GetAllAsync();
            var designationDtos = _mapper.Map<List<DesignationDto>>(designationEntities);
            return Ok(designationDtos);
        }

        [HttpGet]
        [Route("{id:guid}")]
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

        [HttpPost]
        public async Task<IActionResult> AddDesignation(AddDesignationDto addDesignationDto)
        {
            var designationEntity = _mapper.Map<Designation>(addDesignationDto);
            var newDesignation = await _designationRepository.AddAsync(designationEntity);
            var newDesignationDto = _mapper.Map<DesignationDto>(newDesignation);

            return CreatedAtAction(nameof(GetDesignationById), new { id = newDesignationDto.Id }, newDesignationDto);
        }

        [HttpPut]
        [Route("{id:guid}")]
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

        [HttpDelete]
        [Route("{id:guid}")]
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