using System.IO;
using Tldr.Core;

namespace Tldr;

/// <summary>
/// Console smoke test for Foundry Local integration (Phase 2) and PromptBuilder (Phase 3).
/// Run with: dotnet run --project src/Tldr -- --smoke-test
/// </summary>
public static class SmokeTest
{
    public static async Task RunAsync()
    {
        Console.WriteLine("[TLDR Smoke Test]");
        Console.WriteLine();

        // Phase 3: PromptBuilder
        var promptsPath = Path.Combine(AppContext.BaseDirectory, "prompts.json");
        var promptBuilder = new PromptBuilder(promptsPath);
        var systemPrompt = promptBuilder.Build(SummaryStyle.Bullets, DetailLevel.Standard, Tone.Neutral);
        Console.WriteLine($"System prompt ({Summarizer.EstimateTokens(systemPrompt)} est. tokens):");
        Console.WriteLine(systemPrompt);
        Console.WriteLine();

        // Phase 2: Foundry Local
        await using var summarizer = new Summarizer();

        Console.WriteLine("Initializing Foundry Local (first run downloads the model)...");
        await summarizer.InitializeAsync(
            onProgress: p => Console.Write($"\rDownloading model: {p:F1}%"),
            ct: CancellationToken.None);
        Console.WriteLine();
        Console.WriteLine($"Model loaded. Context window: {summarizer.ContextWindowTokens:N0} tokens.");
        Console.WriteLine();

        var input = """
            The James Webb Space Telescope (JWST) is a space telescope designed primarily to conduct 
            infrared astronomy. As the largest optical telescope in space, its high infrared resolution 
            and sensitivity allow it to view objects too old and distant for the Hubble Space Telescope. 
            This is expected to enable a broad range of investigations across the fields of astronomy 
            and cosmology, such as observation of the first stars and the formation of the first galaxies, 
            and detailed atmospheric characterization of potentially habitable exoplanets. JWST was 
            launched on 25 December 2021 on an Ariane 5 rocket from Kourou, French Guiana, and arrived 
            at the Sun-Earth L2 Lagrange point in January 2022. The telescope is named after James E. Webb, 
            who was the administrator of NASA from 1961 to 1968 during the Mercury, Gemini, and Apollo 
            programs. The first JWST image was released to the public via a White House briefing on 
            11 July 2022. The telescope has a mass of about 6,500 kilograms and is positioned about 
            1.5 million kilometers from Earth. Its primary mirror is 6.5 meters in diameter, composed 
            of 18 hexagonal gold-plated beryllium segments. The cost of the project is about US$10 billion, 
            making it one of the most expensive scientific instruments ever built.
            """;

        Console.WriteLine($"Input: {input.Length} chars (~{Summarizer.EstimateTokens(input)} tokens)");
        Console.WriteLine("Summarizing...");
        Console.WriteLine();

        var summary = await summarizer.SummarizeAsync(input, systemPrompt);
        Console.WriteLine("--- Summary ---");
        Console.WriteLine(summary);
        Console.WriteLine("--- End ---");
        Console.WriteLine();
        Console.WriteLine("[OK] Smoke test passed.");
    }
}
