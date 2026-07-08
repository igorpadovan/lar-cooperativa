using LarCooperativa.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace LarCooperativa.Api.Controllers;

public static class ControllerExtensions
{
    /// <summary>
    /// Converte um <see cref="Error"/> de negócio na resposta Problem Details correspondente.
    /// </summary>
    public static ObjectResult ToProblem(this ControllerBase controller, Error error) =>
        controller.Problem(
            title: error.Code,
            detail: error.Message,
            statusCode: error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError,
            });
}
