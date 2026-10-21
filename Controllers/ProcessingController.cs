using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RbcTestTask.Api.Services;
using RbcTestTask.Api.Validators;
using TestTask.Models;

namespace RbcTestTask.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessingController : ControllerBase
{
    private readonly ProcessingService _processingService;
    private readonly IValidator<RequestModel> _validator;

    public ProcessingController(ProcessingService processingService, IValidator<RequestModel> validator)
    {
        _processingService = processingService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> Process([FromBody] RequestModel request)
    {
        // Валидация с помощью FluentValidation
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var response = new ResponseModel
            {
                IsError = 1,
                ErrorCode = "ValidationError",
                ErrorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            };
            return BadRequest(response);
        }

        var result = await _processingService.ProcessAsync(request);

        // Определяем код ответа в зависимости от наличия ошибки
        if (result.IsError == 1)
            return StatusCode(result.ErrorCode is "Base64DecodeError" or "DecryptionError" ? 400 : 500, result);

        // Возвращаем объект, сериализованный в JSON с отступами
        return new JsonResult(result, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}