using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

/// <summary>
/// A client for interacting with the Hugging Face Inference API and FFmpeg for media automation.
/// </summary>
public class MediaAutomationClient
{
	// A single HttpClient instance should be used for the lifetime of the application.
	private static readonly HttpClient httpClient = new HttpClient();
	private readonly string _apiToken;

	// Replace with your actual API token
	private const string ApiToken = "YOUR_HUGGING_FACE_API_TOKEN";

	/// <summary>
	/// Initializes a new instance of the MediaAutomationClient class.
	/// </summary>
	/// <param name="apiToken">The Hugging Face API token for authentication.</param>
	public MediaAutomationClient(string apiToken)
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
	/// <returns>A byte array containing the generated audio data.</param>
	public async Task<byte[]> GenerateVoiceoverAsync(string prompt)
	{
		// Use a Text-to-Speech model URL from Hugging Face.
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
	/// Combines a video file with one or more audio files using FFmpeg.
	/// </summary>
	/// <param name="videoFilePath">The path to the input video file.</param>
	/// <param name="audioFilePaths">An array of paths to the audio files to combine.</param>
	/// <param name="outputFilePath">The desired path for the output video file.</param>
	public async Task CombineAudioAndVideoAsync(string videoFilePath, string[] audioFilePaths, string outputFilePath)
	{
		Console.WriteLine("Starting video and audio combination...");

		try
		{
			var conversion = FFmpeg.Conversions.New();

			// Get the video media info and add the video stream
			var videoInfo = await FFmpeg.GetMediaInfo(videoFilePath);
			conversion.AddStream(videoInfo.VideoStreams);

			// Get the audio media info for each audio file and add the audio streams
			foreach(var audioPath in audioFilePaths)
			{
				var audioInfo = await FFmpeg.GetMediaInfo(audioPath);
				conversion.AddStream(audioInfo.AudioStreams);
			}

			// Set the output path and start the conversion.
			await conversion.SetOutput(outputFilePath).Start();

			Console.WriteLine($"Video successfully created and saved to {outputFilePath}");
		}
		catch(Exception ex)
		{
			Console.WriteLine($"An error occurred during video assembly: {ex.Message}");
			throw;
		}
	}

	/// <summary>
	/// The main entry point of the program.
	/// </summary>
	public static async Task Main(string[] args)
	{
		// Set the path for the FFmpeg executables. This will handle the download automatically
		// when a conversion is requested.
		FFmpeg.SetExecutablesPath(Path.Combine(AppContext.BaseDirectory, "ffmpeg"));

		if(string.IsNullOrEmpty(ApiToken) || ApiToken == "YOUR_HUGGING_FACE_API_TOKEN")
		{
			Console.WriteLine("Please replace 'YOUR_HUGGING_FACE_API_TOKEN' with your actual token.");
			return;
		}

		var client = new MediaAutomationClient(ApiToken);

		// Music Generation
		var musicPrompt = "light, calm, inspirational music with a gentle piano melody, upbeat drums, and a hint of a soft synth pad";
		var musicFilePath = "generated_music.mp3";
		Console.WriteLine($"Generating music for prompt: '{musicPrompt}'...");
		try
		{
			var musicData = await client.GenerateMusicAsync(musicPrompt);
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
		var voiceoverFilePath = "generated_voiceover.wav"; // Hugging Face TTS models often return WAV files
		Console.WriteLine($"Generating voiceover for prompt: '{voiceoverPrompt}'...");
		try
		{
			var voiceoverData = await client.GenerateVoiceoverAsync(voiceoverPrompt);
			await File.WriteAllBytesAsync(voiceoverFilePath, voiceoverData);
			Console.WriteLine($"Voiceover successfully generated and saved to {voiceoverFilePath}");
		}
		catch(Exception ex)
		{
			Console.WriteLine($"An error occurred during voiceover generation: {ex.Message}");
		}

		Console.WriteLine();

		// Video Assembly
		// This is the source video file that the audio will be combined with.
		// For now, you will need to manually place a video file in the output folder.
		var videoFilePath = Path.Combine(AppContext.BaseDirectory, "video_source.mp4");
		var outputVideoPath = "final_output.mp4";

		if(!File.Exists(videoFilePath))
		{
			Console.WriteLine($"The video source file '{videoFilePath}' was not found. Please add a video file to the project's output folder (bin\\Debug\\net8.0).");
			return;
		}

		await client.CombineAudioAndVideoAsync(videoFilePath, new[] { musicFilePath, voiceoverFilePath }, outputVideoPath);
	}
}
