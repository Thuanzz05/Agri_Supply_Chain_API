using Microsoft.AspNetCore.Mvc;
using NongDanService.Models.DTOs;
using NongDanService.Services;

namespace NongDanService.Controllers
{
    [Route("api/don-hang-dai-ly")]
    [ApiController]
    public class DonHangDaiLyController : ControllerBase
    {
        private readonly IDonHangDaiLyService _donHangService;

        public DonHangDaiLyController(IDonHangDaiLyService donHangService)
        {
            _donHangService = donHangService;
        }

        /// <summary>
        /// Lấy tất cả đơn hàng đại lý
        /// </summary>
        [HttpGet("get-all")]
        public IActionResult GetAll()
        {
            try
            {
                var data = _donHangService.GetAll();
                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách đơn hàng thành công",
                    data = data,
                    count = data.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy đơn hàng theo ID
        /// </summary>
        [HttpGet("get-by-id/{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID đơn hàng không hợp lệ"
                    });
                }

                var data = _donHangService.GetById(id);
                if (data == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin đơn hàng thành công",
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy đơn hàng theo mã nông dân
        /// </summary>
        [HttpGet("get-by-nong-dan/{maNongDan}")]
        public IActionResult GetByNongDanId(int maNongDan)
        {
            try
            {
                if (maNongDan <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã nông dân không hợp lệ"
                    });
                }

                var data = _donHangService.GetByNongDanId(maNongDan);
                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách đơn hàng theo nông dân thành công",
                    data = data,
                    count = data.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy đơn hàng theo mã đại lý
        /// </summary>
        [HttpGet("get-by-dai-ly/{maDaiLy}")]
        public IActionResult GetByDaiLyId(int maDaiLy)
        {
            try
            {
                if (maDaiLy <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã đại lý không hợp lệ"
                    });
                }

                var data = _donHangService.GetByDaiLyId(maDaiLy);
                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách đơn hàng theo đại lý thành công",
                    data = data,
                    count = data.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Tạo đơn hàng mới
        /// </summary>
        [HttpPost("create")]
        public IActionResult Create([FromBody] DonHangDaiLyCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState
                    });
                }

                var newId = _donHangService.Create(dto);
                return Ok(new
                {
                    success = true,
                    message = "Tạo đơn hàng thành công",
                    data = new { id = newId }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Cập nhật đơn hàng
        /// </summary>
        [HttpPut("update/{id}")]
        public IActionResult Update(int id, [FromBody] DonHangDaiLyUpdateDTO dto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID đơn hàng không hợp lệ"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState
                    });
                }

                bool result = _donHangService.Update(id, dto);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng để cập nhật"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật đơn hàng thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Xác nhận đơn hàng (Nông dân xác nhận nhận đơn từ đại lý)
        /// </summary>
        [HttpPut("xac-nhan/{id}")]
        public IActionResult XacNhanDon(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID đơn hàng không hợp lệ"
                    });
                }

                bool result = _donHangService.XacNhanDon(id);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng để xác nhận"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Xác nhận đơn hàng thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Xuất đơn hàng (Nông dân xuất hàng cho đại lý)
        /// </summary>
        [HttpPut("xuat-don/{id}")]
        public IActionResult XuatDon(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID đơn hàng không hợp lệ"
                    });
                }

                bool result = _donHangService.XuatDon(id);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng để xuất"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Xuất đơn hàng thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Hủy đơn hàng
        /// </summary>
        [HttpPut("huy-don/{id}")]
        public IActionResult HuyDon(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID đơn hàng không hợp lệ"
                    });
                }

                bool result = _donHangService.HuyDon(id);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng để hủy"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Hủy đơn hàng thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "ID đơn hàng không hợp lệ"
                    });
                }

                bool result = _donHangService.Delete(id);
                if (!result)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng để xóa"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Xóa đơn hàng thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi server: " + ex.Message
                });
            }
        }
    }
}
