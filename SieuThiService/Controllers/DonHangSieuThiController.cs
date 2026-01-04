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
        public ActionResult CreateDonHangOnly([FromBody] CreateDonHangRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Kiểm tra siêu thị có tồn tại không
                var sieuThiExists = _sieuThiRepository.GetSieuThiById(request.MaSieuThi);
                if (!sieuThiExists)
                {
                    return NotFound($"Không tìm thấy siêu thị với mã {request.MaSieuThi}");
                }

                var result = _sieuThiRepository.CreateDonHangOnly(request);
                
                if (!result)
                {
                    return BadRequest("Không thể tạo đơn hàng");
                }

                return Ok("Tạo đơn hàng thành công");
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
        public ActionResult AddChiTietDonHang([FromBody] CreateChiTietDonHangRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = _sieuThiRepository.AddChiTietDonHang(request);
                
                if (!result)
                {
                    return NotFound($"Không tìm thấy đơn hàng với mã {request.MaDonHang}");
                }

                return Ok("Thêm chi tiết đơn hàng thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// API 4: Nhận hàng từ đại lý (phải chọn kho)
        /// </summary>
        /// <param name="id">Mã đơn hàng cần nhận</param>
        /// <param name="request">Thông tin nhận hàng bao gồm mã kho</param>
        /// <returns>Kết quả nhận hàng</returns>
        [HttpPut("nhan-hang/{id}")]
        public ActionResult NhanHang(int id, [FromBody] NhanHangRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (request.MaKho <= 0)
                {
                    return BadRequest("Mã kho là bắt buộc và phải lớn hơn 0");
                }

                var result = _sieuThiRepository.NhanHang(id, request);
                
                if (!result)
                {
                    return BadRequest("Không thể nhận hàng");
                }

                return Ok("Nhận hàng thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// API 5: Sửa chi tiết đơn hàng
        /// </summary>
        /// <param name="request">Thông tin chi tiết đơn hàng cần cập nhật</param>
        /// <returns>Kết quả cập nhật chi tiết đơn hàng</returns>
        [HttpPut("sua-chi-tiet")]
        public ActionResult UpdateChiTietDonHang([FromBody] UpdateChiTietDonHangRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = _sieuThiRepository.UpdateChiTietDonHang(request);
                
                if (!result)
                {
                    return BadRequest("Không thể cập nhật chi tiết đơn hàng");
                }

                return Ok("Cập nhật chi tiết đơn hàng thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// API 6: Xóa chi tiết đơn hàng
        /// </summary>
        /// <param name="request">Thông tin chi tiết đơn hàng cần xóa</param>
        /// <returns>Kết quả xóa chi tiết đơn hàng</returns>
        [HttpDelete("xoa-chi-tiet")]
        public ActionResult DeleteChiTietDonHang([FromBody] DeleteChiTietDonHangRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = _sieuThiRepository.DeleteChiTietDonHang(request);
                
                if (!result)
                {
                    return BadRequest("Không thể xóa chi tiết đơn hàng");
                }

                return Ok("Xóa chi tiết đơn hàng thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        /// <summary>
        /// API 7: Hủy đơn hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng cần hủy</param>
        /// <returns>Kết quả hủy đơn hàng</returns>
        [HttpPut("huy-don-hang/{id}")]
        public ActionResult HuyDonHang(int id)
        {
            try
            {
                var result = _sieuThiRepository.HuyDonHang(id);
                
                if (!result)
                {
                    return BadRequest("Không thể hủy đơn hàng");
                }

                return Ok("Hủy đơn hàng thành công");
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
        public ActionResult CreateDonHang([FromBody] CreateDonHangSieuThiRequest request)
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
                var sieuThiExists = _sieuThiRepository.GetSieuThiById(request.MaSieuThi);
                if (!sieuThiExists)
                {
                    return NotFound($"Không tìm thấy siêu thị với mã {request.MaSieuThi}");
                }

                var result = _sieuThiRepository.CreateDonHang(request);
                
                if (!result)
                {
                    return BadRequest("Không thể tạo đơn hàng");
                }

                return Ok("Tạo đơn hàng thành công");
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
        public ActionResult<DonHangSieuThiResponse> GetDonHangById(int id)
        {
            try
            {
                var donHang = _sieuThiRepository.GetDonHangById(id);
                
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
        public ActionResult<List<DonHangSieuThiResponse>> GetDonHangsBySieuThi(int maSieuThi)
        {
            try
            {
                // Kiểm tra siêu thị có tồn tại không
                var sieuThiExists = _sieuThiRepository.GetSieuThiById(maSieuThi);
                if (!sieuThiExists)
                {
                    return NotFound($"Không tìm thấy siêu thị với mã {maSieuThi}");
                }

                var donHangs = _sieuThiRepository.GetDonHangsBySieuThi(maSieuThi);
                return Ok(donHangs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}