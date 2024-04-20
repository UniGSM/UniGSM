using System.Web;
using GsmCore.DTO;
using GsmCore.Params;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeTypes;
using Swashbuckle.AspNetCore.Annotations;

namespace GsmApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FileController : ControllerBase
{
    [SwaggerResponse(200, "List of files", typeof(ServerFile[]))]
    [HttpGet("list", Name = nameof(ListFiles))]
    public IActionResult ListFiles([FromQuery] string directory)
    {
        directory = HttpUtility.UrlDecode(directory);

        var files = Directory.GetFiles(directory);
        var folders = Directory.GetDirectories(directory);
        var serverFiles = new ServerFile[files.Length + folders.Length];

        for (var i = 0; i < files.Length; i++)
        {
            var fileInfo = new FileInfo(files[i]);
            serverFiles[i] = new ServerFile
            {
                Name = fileInfo.Name,
                Size = fileInfo.Length,
                IsFile = true,
                IsSymlink = fileInfo.LinkTarget != null,
                IsEditable = true,
                MimeType = MimeTypeMap.GetMimeType(fileInfo.Extension),
                ModifiedAt = fileInfo.LastWriteTime,
                CreatedAt = fileInfo.CreationTime
            };
        }

        for (var i = 0; i < folders.Length; i++)
        {
            var folderInfo = new DirectoryInfo(folders[i]);
            serverFiles[files.Length + i] = new ServerFile
            {
                Name = folderInfo.Name,
                Size = 0,
                IsFile = false,
                IsSymlink = folderInfo.LinkTarget != null,
                IsEditable = false,
                MimeType = "inode/directory",
                ModifiedAt = folderInfo.LastWriteTime,
                CreatedAt = folderInfo.CreationTime
            };
        }

        return Ok(serverFiles);
    }

    [SwaggerResponse(200, "The raw file contents", typeof(FileStreamResult))]
    [SwaggerResponse(404, "File not found")]
    [HttpGet("contents", Name = nameof(FileContents))]
    public IActionResult FileContents([FromQuery] string fileName)
    {
        fileName = HttpUtility.UrlDecode(fileName);
        var fileInfo = new FileInfo(fileName);
        if (!fileInfo.Exists) return NotFound();

        return PhysicalFile(fileName, MimeTypeMap.GetMimeType(fileInfo.Extension), true);
    }

    [HttpPut("rename", Name = nameof(RenameFile))]
    public IActionResult RenameFile([FromBody] FileRenameParams fileRenameParams)
    {
        var fileInfo = new FileInfo(fileRenameParams.From);
        if (!fileInfo.Exists) return NotFound();

        fileInfo.MoveTo(fileRenameParams.To);

        return Ok();
    }

    [HttpPost("copy", Name = nameof(CopyFile))]
    public IActionResult CopyFile([FromBody] FileRenameParams fileRenameParams)
    {
        var fileInfo = new FileInfo(fileRenameParams.From);
        if (!fileInfo.Exists) return NotFound();

        fileInfo.CopyTo(fileRenameParams.To);

        return Ok();
    }

    [HttpPost("write", Name = nameof(WriteFile))]
    public async Task<IActionResult> WriteFile([FromQuery] string fileName, [FromBody] Stream content)
    {
        fileName = HttpUtility.UrlDecode(fileName);
        var fileInfo = new FileInfo(fileName);
        if (!fileInfo.Exists) return NotFound();

        await using var fs = new FileStream(fileName, FileMode.Create);
        await content.CopyToAsync(fs);

        return Ok();
    }

    [HttpPost("delete", Name = nameof(DeleteFiles))]
    public IActionResult DeleteFiles([FromBody] FileDeleteParams fileDeleteParams)
    {
        foreach (var fileName in fileDeleteParams.Files)
        {
            var path = Path.Combine(fileDeleteParams.Root, fileName);
            FileInfo fileInfo = new(path);
            fileInfo.Delete();
        }

        return Ok();
    }

    [HttpPost("create-folder", Name = nameof(CreateFolder))]
    public IActionResult CreateFolder([FromBody] FolderCreateParams folderCreateParams)
    {
        var path = Path.Combine(folderCreateParams.Root, folderCreateParams.Name);
        Directory.CreateDirectory(path);

        return Ok();
    }

    [HttpPost("file-upload", Name = nameof(UploadFile))]
    public async Task<IActionResult> UploadFile([FromQuery] string fileName, [FromBody] Stream content)
    {
        fileName = HttpUtility.UrlDecode(fileName);

        await using var fs = new FileStream(fileName, FileMode.Create);
        await content.CopyToAsync(fs);

        return Ok();
    }
}