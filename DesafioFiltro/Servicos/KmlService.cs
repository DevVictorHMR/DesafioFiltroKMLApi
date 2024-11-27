using FiltroKMLApi.Models;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.Collections.Concurrent;
using Placemark = SharpKml.Dom.Placemark;

public class KmlService
{
    private readonly List<Placemark> _placemarks;

    public KmlService(IWebHostEnvironment env)
    {
        var kmlPath = Path.Combine(env.WebRootPath, "DIRECIONADORES1.kml");
        _placemarks = LoadKmlAsync(kmlPath).GetAwaiter().GetResult();
    }

    private async Task<List<Placemark>> LoadKmlAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Arquivo não encontrado: {filePath}");

        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var parser = new Parser();
        await Task.Run(() => parser.Parse(fileStream));

        var kmlFile = parser.Root as Kml;
        var placemarks = new List<Placemark>();

        if (kmlFile?.Feature is Document document)
        {
            placemarks.AddRange(document.Flatten().OfType<Placemark>());
        }

        return placemarks;
    }

    public IEnumerable<Placemark> GetFilteredPlacemarks(PlacemarkFilter filter)
    {
        return _placemarks.AsParallel().Where(p =>
            (string.IsNullOrEmpty(filter.Cliente) || GetExtendedDataValue(p, "CLIENTE") == filter.Cliente) &&
            (string.IsNullOrEmpty(filter.Situacao) || GetExtendedDataValue(p, "SITUAÇÃO") == filter.Situacao) &&
            (string.IsNullOrEmpty(filter.Bairro) || GetExtendedDataValue(p, "BAIRRO") == filter.Bairro) &&
            (string.IsNullOrEmpty(filter.Referencia) || (filter.Referencia.Length >= 3 && GetExtendedDataValue(p, "REFERENCIA")?.Contains(filter.Referencia) == true)) &&
            (string.IsNullOrEmpty(filter.RuaCruzamento) || (filter.RuaCruzamento.Length >= 3 && GetExtendedDataValue(p, "RUA/CRUZAMENTO")?.Contains(filter.RuaCruzamento) == true))
        );
    }

    public HashSet<string> GetDistinctFieldValues(string nameCamp)
    {
        return _placemarks
            .Select(p => GetExtendedDataValue(p, nameCamp))
            .Where(value => value != null)
            .ToHashSet();
    }

    private static string? GetExtendedDataValue(Placemark placemark, string nameCamp)
    {
        var extendedData = placemark.ExtendedData;
        if (extendedData == null) return null;

        var data = extendedData.Data.FirstOrDefault(d => d.Name == nameCamp);
        return data?.Value;
    }

    public async Task<string> ExportFilteredPlacemarksAsync(IEnumerable<Placemark> filteredPlacemarks, IWebHostEnvironment env)
    {
        var arquivo = new Document();

        foreach (var placemark in filteredPlacemarks)
        {
            var clonedPlacemark = placemark.Clone() as Placemark;
            arquivo.AddFeature(clonedPlacemark);
        }

        var kml = new Kml
        {
            Feature = arquivo
        };

        var serializer = new Serializer();
        serializer.Serialize(kml);

        var arquiveName = $"KMLExportado_{DateTime.Now:yyyyMMdd}.kml";
        var exportPath = Path.Combine(env.WebRootPath, "exports");

        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        var filePath = Path.Combine(exportPath, arquiveName);

        await File.WriteAllTextAsync(filePath, serializer.Xml);

        return $"/exports/{arquiveName}";
    }
}
