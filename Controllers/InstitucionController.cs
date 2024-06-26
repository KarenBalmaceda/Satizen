﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Proyec_Satizen_Api.Datos;
using Proyec_Satizen_Api.Models;
using Proyec_Satizen_Api.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace Proyec_Satizen_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstitucionController : ControllerBase
    {
        private readonly ILogger<InstitucionController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public InstitucionController (ILogger<InstitucionController> logger, ApplicationDbContext db, IMapper mapper)
        {
            _logger = logger;
            _db = db;
            _mapper = mapper;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InstitucionDto>>> GetInstitucionModels()
        {
            _logger.LogInformation("Obtener las Instituciones");

            IEnumerable<InstitucionModels> institucionList = await _db.Institucions.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<InstitucionDto>>(institucionList));
        }

        [HttpGet("id:int", Name ="GetInstitucion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InstitucionDto>> GetInstitucion(int id)
        {
            if (id == 0)
            {
                _logger.LogError("Error al traer Institucion con Id" + id);
                return BadRequest();
            }
            //var institucion = InstitucionStore.institucionList.FirstOrDefault(v => v.idInstitucion == id);
            var institucion = await _db.Institucions.FirstOrDefaultAsync(v => v.idInstitucion == id);

            if (institucion == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<InstitucionDto>(institucion));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<InstitucionDto>> CrearInstitucion([FromBody] InstitucionCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (await _db.Institucions.FirstOrDefaultAsync(v => v.nombreInstitucion.ToLower() == createDto.nombreInstitucion.ToLower()) != null)
            {
                ModelState.AddModelError("NombreExiste", "La institucion ya existe");
                return BadRequest(ModelState);
            }
            if (createDto == null)
            {
                return BadRequest(createDto);
            }
            InstitucionModels models = _mapper.Map<InstitucionModels>(createDto);
           
           await _db.Institucions.AddAsync(models);
            await _db.SaveChangesAsync();

            return CreatedAtRoute("GetInstitucion", new { idInstucion = models.idInstitucion }, models);

        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> DeleteInstitucion(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var institucion = await _db.Institucions.FirstOrDefaultAsync(v =>v.idInstitucion==id);
            if (institucion == null)
            {
                return NotFound();
            }
            _db.Institucions.Remove(institucion);
           await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<IActionResult> UpdateInstitucion(int id, [FromBody] InstitucionUpdateDto updateDto)
        {
            if (updateDto==null || id != updateDto.idInstitucion)
            {
                return BadRequest();
            }
            //var institucion = InstitucionStore.institucionList.FirstOrDefault(v => v.idInstitucion == id);
            //institucion.nombreInstitucion = institucionDto.nombreInstitucion;
            //institucion.direccionInstitucion = institucionDto.direccionInstitucion;
            //institucion.telefonoInstitucion = institucionDto.telefonoInstitucion;
            //institucion.correoInstitucion = institucionDto.correoInstitucion;
            //institucion.celularInstitucion = institucionDto.celularInstitucion;

            InstitucionModels models = _mapper.Map<InstitucionModels>(updateDto);

            _db.Institucions.Update(models);
            await _db.SaveChangesAsync();

            return NoContent();
        }


        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task< IActionResult> UpdatePartialInstitucion(int id, JsonPatchDocument<InstitucionUpdateDto> patchDto)
        {
            if (patchDto == null || id != 0)
            {
                return BadRequest();
            }
            var institucion = await _db.Institucions.AsNoTracking().FirstOrDefaultAsync(v => v.idInstitucion == id);

            InstitucionUpdateDto institucionDto = _mapper.Map<InstitucionUpdateDto>(institucion);

            if (institucion == null)
                return BadRequest();

            patchDto.ApplyTo(institucionDto, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            InstitucionModels models = _mapper.Map<InstitucionModels>(institucionDto);

            _db.Institucions.Update(models);
            await _db.SaveChangesAsync();
            return NoContent();
        }


    }
}

