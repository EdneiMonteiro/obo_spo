/*
This Sample Code is provided for the purpose of illustration only and is not
intended to be used in a production environment. THIS SAMPLE CODE AND ANY
RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER
EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF
MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.

We grant You a nonexclusive, royalty-free right to use and modify the Sample
Code and to reproduce and distribute the object code form of the Sample Code,
provided that You agree: (i) to not use Our name, logo, or trademarks to market
Your software product in which the Sample Code is embedded; (ii) to include a
valid copyright notice on Your software product in which the Sample Code is
embedded; and (iii) to indemnify, hold harmless, and defend Us and Our
suppliers from and against any claims or lawsuits, including attorneys' fees,
that arise or result from the use or distribution of the Sample Code.

Please note: None of the conditions outlined in the disclaimer above will
supersede the terms and conditions contained within the Customers Support
Services Description.
*/

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
