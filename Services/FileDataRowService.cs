using Sample_Librarian.Model;
using Sample_Librarian.Services;

namespace Sample_Librarian.Services;
    public class FileDataRowService
    {
        List<FileDataRow> dataRows = new List<FileDataRow>();

    public List<FileDataRow> GetFileDataRows()
    {
        if (dataRows.Count > 0) { dataRows.Clear(); };
        List<FileDataRow> newDataRows = new List<FileDataRow>();
        string filepath = "X:\\Downloads\\test";
        string[] files = Directory.GetFiles(filepath);
        foreach (string file in files)
        {
            var fileDataRow = new FileDataRow();
            var fileInfo = new FileInfo(file);
            fileDataRow.Format = fileInfo.Extension;
            fileDataRow.FileName = Path.GetFileNameWithoutExtension(file);
            fileDataRow.Size = fileInfo.Length.ToString();

            dataRows.Add(fileDataRow);

        }
        return dataRows;
    }
    }
