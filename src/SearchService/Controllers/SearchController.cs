using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Item>>> SearchItems(string searchTerm)
    {
        var query = DB.Find<Item>();
        
        query.Sort(_ => _.Ascending(a => a.Make));

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query.Match(Search.Full, searchTerm).SortByTextScore();
        }

        var result = await query.ExecuteAsync();

        return result;
    }

}
