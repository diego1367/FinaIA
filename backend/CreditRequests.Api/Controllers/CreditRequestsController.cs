using CreditRequests.Api.Contracts;
using CreditRequests.Application.Contracts;
using CreditRequests.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CreditRequests.Api.Controllers;

[ApiController]
[Route("api/credit-requests")]
public sealed class CreditRequestsController(ICreditRequestService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CreditRequestResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CreditRequestResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await service.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CreditRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreditRequestResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var request = await service.GetByIdAsync(id, cancellationToken);
        if (request is null)
            return Problem(detail: "No existe una solicitud con ese identificador.", statusCode: StatusCodes.Status404NotFound);

        return Ok(request);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreditRequestResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreditRequestResponseDto>> Create(CreateCreditRequestDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateCreditRequestCommand(
            dto.DocumentNumber, dto.FullName, dto.RequestedAmount, dto.TermMonths);
        var response = await service.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(CreditRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreditRequestResponseDto>> SetStatus(int id, SetCreditRequestStatusDto dto, CancellationToken cancellationToken)
    {
        var result = await service.SetStatusAsync(id, dto.Status, cancellationToken);
        return result switch
        {
            SetCreditRequestStatusResult.Updated updated => Ok(updated.Request),
            SetCreditRequestStatusResult.InvalidStatus => Problem(
                detail: "El estado debe ser Approved o Rejected.", statusCode: StatusCodes.Status400BadRequest),
            SetCreditRequestStatusResult.NotFound => Problem(
                detail: "No existe una solicitud con ese identificador.", statusCode: StatusCodes.Status404NotFound),
            SetCreditRequestStatusResult.AlreadyResolved => Problem(
                detail: "Solo puedes aprobar o rechazar una solicitud pendiente.", statusCode: StatusCodes.Status409Conflict),
            SetCreditRequestStatusResult.ConcurrentChange => Problem(
                detail: "La solicitud acaba de cambiar. Actualiza la lista para ver su estado.", statusCode: StatusCodes.Status409Conflict),
            _ => throw new InvalidOperationException("Resultado de cambio de estado desconocido.")
        };
    }
}
