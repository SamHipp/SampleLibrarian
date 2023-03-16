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
        string filepath = @"X:\Programming\Projects\0323\Sample-Librarian\Resources\Raw";
        string[] files = Directory.GetFiles(filepath);
        for (int i = 0; i < files.Length; i++)
        {
            var fileDataRow = new FileDataRow();
            var fileInfo = new FileInfo(files[i]);
            fileDataRow.Id = i;
            fileDataRow.Format = fileInfo.Extension;
            fileDataRow.FileName = Path.GetFileNameWithoutExtension(files[i]);
            fileDataRow.Size = fileInfo.Length.ToString();
            fileDataRow.FilePath = files[i];
            string fullFileName = $"{fileDataRow.FileName}{fileDataRow.Format}";
            fileDataRow.Player = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(fullFileName));


            dataRows.Add(fileDataRow);
        }
        return dataRows;
    }
    }
