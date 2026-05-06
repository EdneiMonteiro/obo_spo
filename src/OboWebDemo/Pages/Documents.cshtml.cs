// Copyright (c) 2026 Ednei Monteiro. Licensed under the MIT License.
// See LICENSE and DISCLAIMER.md in the project root for details.

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using OboWebDemo.Models;
using OboWebDemo.Services;

namespace OboWebDemo.Pages;

[AuthorizeForScopes(ScopeKeySection = "OboDemo:GraphScopes")]
public class DocumentsModel : PageModel
{
    private readonly OboSharePointService _service;
    private readonly ILogger<DocumentsModel> _logger;

    public DocumentsModel(OboSharePointService service, ILogger<DocumentsModel> logger)
    {
        _service = service;
        _logger = logger;
    }

    public OboResult? Result { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            Result = await _service.GetDocumentsAsync(HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SharePoint documents");
            ErrorMessage = ex.Message;
        }
    }
}
