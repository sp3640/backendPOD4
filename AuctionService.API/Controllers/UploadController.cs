using Microsoft.AspNetCore.Mvc;

// ADD THIS NAMESPACE to use ApiExplorerSettings
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace OnlineAuctionSystem.AuctionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public UploadController(IWebHostEnvironment environment)
        {
            // This 'environment' service helps us find the 'wwwroot' folder
            _environment = environment;
        }

        [HttpPost("upload")]

        // ✅ FIX 1: This hides the endpoint from Swagger, fixing the backend crash.
        [ApiExplorerSettings(IgnoreApi = true)]

        // ✅ FIX 2: This correctly reads the file from the form data.
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                // This is the "file field is required" error, but now 
                // [FromForm] will bind it correctly.
                return BadRequest(new { message = "No file uploaded." });
            }

            // Get the path to wwwroot/uploads
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");

            // Create the directory if it doesn't exist
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Create a unique file name to prevent files from overriding each other
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            // Save the file to the server's disk
            try
            {
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }

            // Build the public URL to return to the React frontend
            // This will look like: http://localhost:5115/uploads/xxxxxxxx-xxxx.jpg
            var request = HttpContext.Request;
            var fileUrl = $"{request.Scheme}://{request.Host}/uploads/{uniqueFileName}";

            // Return the URL in a JSON object
            return Ok(new { url = fileUrl });
        }
    }
}