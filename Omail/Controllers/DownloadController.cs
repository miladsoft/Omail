using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Omail.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DownloadController : ControllerBase
    {
        [HttpGet("recoverycodes")]
        public IActionResult DownloadRecoveryCodes(string codes)
        {
            if (string.IsNullOrEmpty(codes))
            {
                return BadRequest("No recovery codes provided");
            }

            var decodedCodes = System.Net.WebUtility.UrlDecode(codes);
            var codesList = decodedCodes.Split(',');
            var content = string.Join(Environment.NewLine, codesList);
            
            var bytes = Encoding.UTF8.GetBytes(content);
            return File(bytes, "text/plain", "omail-recovery-codes.txt");
        }
    }
}
