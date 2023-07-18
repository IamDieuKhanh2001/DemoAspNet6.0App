using DemoAspNetApp.Models;
using DemoAspNetApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DemoAspNetApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IHangHoaRepository _hangHoaRepository;
        public ProductController(IHangHoaRepository hangHoaRepository)
        {
            _hangHoaRepository = hangHoaRepository;
        }

        [HttpGet]
        public IActionResult GetAllProducts(
            string? search, 
            double? from, 
            double? to, 
            string? sortBy,
            int? page = 1)
        {
            if(page == null)
            {
                page = 1;
            }
            try
            {
                int pageNumber = page.Value;

                var result = _hangHoaRepository.GetAll(search, from, to, sortBy, pageNumber);
                return Ok(result);
            }catch
            {
                return BadRequest("Can not get product list");
            }
        }

        [HttpPost]
        public IActionResult CreateNew(HangHoaModel model)
        {
            try
            {
                return Ok(_hangHoaRepository.Add(model));
            }
            catch (Exception ex)
            {
                return BadRequest("Can not add hang hoa");
            }
        }
    }
}
