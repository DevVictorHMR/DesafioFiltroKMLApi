using FiltroKMLApi.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/placemarks")]
public class PlacemarkControlador : ControllerBase
{
    private readonly KmlService _kmlServico;
    private readonly IWebHostEnvironment _env;
    private readonly IValidator<PlacemarkFilter> _placemarkFilterValidator;

    public PlacemarkControlador(KmlService kmlServico, IWebHostEnvironment env, IValidator<PlacemarkFilter> placemarkFilterValidator)
    {
        _kmlServico = kmlServico;
        _env = env;
        _placemarkFilterValidator = placemarkFilterValidator;
    }

    [HttpGet]
    public IActionResult GetPlacemarks([FromQuery] PlacemarkFilter filtro)
    {
        var validationResult = _placemarkFilterValidator.Validate(filtro);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }
        var placemarks = _kmlServico.GetFilteredPlacemarks(filtro);
        return Ok(placemarks);
    }

    [HttpGet("filters")]
    public IActionResult GetFilterValues()
    {
        var clientes = _kmlServico.GetDistinctFieldValues("CLIENTE");
        var situacoes = _kmlServico.GetDistinctFieldValues("SITUAÇÃO");
        var bairros = _kmlServico.GetDistinctFieldValues("BAIRRO");

        return Ok(new { Clientes = clientes, Situacoes = situacoes, Bairros = bairros });
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportPlacemarksAsync([FromBody] PlacemarkFilter filter)
    {
        var validationResult = _placemarkFilterValidator.Validate(filter);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var filterPlacemarks = _kmlServico.GetFilteredPlacemarks(filter);

        if (!filterPlacemarks.Any())
        {
            return BadRequest(new { Error = "Nenhum placemark encontrado com os filtros aplicados." });
        }

        var filePath = await _kmlServico.ExportFilteredPlacemarksAsync(filterPlacemarks, _env);

        return Ok(new { Message = "Arquivo exportado com sucesso.", FilePath = filePath });
    }
}
