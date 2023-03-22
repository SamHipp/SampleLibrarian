using Sample_Librarian.Model;
using Sample_Librarian.Services;
using Plugin.Maui.Audio;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using System.Diagnostics;

namespace Sample_Librarian.Services;
public class SourceFolderService
{
    SourceFolder sourceFolder = new SourceFolder();

    public async Task<SourceFolder> GetSourceFolder(CancellationToken cancellationToken)
    {
        try
        {
            var result = await FolderPicker.Default.PickAsync(cancellationToken);
            result.EnsureSuccess();
            sourceFolder.FilePath = result.Folder.Path;
            sourceFolder.Name = result.Folder.Name;
            sourceFolder.IsSelected = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        return sourceFolder;
    }
}
