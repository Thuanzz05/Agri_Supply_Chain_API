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
        /// Lấy danh sách kho hàng của siêu thị (chỉ thông tin cơ bản)
        /// </summary>
        /// <param name="maSieuThi">Mã siêu thị</param>
        /// <returns>Danh sách kho hàng cơ bản</returns>
        [HttpGet("sieu-thi/{maSieuThi}")]
        public async Task<ActionResult<DanhSachKhoSimpleResponse>> GetDanhSachKhoBySieuThi(int maSieuThi)
        {
            try
            {
                var result = await _sieuThiRepository.GetDanhSachKhoBySieuThiAsync(maSieuThi);
                
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

        /// <summary>
        /// Tạo kho mới
        /// </summary>
        /// <param name="request">Thông tin kho mới</param>
        /// <returns>Kết quả tạo kho</returns>
        [HttpPost("tao-kho")]
        public async Task<ActionResult<CreateKhoResponse>> CreateKho([FromBody] CreateKhoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _sieuThiRepository.CreateKhoAsync(request);
                
                if (result == null)
                {
                    return NotFound($"Không tìm thấy siêu thị với mã {request.MaSieuThi}");
                }

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetKhoHangById), new { maKho = result.MaKho }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin kho
        /// </summary>
        /// <param name="request">Thông tin cập nhật kho</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("cap-nhat-kho")]
        public async Task<ActionResult<UpdateKhoResponse>> UpdateKho([FromBody] UpdateKhoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _sieuThiRepository.UpdateKhoAsync(request);
                
                if (result == null)
                {
                    return NotFound($"Không tìm thấy kho với mã {request.MaKho}");
                }

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa kho
        /// </summary>
        /// <param name="maKho">Mã kho cần xóa</param>
        /// <returns>Kết quả xóa kho</returns>
        [HttpDelete("xoa-kho/{maKho}")]
        public async Task<ActionResult<DeleteKhoResponse>> DeleteKho(int maKho)
        {
            try
            {
                var result = await _sieuThiRepository.DeleteKhoAsync(maKho);
                
                if (result == null)
                {
                    return NotFound($"Không tìm thấy kho với mã {maKho}");
                }

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa tồn kho (xóa sản phẩm khỏi kho)
        /// </summary>
        /// <param name="request">Thông tin xóa tồn kho</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("xoa-ton-kho")]
        public async Task<ActionResult<DeleteTonKhoResponse>> DeleteTonKho([FromBody] DeleteTonKhoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _sieuThiRepository.DeleteTonKhoAsync(request);
                
                if (result == null)
                {
                    return NotFound($"Không tìm thấy kho với mã {request.MaKho}");
                }

                if (!result.Success)
                {
                    return BadRequest(result);
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