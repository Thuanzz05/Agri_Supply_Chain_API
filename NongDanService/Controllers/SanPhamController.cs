using Microsoft.AspNetCore.Mvc;
using NongDanService.Models.DTOs;
using NongDanService.Services;

namespace NongDanService.Controllers
{
    [ApiController]
    [Route("api/san-pham")]
    public class SanPhamController : ControllerBase
    {
        private readonly ISanPhamService _service;

        public SanPhamController(ISanPhamService service)
        {
            _service = service;
        }

        // GET: /api/san-pham/get-all
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAll();
            return Ok(data);
        }

        // GET: /api/san-pham/get-by-id/5
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetById(id);
            if (data == null)
                return NotFound();

            return Ok(data);
        }

        // POST: /api/san-pham/create
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] SanPhamCreateDTO dto)
        {
            var id = await _service.Create(dto);
            return Ok(new
            {
                Message = "Tạo sản phẩm thành công",
                MaSanPham = id
            });
        }

        // PUT: /api/san-pham/update/5
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SanPhamUpdateDTO dto)
        {
            var result = await _service.Update(id, dto);
            if (!result)
                return NotFound();

            return Ok("Cập nhật sản phẩm thành công");
        }

        // DELETE: /api/san-pham/delete/5
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            if (!result)
                return NotFound();

            return Ok("Xóa sản phẩm thành công");
        }
    }
}
