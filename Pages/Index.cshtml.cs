using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using AikidoLive.Services.DBConnector;
using AikidoLive.DataModels;

namespace AikidoLive.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    private readonly DBServiceConnector _dbServiceConnector;    
    private readonly IConfiguration _configuration;
    public List<UserList> _libUserList;

    public IndexModel(ILogger<IndexModel> logger, DBServiceConnector dbServiceConnector, IConfiguration configuration)
    {
        _logger = logger;
        _dbServiceConnector = dbServiceConnector;
        _configuration = configuration;
        _libUserList = new List<UserList>();
    }

    public async Task OnGetAsync()
    {
        var databases = await DBServiceConnector.CreateAsync(_configuration);

        _libUserList = await databases.GetUsers();
    }
}