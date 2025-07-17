using Microsoft.AspNetCore.Http;

namespace SmartBaby.API.Models;

/// <summary>
/// DTO for multimodal analysis upload request
/// </summary>
public class MultimodalUploadRequestDto
{
    /// <summary>
    /// Optional image file for analysis
    /// </summary>
    public IFormFile? ImageFile { get; set; }

    /// <summary>
    /// Optional audio file for analysis
    /// </summary>
    public IFormFile? AudioFile { get; set; }

    /// <summary>
    /// Optional baby ID to associate with analysis
    /// </summary>
    public int? BabyId { get; set; }

    /// <summary>
    /// Minimum confidence threshold for results
    /// </summary>
    public float ConfidenceThreshold { get; set; } = 0.5f;
}
