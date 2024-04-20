using System.Web;
using GsmCore.DTO;
using GsmCore.Params;
using GsmCore.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeTypes;
using Swashbuckle.AspNetCore.Annotations;

namespace GsmApi.Controllers;

[ApiController]
[Route("server/{serverId:required}/file")]
[Authorize]
public class FileController : ControllerBase
{
    [SwaggerResponse(200, "List of files", typeof(ServerFile[]))]
    [HttpGet("list", Name = nameof(ListFiles))]
    public IActionResult ListFiles([FromQuery] string directory, string serverId)
    {
        directory = HttpUtility.UrlDecode(directory);
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        directory = Path.Combine(rootPath, directory);

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
    public IActionResult FileContents([FromQuery] string fileName, string serverId)
    {
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        fileName = HttpUtility.UrlDecode(fileName);
        fileName = Path.Combine(rootPath, fileName);
        var fileInfo = new FileInfo(fileName);
        if (!fileInfo.Exists) return NotFound();

        return PhysicalFile(fileName, MimeTypeMap.GetMimeType(fileInfo.Extension), true);
    }

    [HttpPut("rename", Name = nameof(RenameFile))]
    public IActionResult RenameFile([FromBody] FileRenameParams fileRenameParams, string serverId)
    {
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        var fileInfo = new FileInfo(Path.Combine(rootPath, fileRenameParams.From));
        if (!fileInfo.Exists) return NotFound();

        fileInfo.MoveTo(Path.Combine(rootPath, fileRenameParams.To));

        return Ok();
    }

    [HttpPost("copy", Name = nameof(CopyFile))]
    public IActionResult CopyFile([FromBody] FileRenameParams fileRenameParams, string serverId)
    {
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        var fileInfo = new FileInfo(Path.Combine(rootPath, fileRenameParams.From));
        if (!fileInfo.Exists) return NotFound();

        fileInfo.CopyTo(Path.Combine(rootPath, fileRenameParams.To));

        return Ok();
    }

    [HttpPost("write", Name = nameof(WriteFile))]
    public async Task<IActionResult> WriteFile([FromQuery] string fileName, [FromBody] Stream content, string serverId)
    {
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        fileName = HttpUtility.UrlDecode(fileName);
        fileName = Path.Combine(rootPath, fileName);

        var fileInfo = new FileInfo(fileName);
        if (!fileInfo.Exists) return NotFound();

        await using var fs = new FileStream(fileName, FileMode.Create);
        await content.CopyToAsync(fs);

        return Ok();
    }

    [HttpPost("delete", Name = nameof(DeleteFiles))]
    public IActionResult DeleteFiles([FromBody] FileDeleteParams fileDeleteParams, string serverId)
    {
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        foreach (var fileName in fileDeleteParams.Files)
        {
            var path = Path.Combine(rootPath, fileDeleteParams.Root, fileName);
            FileInfo fileInfo = new(path);
            fileInfo.Delete();
        }

        return Ok();
    }

    [HttpPost("create-folder", Name = nameof(CreateFolder))]
    public IActionResult CreateFolder([FromBody] FolderCreateParams folderCreateParams, string serverId)
    {
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        var path = Path.Combine(rootPath, folderCreateParams.Root, folderCreateParams.Name);
        Directory.CreateDirectory(path);

        return Ok();
    }

    [HttpPost("file-upload", Name = nameof(UploadFile))]
    public async Task<IActionResult> UploadFile([FromQuery] string fileName, [FromBody] Stream content, string serverId)
    {
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        fileName = HttpUtility.UrlDecode(fileName);
        fileName = Path.Combine(rootPath, fileName);

        await using var fs = new FileStream(fileName, FileMode.Create);
        await content.CopyToAsync(fs);

        return Ok();
    }
}