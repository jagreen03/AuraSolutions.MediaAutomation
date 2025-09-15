using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

/// <summary>
/// A client for interacting with the Hugging Face Inference API to generate music and voiceovers.
/// </summary>
public class HuggingFaceClient
{
	// A single HttpClient instance should be used for the lifetime of the application.
	private static readonly HttpClient httpClient = new HttpClient();
	private readonly string _apiToken;

	// Replace with your actual API token
	private const string ApiToken = "YOUR_HUGGING_FACE_API_TOKEN";

	/// <summary>
	/// Initializes a new instance of the HuggingFaceClient class.
	/// </summary>
	/// <param name="apiToken">The Hugging Face API token for authentication.</param>
	public HuggingFaceClient(string apiToken)
	{
		_apiToken = apiToken;
		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiToken);
	}

	/// <summary>
	/// Generates music from a text prompt using the MusicGen model.
	/// </summary>
	/// <param name="prompt">The text prompt describing the desired music.</param>
	/// <returns>A byte array containing the generated audio data (e.g., MP3).</returns>
	public async Task<byte[]> GenerateMusicAsync(string prompt)
	{
		// The URL for the MusicGen model on the Hugging Face Inference API.
		var modelUrl = "https://api-inference.huggingface.co/models/facebook/musicgen-large";
		var payload = new { inputs = prompt };
		var jsonPayload = JsonSerializer.Serialize(payload);

		var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

		try
		{
			var response = await httpClient.PostAsync(modelUrl, content);
			response.EnsureSuccessStatusCode(); // Throws an exception on failure

			return await response.Content.ReadAsByteArrayAsync();
		}
		catch(HttpRequestException ex)
		{
			Console.WriteLine($"Error calling Hugging Face API: {ex.Message}");
			throw;
		}
	}

	/// <summary>
	/// Generates a voiceover from a text prompt using a Text-to-Speech (TTS) model.
	/// </summary>
	/// <param name="prompt">The text to be converted to speech.</param>
	/// <returns>A byte array containing the generated audio data.</returns>
	public async Task<byte[]> GenerateVoiceoverAsync(string prompt)
	{
		// Use a Text-to-Speech model URL from Hugging Face.
		// You may need to change this URL to a model you prefer.
		var modelUrl = "https://api-inference.huggingface.co/models/facebook/mms-tts-eng";
		var payload = new { inputs = prompt };
		var jsonPayload = JsonSerializer.Serialize(payload);

		var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

		try
		{
			var response = await httpClient.PostAsync(modelUrl, content);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsByteArrayAsync();
		}
		catch(HttpRequestException ex)
		{
			Console.WriteLine($"Error calling Hugging Face TTS API: {ex.Message}");
			throw;
		}
	}

	/// <summary>
	/// The main entry point of the program.
	/// </summary>
	public static async Task Main(string[] args)
	{
		if(string.IsNullOrEmpty(ApiToken) || ApiToken == "YOUR_HUGGING_FACE_API_TOKEN")
		{
			Console.WriteLine("Please replace 'YOUR_HUGGING_FACE_API_TOKEN' with your actual token.");
			return;
		}

		var client = new HuggingFaceClient(ApiToken);

		// Music Generation
		var musicPrompt = "light, calm, inspirational music with a gentle piano melody, upbeat drums, and a hint of a soft synth pad";
		Console.WriteLine($"Generating music for prompt: '{musicPrompt}'...");
		try
		{
			var musicData = await client.GenerateMusicAsync(musicPrompt);
			var musicFilePath = "generated_music.mp3";
			await File.WriteAllBytesAsync(musicFilePath, musicData);
			Console.WriteLine($"Music successfully generated and saved to {musicFilePath}");
		}
		catch(Exception ex)
		{
			Console.WriteLine($"An error occurred during music generation: {ex.Message}");
		}

		Console.WriteLine(); // Add a blank line for readability

		// Voiceover Generation
		var voiceoverPrompt = "Hello, this is a test of the Text-to-Speech API.";
		Console.WriteLine($"Generating voiceover for prompt: '{voiceoverPrompt}'...");
		try
		{
			var voiceoverData = await client.GenerateVoiceoverAsync(voiceoverPrompt);
			var voiceoverFilePath = "generated_voiceover.wav"; // Hugging Face TTS models often return WAV files
			await File.WriteAllBytesAsync(voiceoverFilePath, voiceoverData);
			Console.WriteLine($"Voiceover successfully generated and saved to {voiceoverFilePath}");
		}
		catch(Exception ex)
		{
			Console.WriteLine($"An error occurred during voiceover generation: {ex.Message}");
		}
	}
}
