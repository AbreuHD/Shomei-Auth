namespace Shomei.Infraestructure.Identity.DTOs.Generic
{
    /// <summary>
    /// A generic response class for API calls.
    /// </summary>
    /// <typeparam name="DTO">The type of the payload returned by the API call.</typeparam>
    public class GenericApiResponse<DTO>
    {
        /// <summary>
        /// The payload data returned from the API call.
        /// </summary>
        public DTO? Payload { get; set; }

        /// <summary>
        /// Indicates whether the API call was successful or not.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// The HTTP status code representing the result of the API call.
        /// </summary>
        public required int Statuscode { get; set; }

        /// <summary>
        /// A message describing the result of the API call.
        /// </summary>
        public required string Message { get; set; }
    }
}
