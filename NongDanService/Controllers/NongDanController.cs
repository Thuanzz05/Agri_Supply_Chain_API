using Microsoft.AspNetCore.Mvc;
using NongDanService.Models.DTOs;
using NongDanService.Services;

namespace NongDanService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NongDanController : ControllerBase
    {
        private readonly INongDanService _nongDanService;

        public NongDanController(INongDanService nongDanService)
        {
            _nongDanService = nongDanService;
        }

        // GET: api/nongdan
        [HttpGet]
        public IActionResult GetAll()
        {
            var data = _nongDanService.GetAll();
            return Ok(data);
        }

        // GET: api/nongdan/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var data = _nongDanService.GetById(id);
            if (data == null)
                return NotFound();
            return Ok(data);
        }

        // POST: api/nongdan
        [HttpPost]
        public IActionResult Create(NongDanCreateDTO dto)
        {
            var newId = _nongDanService.Create(dto);
            return Ok(new { Id = newId });
        }

        // PUT: api/nongdan/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, NongDanUpdateDTO dto)
        {
            bool result = _nongDanService.Update(id, dto);
            if (!result)
                return NotFound();
            return Ok();
        }

        // DELETE: api/nongdan/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            bool result = _nongDanService.Delete(id);
            if (!result)
                return NotFound();
            return Ok();
        }
    }
}
