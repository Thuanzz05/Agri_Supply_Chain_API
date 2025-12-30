using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SieuThiService.Data;
using SieuThiService.Models.DTOs;

namespace SieuThiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangSieuThiController : ControllerBase
    {
        private readonly ISieuThiRepository _sieuThiRepository;

        public DonHangSieuThiController(ISieuThiRepository sieuThiRepository)
        {
            _sieuThiRepository = sieuThiRepository;
        }

        [HttpGet("sieu-thi/{maSieuThi}")]
        public async Task<ActionResult<List<DonHangSieuThiResponse>>> GetDonHangsBySieuThi(int maSieuThi)
        {
            try
            {
                // Kiểm tra siêu thị có tồn tại không
                var sieuThi = await _sieuThiRepository.GetSieuThiByIdAsync(maSieuThi);
                if (sieuThi == null)
                {
                    return NotFound($"Không tìm thấy siêu thị với mã {maSieuThi}");
                }

                var donHangs = await _sieuThiRepository.GetDonHangsBySieuThiAsync(maSieuThi);
                return Ok(donHangs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}
