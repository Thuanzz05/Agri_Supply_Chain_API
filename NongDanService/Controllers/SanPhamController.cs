using Microsoft.AspNetCore.Mvc;
using NongDanService.Models.DTOs;
using NongDanService.Services;

[ApiController]
[Route("api/san-pham")]
public class SanPhamController : ControllerBase
{
    private readonly ISanPhamService _sanPhamService;

    public SanPhamController(ISanPhamService sanPhamService)
    {
        _sanPhamService = sanPhamService;
    }

    // GET: api/san-pham/get-all
    [HttpGet("get-all")]
    public IActionResult GetAll()
    {
        var data = _sanPhamService.GetAll();
        return Ok(data);
    }

    // POST: api/san-pham/create
    [HttpPost("create")]
    public IActionResult Create(SanPhamCreateDTO dto)
    {
        var newId = _sanPhamService.Create(dto);
        return Ok(newId);
    }

    // PUT: api/san-pham/update/5
    [HttpPut("update/{id}")]
    public IActionResult Update(int id, SanPhamUpdateDTO dto)
    {
        bool result = _sanPhamService.Update(id, dto);

        if (result == false)
        {
            return NotFound();
        }

        return Ok();
    }

    // DELETE: api/san-pham/delete/5
    [HttpDelete("delete/{id}")]
    public IActionResult Delete(int id)
    {
        bool result = _sanPhamService.Delete(id);

        if (result == false)
        {
            return NotFound();
        }

        return Ok();
    }
}
