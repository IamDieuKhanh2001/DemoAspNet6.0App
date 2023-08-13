using DemoAspNetApp.Data;
using DemoAspNetApp.Models;
using DemoAspNetApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DemoAspNetApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiController : ControllerBase
    {
        private readonly ILoaiRepository _loaiRepository;

        public LoaiController(ILoaiRepository loaiRepository)
        {
            _loaiRepository = loaiRepository;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                return Ok(_loaiRepository.GetAll());
            }catch
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var data = _loaiRepository.GetById(id);
                if(data != null)
                {
                    return Ok(data);
                }else
                {
                    return StatusCode(StatusCodes.Status404NotFound);
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateNew(LoaiModel loaiModel)
        {
            try
            {
                return Ok(_loaiRepository.Add(loaiModel));
            }
            catch(Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult UpdateById(int id, LoaiVM loai)
        {
            if(id != loai.MaLoai)
            {
                return BadRequest("Loai id not correct");
            }
            try
            {
                _loaiRepository.Update(loai);
                return NoContent();
            }
            catch
            {
                 return StatusCode(StatusCodes.Status400BadRequest);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult DeleteById(int id)
        {
            try
            {
                _loaiRepository.Delete(id);
                return Ok();
            }
            catch
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
        }
    }
}
