using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SieuThiService.Data;
using SieuThiService.Models.DTOs;

namespace SieuThiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KhoHangController : ControllerBase
    {
        private readonly ISieuThiRepository _sieuThiRepository;

        public KhoHangController(ISieuThiRepository sieuThiRepository)
        {
            _sieuThiRepository = sieuThiRepository;
        }

        /// <summary>
        /// Lấy danh sách kho hàng của siêu thị
        /// </summary>
        /// <param name="maSieuThi">Mã siêu thị</param>
        /// <returns>Danh sách kho hàng và tồn kho</returns>
        [HttpGet("sieu-thi/{maSieuThi}")]
        public async Task<ActionResult<DanhSachKhoResponse>> GetKhoHangBySieuThi(int maSieuThi)
        {
            try
            {
                var result = await _sieuThiRepository.GetKhoHangBySieuThiAsync(maSieuThi);
                
                if (result == null)
                {
                    return NotFound($"Không tìm thấy siêu thị với mã {maSieuThi}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một kho hàng
        /// </summary>
        /// <param name="maKho">Mã kho</param>
        /// <returns>Thông tin chi tiết kho hàng</returns>
        [HttpGet("{maKho}")]
        public async Task<ActionResult<KhoHangResponse>> GetKhoHangById(int maKho)
        {
            try
            {
                var result = await _sieuThiRepository.GetKhoHangByIdAsync(maKho);
                
                if (result == null)
                {
                    return NotFound($"Không tìm thấy kho với mã {maKho}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}