# AuraSolutions.MediaAutomation

This repository contains a C# console application designed to automate the process of creating media content for a YouTube channel. The application interacts with various AI APIs (like Hugging Face) to generate assets such as music, voiceovers, and videos, and then uses a powerful tool like FFmpeg to assemble them into a final video file.

This project is intended as a central hub for an automated content creation pipeline, following solid software design principles like Separation of Concerns.

## Getting Started

### Prerequisites

- .NET 8 SDK
    
- A Hugging Face API token
    

### Setup

1. **Clone the Repository:** Create a new repository on your GitHub account and clone this project into it.
    
2. **Open the Solution:** Open the `AuraSolutions.MediaAutomation.sln` file in Visual Studio or your preferred C# editor (e.g., VS Code).
    
3. **Add Your API Token:** Open the `Program.cs` file and replace `"YOUR_HUGGING_FACE_API_TOKEN"` with your actual token.
    

```
private const string ApiToken = "YOUR_HUGGING_FACE_API_TOKEN";
```

### Usage

Run the console application from your editor or the terminal.

```
dotnet run
```

The application will use your prompt to generate a music file and save it to the project's root directory. The next steps will involve adding code to generate voiceovers and videos, and then using FFmpeg to combine them.