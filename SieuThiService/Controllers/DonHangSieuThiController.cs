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

        /// <summary>
        /// API 1: Tạo đơn hàng (chỉ thông tin cơ bản, chưa có chi tiết)
        /// </summary>
        /// <param name="request">Thông tin đơn hàng cơ bản</param>
        /// <returns>Thông tin đơn hàng đã tạo</returns>
        [HttpPost("tao-don-hang")]
        public async Task<ActionResult<DonHangResponse>> CreateDonHangOnly([FromBody] CreateDonHangRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra siêu thị có tồn tại không
                var sieuThi = await _sieuThiRepository.GetSieuThiByIdAsync(request.MaSieuThi);
                if (sieuThi == null)
                {
                    return NotFound($"Không tìm thấy siêu thị với mã {request.MaSieuThi}");
                }

                var result = await _sieuThiRepository.CreateDonHangOnlyAsync(request);
                
                if (result == null)
                {
                    return BadRequest("Không thể tạo đơn hàng");
                }

                return CreatedAtAction(nameof(GetDonHangById), new { id = result.MaDonHang }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// API 2: Thêm chi tiết đơn hàng vào đơn hàng đã tạo
        /// </summary>
        /// <param name="request">Thông tin chi tiết đơn hàng</param>
        /// <returns>Thông tin chi tiết đã thêm</returns>
        [HttpPost("them-chi-tiet")]
        public async Task<ActionResult<ChiTietDonHangAddResponse>> AddChiTietDonHang([FromBody] CreateChiTietDonHangRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _sieuThiRepository.AddChiTietDonHangAsync(request);
                
                if (result == null)
                {
                    return NotFound($"Không tìm thấy đơn hàng với mã {request.MaDonHang}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo đơn hàng mới cho siêu thị (API gộp - giữ lại để tương thích)
        /// </summary>
        /// <param name="request">Thông tin đơn hàng cần tạo</param>
        /// <returns>Thông tin đơn hàng đã tạo</returns>
        [HttpPost]
        public async Task<ActionResult<DonHangSieuThiResponse>> CreateDonHang([FromBody] CreateDonHangSieuThiRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra danh sách chi tiết đơn hàng không rỗng
                if (!request.ChiTietDonHangs.Any())
                {
                    return BadRequest("Đơn hàng phải có ít nhất một sản phẩm");
                }

                // Kiểm tra siêu thị có tồn tại không
                var sieuThi = await _sieuThiRepository.GetSieuThiByIdAsync(request.MaSieuThi);
                if (sieuThi == null)
                {
                    return NotFound($"Không tìm thấy siêu thị với mã {request.MaSieuThi}");
                }

                var result = await _sieuThiRepository.CreateDonHangAsync(request);
                
                if (result == null)
                {
                    return BadRequest("Không thể tạo đơn hàng");
                }

                return CreatedAtAction(nameof(GetDonHangById), new { id = result.MaDonHang }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin đơn hàng theo ID
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns>Thông tin đơn hàng</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<DonHangSieuThiResponse>> GetDonHangById(int id)
        {
            try
            {
                var donHang = await _sieuThiRepository.GetDonHangByIdAsync(id);
                
                if (donHang == null)
                {
                    return NotFound($"Không tìm thấy đơn hàng với mã {id}");
                }

                return Ok(donHang);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách đơn hàng của một siêu thị
        /// </summary>
        /// <param name="maSieuThi">Mã siêu thị</param>
        /// <returns>Danh sách đơn hàng</returns>
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
