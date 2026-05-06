using System.IO;
using ChomikEngine.Models;

namespace ChomikEngine.Export;

public static class ModelExporter
{
    public static void Export(Mesh mesh, string path, ExportFormat format)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);

        switch (format)
        {
            case ExportFormat.StlBinary:
            case ExportFormat.StlAscii:
                StlExporter.Export(mesh, path, format == ExportFormat.StlAscii);
                break;
            case ExportFormat.Obj:
                ObjExporter.Export(mesh, path);
                break;
            case ExportFormat.Ply:
                PlyExporter.Export(mesh, path);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }
}
