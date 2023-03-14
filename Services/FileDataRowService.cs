using Sample_Librarian.Model;
using Sample_Librarian.Services;
using Plugin.Maui.Audio;

namespace Sample_Librarian.Services;
    public class FileDataRowService
    {
        List<FileDataRow> dataRows = new List<FileDataRow>();

    public async Task<List<FileDataRow>> GetFileDataRows()
    {
        if (dataRows.Count > 0) { dataRows.Clear(); };
        List<FileDataRow> newDataRows = new List<FileDataRow>();
        string filepath = @"X:\Programming\Projects\0323\Sample-Librarian\Resources\Raw";
        string[] files = Directory.GetFiles(filepath);
        foreach (string file in files)
        {
            var fileDataRow = new FileDataRow();
            var fileInfo = new FileInfo(file);
            fileDataRow.Format = fileInfo.Extension;
            fileDataRow.FileName = Path.GetFileNameWithoutExtension(file);
            fileDataRow.Size = fileInfo.Length.ToString();
            fileDataRow.FilePath = file;
            string fullFileName = $"{fileDataRow.FileName}{fileDataRow.Format}";
            fileDataRow.Player = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(fullFileName));


            dataRows.Add(fileDataRow);

        }
        return dataRows;
    }
    }
