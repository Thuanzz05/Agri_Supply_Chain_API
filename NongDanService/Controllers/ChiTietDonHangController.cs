using Microsoft.AspNetCore.Mvc;
using NongDanService.Models.DTOs;
using NongDanService.Services;

namespace NongDanService.Controllers
{
    [Route("api/chi-tiet-don-hang")]
    [ApiController]
    public class ChiTietDonHangController : ControllerBase
    {
        private readonly IChiTietDonHangService _service;

        public ChiTietDonHangController(IChiTietDonHangService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lấy chi tiết đơn hàng theo mã đơn hàng
        /// </summary>
        [HttpGet("get-by-don-hang/{maDonHang}")]
        public IActionResult GetByDonHangId(int maDonHang)
        {
            try
            {
                if (maDonHang <= 0)
                    return BadRequest(new { success = false, message = "Mã đơn hàng không hợp lệ" });

                var data = _service.GetByDonHangId(maDonHang);
                return Ok(new
                {
                    success = true,
                    message = "Lấy chi tiết đơn hàng thành công",
                    data,
                    count = data.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        /// <summary>
        /// Lấy chi tiết theo mã đơn hàng và mã lô
        /// </summary>
        [HttpGet("get-by-id/{maDonHang}/{maLo}")]
        public IActionResult GetById(int maDonHang, int maLo)
        {
            try
            {
                var data = _service.GetById(maDonHang, maLo);
                if (data == null)
                    return NotFound(new { success = false, message = "Không tìm thấy chi tiết đơn hàng" });

                return Ok(new { success = true, message = "Lấy chi tiết thành công", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        /// <summary>
        /// Thêm chi tiết vào đơn hàng (thêm lô sản phẩm)
        /// </summary>
        [HttpPost("create")]
        public IActionResult Create([FromBody] ChiTietDonHangCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

                var result = _service.Create(dto);
                if (!result)
                    return BadRequest(new { success = false, message = "Không thể thêm chi tiết đơn hàng" });

                return Ok(new { success = true, message = "Thêm chi tiết đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật chi tiết đơn hàng
        /// </summary>
        [HttpPut("update/{maDonHang}/{maLo}")]
        public IActionResult Update(int maDonHang, int maLo, [FromBody] ChiTietDonHangUpdateDTO dto)
        {
            try
            {
                var result = _service.Update(maDonHang, maLo, dto);
                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy chi tiết để cập nhật" });

                return Ok(new { success = true, message = "Cập nhật chi tiết thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }

        /// <summary>
        /// Xóa chi tiết khỏi đơn hàng
        /// </summary>
        [HttpDelete("delete/{maDonHang}/{maLo}")]
        public IActionResult Delete(int maDonHang, int maLo)
        {
            try
            {
                var result = _service.Delete(maDonHang, maLo);
                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy chi tiết để xóa" });

                return Ok(new { success = true, message = "Xóa chi tiết thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }
    }
}
