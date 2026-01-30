using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Text.Json;

namespace WaqfGIS.Services.GIS;

/// <summary>
/// خدمة معالجة الهندسة الجغرافية
/// </summary>
public class GeometryService
{
    private readonly GeometryFactory _geometryFactory;
    private readonly WKTReader _wktReader;
    private readonly WKTWriter _wktWriter;

    public GeometryService()
    {
        _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        _wktReader = new WKTReader(_geometryFactory);
        _wktWriter = new WKTWriter();
    }

    #region Geometry Creation

    public Point CreatePoint(double longitude, double latitude)
    {
        return _geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
    }

    public Polygon CreatePolygon(double[][] coordinates)
    {
        if (coordinates == null || coordinates.Length < 3)
            throw new ArgumentException("Polygon requires at least 3 coordinates");

        var coords = coordinates.Select(c => new Coordinate(c[0], c[1])).ToList();
        
        if (!coords.First().Equals2D(coords.Last()))
            coords.Add(coords.First());

        var ring = _geometryFactory.CreateLinearRing(coords.ToArray());
        return _geometryFactory.CreatePolygon(ring);
    }

    public LineString CreateLineString(double[][] coordinates)
    {
        if (coordinates == null || coordinates.Length < 2)
            throw new ArgumentException("LineString requires at least 2 coordinates");

        var coords = coordinates.Select(c => new Coordinate(c[0], c[1])).ToArray();
        return _geometryFactory.CreateLineString(coords);
    }

    #endregion

    #region GeoJSON Conversion

    public Geometry? FromGeoJson(string geoJson)
    {
        if (string.IsNullOrEmpty(geoJson)) return null;
        
        try
        {
            using var doc = JsonDocument.Parse(geoJson);
            var root = doc.RootElement;
            var type = root.GetProperty("type").GetString();
            var coordinates = root.GetProperty("coordinates");

            return type switch
            {
                "Point" => ParsePoint(coordinates),
                "LineString" => ParseLineString(coordinates),
                "Polygon" => ParsePolygon(coordinates),
                "MultiPolygon" => ParseMultiPolygon(coordinates),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    public string ToGeoJson(Geometry? geometry)
    {
        if (geometry == null) return "null";

        return geometry switch
        {
            Point p => $"{{\"type\":\"Point\",\"coordinates\":[{p.X.ToString(System.Globalization.CultureInfo.InvariantCulture)},{p.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}]}}",
            LineString ls => FormatLineStringGeoJson(ls),
            Polygon pg => FormatPolygonGeoJson(pg),
            MultiPolygon mp => FormatMultiPolygonGeoJson(mp),
            _ => "null"
        };
    }

    private Point ParsePoint(JsonElement coordinates)
    {
        var x = coordinates[0].GetDouble();
        var y = coordinates[1].GetDouble();
        return _geometryFactory.CreatePoint(new Coordinate(x, y));
    }

    private LineString ParseLineString(JsonElement coordinates)
    {
        var coords = new List<Coordinate>();
        foreach (var coord in coordinates.EnumerateArray())
        {
            coords.Add(new Coordinate(coord[0].GetDouble(), coord[1].GetDouble()));
        }
        return _geometryFactory.CreateLineString(coords.ToArray());
    }

    private Polygon ParsePolygon(JsonElement coordinates)
    {
        var rings = new List<LinearRing>();
        foreach (var ring in coordinates.EnumerateArray())
        {
            var coords = new List<Coordinate>();
            foreach (var coord in ring.EnumerateArray())
            {
                coords.Add(new Coordinate(coord[0].GetDouble(), coord[1].GetDouble()));
            }
            if (!coords.First().Equals2D(coords.Last()))
                coords.Add(coords.First());
            rings.Add(_geometryFactory.CreateLinearRing(coords.ToArray()));
        }

        if (rings.Count == 1)
            return _geometryFactory.CreatePolygon(rings[0]);
        else
            return _geometryFactory.CreatePolygon(rings[0], rings.Skip(1).ToArray());
    }

    private MultiPolygon ParseMultiPolygon(JsonElement coordinates)
    {
        var polygons = new List<Polygon>();
        foreach (var polygonCoords in coordinates.EnumerateArray())
        {
            polygons.Add(ParsePolygon(polygonCoords));
        }
        return _geometryFactory.CreateMultiPolygon(polygons.ToArray());
    }

    private string FormatLineStringGeoJson(LineString ls)
    {
        var coords = string.Join(",", ls.Coordinates.Select(c => 
            $"[{c.X.ToString(System.Globalization.CultureInfo.InvariantCulture)},{c.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}]"));
        return $"{{\"type\":\"LineString\",\"coordinates\":[{coords}]}}";
    }

    private string FormatPolygonGeoJson(Polygon pg)
    {
        var rings = new List<string>();
        rings.Add(FormatRing(pg.ExteriorRing));
        for (int i = 0; i < pg.NumInteriorRings; i++)
            rings.Add(FormatRing(pg.GetInteriorRingN(i)));
        return $"{{\"type\":\"Polygon\",\"coordinates\":[{string.Join(",", rings)}]}}";
    }

    private string FormatMultiPolygonGeoJson(MultiPolygon mp)
    {
        var polygons = new List<string>();
        for (int i = 0; i < mp.NumGeometries; i++)
        {
            var pg = (Polygon)mp.GetGeometryN(i);
            var rings = new List<string>();
            rings.Add(FormatRing(pg.ExteriorRing));
            for (int j = 0; j < pg.NumInteriorRings; j++)
                rings.Add(FormatRing(pg.GetInteriorRingN(j)));
            polygons.Add($"[{string.Join(",", rings)}]");
        }
        return $"{{\"type\":\"MultiPolygon\",\"coordinates\":[{string.Join(",", polygons)}]}}";
    }

    private string FormatRing(LineString ring)
    {
        var coords = string.Join(",", ring.Coordinates.Select(c => 
            $"[{c.X.ToString(System.Globalization.CultureInfo.InvariantCulture)},{c.Y.ToString(System.Globalization.CultureInfo.InvariantCulture)}]"));
        return $"[{coords}]";
    }

    #endregion

    #region WKT Conversion

    public Geometry? FromWkt(string wkt)
    {
        if (string.IsNullOrEmpty(wkt)) return null;
        try { return _wktReader.Read(wkt); }
        catch { return null; }
    }

    public string ToWkt(Geometry? geometry)
    {
        if (geometry == null) return string.Empty;
        return _wktWriter.Write(geometry);
    }

    #endregion

    #region Calculations

    public double CalculateAreaSquareMeters(Geometry? geometry)
    {
        if (geometry == null || geometry.IsEmpty) return 0;
        const double latFactor = 111320.0;
        const double lonFactor = 93400.0;
        return Math.Abs(geometry.Area * latFactor * lonFactor);
    }

    public double CalculatePerimeterMeters(Geometry? geometry)
    {
        if (geometry == null || geometry.IsEmpty) return 0;
        const double avgFactor = 102360.0;
        return geometry.Length * avgFactor;
    }

    public double CalculateLengthMeters(LineString? lineString)
    {
        if (lineString == null || lineString.IsEmpty) return 0;
        double totalLength = 0;
        var coords = lineString.Coordinates;
        for (int i = 0; i < coords.Length - 1; i++)
            totalLength += HaversineDistance(coords[i].Y, coords[i].X, coords[i + 1].Y, coords[i + 1].X);
        return totalLength;
    }

    public Point? CalculateCentroid(Geometry? geometry)
    {
        if (geometry == null || geometry.IsEmpty) return null;
        return _geometryFactory.CreatePoint(geometry.Centroid.Coordinate);
    }

    public double[] GetBoundingBox(Geometry? geometry)
    {
        if (geometry == null || geometry.IsEmpty) return new double[] { 0, 0, 0, 0 };
        var env = geometry.EnvelopeInternal;
        return new double[] { env.MinX, env.MinY, env.MaxX, env.MaxY };
    }

    #endregion

    #region Spatial Analysis

    public Geometry? CreateBuffer(Geometry? geometry, double distanceMeters)
    {
        if (geometry == null || geometry.IsEmpty) return null;
        double distanceDegrees = distanceMeters / 111320.0;
        return geometry.Buffer(distanceDegrees, 16);
    }

    public bool Intersects(Geometry? geom1, Geometry? geom2)
    {
        if (geom1 == null || geom2 == null) return false;
        return geom1.Intersects(geom2);
    }

    public Geometry? GetIntersection(Geometry? geom1, Geometry? geom2)
    {
        if (geom1 == null || geom2 == null) return null;
        if (!geom1.Intersects(geom2)) return null;
        return geom1.Intersection(geom2);
    }

    public bool Contains(Geometry? container, Geometry? contained)
    {
        if (container == null || contained == null) return false;
        return container.Contains(contained);
    }

    public double CalculateDistance(Geometry? geom1, Geometry? geom2)
    {
        if (geom1 == null || geom2 == null) return double.MaxValue;
        var p1 = geom1.Centroid;
        var p2 = geom2.Centroid;
        return HaversineDistance(p1.Y, p1.X, p2.Y, p2.X);
    }

    public bool IsWithinDistance(Geometry? geom1, Geometry? geom2, double distanceMeters)
    {
        return CalculateDistance(geom1, geom2) <= distanceMeters;
    }

    public Geometry? Union(IEnumerable<Geometry> geometries)
    {
        var geomList = geometries.Where(g => g != null && !g.IsEmpty).ToList();
        if (!geomList.Any()) return null;
        var collection = _geometryFactory.CreateGeometryCollection(geomList.ToArray());
        return collection.Union();
    }

    #endregion

    #region Validation

    public bool IsValid(Geometry? geometry)
    {
        if (geometry == null) return false;
        return geometry.IsValid;
    }

    public Geometry? MakeValid(Geometry? geometry)
    {
        if (geometry == null) return null;
        if (geometry.IsValid) return geometry;
        try { return geometry.Buffer(0); }
        catch { return geometry; }
    }

    public Geometry? Simplify(Geometry? geometry, double tolerance = 0.00001)
    {
        if (geometry == null) return null;
        return NetTopologySuite.Simplify.TopologyPreservingSimplifier.Simplify(geometry, tolerance);
    }

    #endregion

    #region Private Helpers

    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180.0;

    #endregion
}
