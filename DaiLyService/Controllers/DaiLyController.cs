using Microsoft.AspNetCore.Mvc;
using DaiLyService.Services;
using DaiLyService.Models.DTOs;

namespace DaiLyService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DaiLyController : ControllerBase
    {
        private readonly IDaiLyService _daiLyService;
        private readonly ILogger<DaiLyController> _logger;

        public DaiLyController(
            IDaiLyService daiLyService,
            ILogger<DaiLyController> logger)
        {
            _daiLyService = daiLyService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả đại lý
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<DaiLyPhanHoi>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await _daiLyService.LayTatCaDaiLy();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách đại lý");
                return StatusCode(500, new { Message = "Lỗi server", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin đại lý theo mã
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DaiLyPhanHoi), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var result = await _daiLyService.LayDaiLyTheoMa(id);

                if (result == null)
                {
                    return NotFound(new { Message = $"Không tìm thấy đại lý có mã {id}" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy đại lý ID: {Id}", id);
                return StatusCode(500, new { Message = "Lỗi server", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Tạo mới đại lý
        /// </summary>
        [HttpPost("create")]
        [ProducesResponseType(typeof(DaiLyPhanHoi), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] DaiLyTaoMoi model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var newDaiLy = await _daiLyService.TaoMoiDaiLy(model);

                return CreatedAtAction(
                    nameof(Get),
                    new { id = newDaiLy.MaDaiLy },
                    newDaiLy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đại lý mới");
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin đại lý
        /// </summary>
        [HttpPut("update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] DaiLyTaoMoi model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var isSuccess = await _daiLyService.CapNhatDaiLy(id, model);

                if (!isSuccess)
                {
                    return NotFound(new { Message = $"Không tìm thấy đại lý có mã {id}" });
                }

                return Ok(new { Message = "Cập nhật thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật đại lý ID: {Id}", id);
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Xóa đại lý
        /// </summary>
        [HttpDelete("delete/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var isSuccess = await _daiLyService.XoaDaiLy(id);

                if (!isSuccess)
                {
                    return NotFound(new { Message = $"Không tìm thấy đại lý có mã {id}" });
                }

                return Ok(new { Message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa đại lý ID: {Id}", id);
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm đại lý theo tên và số điện thoại
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<DaiLyPhanHoi>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(
            [FromQuery] string? ten,
            [FromQuery] string? sdt)
        {
            try
            {
                var result = await _daiLyService.TimKiemDaiLy(ten, sdt);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm đại lý");
                return StatusCode(500, new { Message = "Lỗi server", Detail = ex.Message });
            }
        }
    }
}