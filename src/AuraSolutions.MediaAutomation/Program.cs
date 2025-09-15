using AuraSolutions.Core;

using System;
using System.IO;
using System.Threading.Tasks;

public class Program
{
	private const string ApiToken = "YOUR_HUGGING_FACE_API_TOKEN";

	public static async Task Main(string[] args)
	{
		// Set the path for the FFmpeg executables. This will handle the download automatically
		// when a conversion is requested.
		Xabe.FFmpeg.FFmpeg.SetExecutablesPath(Path.Combine(AppContext.BaseDirectory, "ffmpeg"));

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

		// Video Generation and Assembly
		var imageSourcePath = Path.Combine(AppContext.BaseDirectory, "image_source.jpg");
		var videoFilePath = Path.Combine(AppContext.BaseDirectory, "video_source.mp4");
		var outputVideoPath = "final_output.mp4";
		var videoDuration = 10; // seconds

		if(!File.Exists(imageSourcePath))
		{
			Console.WriteLine($"The image source file '{imageSourcePath}' was not found. Please add a JPG file to the project's output folder (bin\\Debug\\net8.0).");
			return;
		}

		// Create the placeholder video from the image
		await client.CreateVideoFromImageAsync(imageSourcePath, videoDuration, videoFilePath);

		// Combine the newly created video with the audio files
		await client.CombineAudioAndVideoAsync(videoFilePath, new[] { musicFilePath, voiceoverFilePath }, outputVideoPath);
	}
}