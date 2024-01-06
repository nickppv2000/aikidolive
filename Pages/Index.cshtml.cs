using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using AikidoLive.Services;
using AikidoLive.Services.DBConnector;
using AikidoLive.DataModels;

namespace AikidoLive.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    private readonly DBServiceConnector _dbServiceConnector;    
    private readonly IConfiguration _configuration;
    private readonly UserService _userService;

    

    public IndexModel(ILogger<IndexModel> logger, DBServiceConnector dbServiceConnector, IConfiguration configuration, UserService userService)
    {
        _logger = logger;
        _dbServiceConnector = dbServiceConnector;
        _configuration = configuration;
        _userService = userService;
    }

    public async Task OnGetAsync()
    {
        
    }
}