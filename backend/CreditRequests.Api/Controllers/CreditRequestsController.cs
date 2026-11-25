using CreditRequests.Api.Contracts;
using CreditRequests.Api.Data;
using CreditRequests.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreditRequests.Api.Controllers;

[ApiController]
[Route("api/credit-requests")]
public sealed class CreditRequestsController(CreditRequestsDbContext db) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CreditRequestResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CreditRequestResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var requests = await db.CreditRequests.AsNoTracking()
            .OrderByDescending(request => request.CreatedAtUtc)
            .ThenByDescending(request => request.Id)
            .Select(request => CreditRequestResponseDto.FromEntity(request))
            .ToListAsync(cancellationToken);

        return Ok(requests);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CreditRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreditRequestResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var request = await db.CreditRequests.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (request is null)
            return Problem(detail: "No existe una solicitud con ese identificador.", statusCode: StatusCodes.Status404NotFound);

        return Ok(CreditRequestResponseDto.FromEntity(request));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreditRequestResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreditRequestResponseDto>> Create(CreateCreditRequestDto dto, CancellationToken cancellationToken)
    {
        var request = new CreditRequest(dto.DocumentNumber, dto.FullName.Trim(), dto.RequestedAmount, dto.TermMonths);
        db.CreditRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);

        var response = CreditRequestResponseDto.FromEntity(request);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, response);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(CreditRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreditRequestResponseDto>> SetStatus(int id, SetCreditRequestStatusDto dto, CancellationToken cancellationToken)
    {
        var newStatus = dto.Status switch
        {
            "Approved" => CreditRequestStatus.Approved,
            "Rejected" => CreditRequestStatus.Rejected,
            _ => CreditRequestStatus.Pending
        };

        if (newStatus == CreditRequestStatus.Pending)
            return Problem(detail: "El estado debe ser Approved o Rejected.", statusCode: StatusCodes.Status400BadRequest);

        var request = await db.CreditRequests.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (request is null)
            return Problem(detail: "No existe una solicitud con ese identificador.", statusCode: StatusCodes.Status404NotFound);

        if (!request.TrySetStatus(newStatus))
            return Problem(detail: "Solo puedes aprobar o rechazar una solicitud pendiente.", statusCode: StatusCodes.Status409Conflict);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(detail: "La solicitud acaba de cambiar. Actualiza la lista para ver su estado.", statusCode: StatusCodes.Status409Conflict);
        }

        return Ok(CreditRequestResponseDto.FromEntity(request));
    }
}
