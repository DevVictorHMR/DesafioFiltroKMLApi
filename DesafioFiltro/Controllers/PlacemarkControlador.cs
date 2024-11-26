using FiltroKMLApi.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/placemarks")]
public class PlacemarkControlador : ControllerBase
{
    private readonly KmlServico _kmlServico;
    private readonly IWebHostEnvironment _env;
    private readonly IValidator<PlacemarkFiltro> _placemarkFiltroValidador;

    public PlacemarkControlador(KmlServico kmlServico, IWebHostEnvironment env, IValidator<PlacemarkFiltro> placemarkFiltroValidador)
    {
        _kmlServico = kmlServico;
        _env = env;
        _placemarkFiltroValidador = placemarkFiltroValidador;
    }

    [HttpGet]
    public IActionResult GetPlacemarks([FromQuery] PlacemarkFiltro filtro)
    {
        var validationResult = _placemarkFiltroValidador.Validate(filtro);

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
    public async Task<IActionResult> ExportPlacemarksAsync([FromBody] PlacemarkFiltro filtro)
    {
        var validationResult = _placemarkFiltroValidador.Validate(filtro);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var filtroPlacemarks = _kmlServico.GetFilteredPlacemarks(filtro);

        if (!filtroPlacemarks.Any())
        {
            return BadRequest(new { Error = "Nenhum placemark encontrado com os filtros aplicados." });
        }

        var filePath = await _kmlServico.ExportFilteredPlacemarksAsync(filtroPlacemarks, _env);

        return Ok(new { Message = "Arquivo exportado com sucesso.", FilePath = filePath });
    }
}
