using Application.Features.Departments;
using Application.Features.Departments.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/departments")]
public class DepartmentsController : BaseApiController
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _departmentService.GetAllAsync(cancellationToken);
        return FromResult(result, departments => Ok(departments));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _departmentService.GetByIdAsync(id, cancellationToken);
        return FromResult(result, department => Ok(department));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _departmentService.CreateAsync(request, cancellationToken);
        return FromResult(result, department => CreatedAtAction(nameof(GetById), new { id = department.DepartmentId }, department));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _departmentService.UpdateAsync(id, request, cancellationToken);
        return FromResult(result, department => Ok(department));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _departmentService.DeleteAsync(id, cancellationToken);
        return FromResult(result, () => NoContent());
    }
}
