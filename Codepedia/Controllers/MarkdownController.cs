using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Codepedia
{
    using static Functions;

    [Route("api/[controller]")]
    [ApiController]
    public class MarkdownController : ControllerBase
    {
        public string Post ([FromForm] string markdown)
        {
            return DisplayMarkdown(markdown, colorCode: true);
        }
    }
}
