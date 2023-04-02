using Sample_Librarian.Model;
using Sample_Librarian.Services;
using Plugin.Maui.Audio;
using System.Diagnostics;
using System.IO;

namespace Sample_Librarian.Services;
    public class FileDataRowService
    {
        List<FileDataRow> dataRows = new List<FileDataRow>();

    public List<FileDataRow> GetFileDataRows(string filePath)
    {
        try
        {
            
            if (dataRows.Count > 0) { dataRows.Clear(); };
            string[] files = Directory.GetFiles(filePath, "*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {

                var fileDataRow = new FileDataRow();
                var fileInfo = new FileInfo(files[i]);
                fileDataRow.Id = i;
                fileDataRow.Format = fileInfo.Extension;
                fileDataRow.FileName = Path.GetFileNameWithoutExtension(files[i]);
                fileDataRow.Size = fileInfo.Length.ToString();
                fileDataRow.FilePath = files[i];
                fileDataRow.IsChangingName = false;
                fileDataRow.IsNotChangingName = true;
                if (fileDataRow.Format == ".wav" || fileDataRow.Format == ".mp3")
                {
                    Stream stream = new FileStream(fileDataRow.FilePath, FileMode.Open, FileAccess.Read);
                    fileDataRow.Player = AudioManager.Current.CreatePlayer(stream);
                    fileDataRow.HasPlayer = true;
                    fileDataRow.PlayerIcon = "play_icon.png";
                }
                dataRows.Add(fileDataRow);
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
