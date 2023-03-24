using Sample_Librarian.Model;
using Sample_Librarian.Services;
using Plugin.Maui.Audio;
using System.Diagnostics;
using System.IO;

namespace Sample_Librarian.Services;
    public class FileDataRowService
    {
        List<FileDataRow> dataRows = new List<FileDataRow>();

    public async Task<List<FileDataRow>> GetFileDataRows(string filePath)
    {
        string localFilePathDirectory = @"X:\Programming\Projects\0323\Sample-Librarian\Resources\Raw";
        System.IO.DirectoryInfo directoryInfo = new DirectoryInfo(localFilePathDirectory);
        try
        {
            if (directoryInfo.GetFiles().Length > 0)
            {
                await Task.Run(() =>
                {
                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        file.Delete();
                    }
                });
            }
            if (dataRows.Count > 0) { dataRows.Clear(); };
            string[] files = Directory.GetFiles(filePath);
            for (int i = 0; i < files.Length; i++)
            {

                var fileDataRow = new FileDataRow();
                var fileInfo = new FileInfo(files[i]);
                fileDataRow.Id = i;
                fileDataRow.Format = fileInfo.Extension;
                fileDataRow.FileName = Path.GetFileNameWithoutExtension(files[i]);
                fileDataRow.Size = fileInfo.Length.ToString();
                fileDataRow.FilePath = files[i];
                if (fileDataRow.Format == ".wav" || fileDataRow.Format == ".mp3")
                {
                    string fullFileName = $"{fileDataRow.FileName}{fileDataRow.Format}";
                    string copiedFilePath = $"{localFilePathDirectory}\\{fullFileName}";
                    await Task.Run(() => { File.Copy(files[i], copiedFilePath); });
                    fileDataRow.LocalFilePath = copiedFilePath;
                }
                dataRows.Add(fileDataRow);
            }
            for (int j = 0; j < dataRows.Count; j++)
            {
                if (dataRows[j].Format == ".wav" || dataRows[j].Format == ".mp3")
                {
                    dataRows[j].Player = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(dataRows[j].LocalFilePath));
                    dataRows[j].HasPlayer = true;
                }
                else { dataRows[j].HasPlayer = false; }
            }
            return dataRows;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return dataRows;
        }
        
    }
}
