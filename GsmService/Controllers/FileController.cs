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
[Route("api/server/{serverId:required}/file")]
[Authorize]
public class FileController : ControllerBase
{
    [SwaggerResponse(200, "List of files", typeof(ServerFile[]))]
    [SwaggerResponse(404, "File not found")]
    [SwaggerResponse(403, "Forbidden")]
    [HttpGet("list", Name = nameof(ListFiles))]
    public IActionResult ListFiles([FromQuery] string directory, string serverId)
    {
        directory = HttpUtility.UrlDecode(directory);
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        directory = Path.Combine(rootPath, directory);

        if (!IsPathAllowed(directory, serverId))
        {
            return Forbid();
        }

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

    [SwaggerResponse(200, "The raw file contents")]
    [SwaggerResponse(404, "File not found")]
    [SwaggerResponse(403, "Forbidden")]
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

        if (!IsPathAllowed(fileName, serverId))
        {
            return Forbid();
        }

        return PhysicalFile(fileName, MimeTypeMap.GetMimeType(fileInfo.Extension), true);
    }

    [SwaggerResponse(200, "File renamed")]
    [SwaggerResponse(404, "File not found")]
    [SwaggerResponse(403, "Forbidden")]
    [HttpPut("rename", Name = nameof(RenameFile))]
    public IActionResult RenameFile([FromBody] FileRenameParams fileRenameParams, string serverId)
    {
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        var fileInfo = new FileInfo(Path.Combine(rootPath, fileRenameParams.From));
        if (!fileInfo.Exists) return NotFound();

        var path = Path.Combine(rootPath, fileRenameParams.To);

        if (!IsPathAllowed(path, serverId))
        {
            return Forbid();
        }

        fileInfo.MoveTo(path);

        return Ok();
    }

    [SwaggerResponse(200, "File copied")]
    [SwaggerResponse(404, "File not found")]
    [SwaggerResponse(403, "Forbidden")]
    [HttpPost("copy", Name = nameof(CopyFile))]
    public IActionResult CopyFile([FromBody] FileRenameParams fileRenameParams, string serverId)
    {
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        var fileInfo = new FileInfo(Path.Combine(rootPath, fileRenameParams.From));
        if (!fileInfo.Exists) return NotFound();

        var path = Path.Combine(rootPath, fileRenameParams.To);
        if (!IsPathAllowed(path, serverId))
        {
            return Forbid();
        }

        fileInfo.CopyTo(path);

        return Ok();
    }

    [SwaggerResponse(200, "File written")]
    [SwaggerResponse(404, "File not found")]
    [SwaggerResponse(403, "Forbidden")]
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

        if (!IsPathAllowed(fileName, serverId))
        {
            return Forbid();
        }

        await using var fs = new FileStream(fileName, FileMode.Create);
        await content.CopyToAsync(fs);

        return Ok();
    }

    [SwaggerResponse(200, "File(s) deleted")]
    [SwaggerResponse(404, "File not found")]
    [SwaggerResponse(403, "Forbidden")]
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
            if (!IsPathAllowed(path, serverId))
            {
                return Forbid();
            }

            fileInfo.Delete();
        }

        return Ok();
    }

    [SwaggerResponse(200, "Folder created")]
    [SwaggerResponse(404, "File not found")]
    [SwaggerResponse(403, "Forbidden")]
    [HttpPost("create-folder", Name = nameof(CreateFolder))]
    public IActionResult CreateFolder([FromBody] FolderCreateParams folderCreateParams, string serverId)
    {
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        var path = Path.Combine(rootPath, folderCreateParams.Root, folderCreateParams.Name);
        if (!IsPathAllowed(path, serverId))
        {
            return Forbid();
        }

        Directory.CreateDirectory(path);

        return Ok();
    }

    [SwaggerResponse(200, "File uploaded")]
    [SwaggerResponse(404, "File not found")]
    [SwaggerResponse(403, "Forbidden")]
    [HttpPost("file-upload", Name = nameof(UploadFile))]
    public async Task<IActionResult> UploadFile([FromQuery] string fileName, [FromBody] Stream content, string serverId)
    {
        if (!PathUtil.TryGetServerDataPath(serverId, out var rootPath))
        {
            return NotFound();
        }

        fileName = HttpUtility.UrlDecode(fileName);
        fileName = Path.Combine(rootPath, fileName);

        if (!IsPathAllowed(fileName, serverId))
        {
            return Forbid();
        }

        await using var fs = new FileStream(fileName, FileMode.Create);
        await content.CopyToAsync(fs);

        return Ok();
    }

    private bool IsPathAllowed(string path, string serverId)
    {
        PathUtil.TryGetServerDataPath(serverId, out var serverRootPath);
        return path.StartsWith(serverRootPath);
    }
}