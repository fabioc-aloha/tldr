// generate-banner.js — One-shot README banner generator for TLDR
// Usage: node scripts/generate-banner.js
import Replicate from "replicate";
import { writeFile } from "node:fs/promises";
import { readFileSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const rootDir = resolve(__dirname, "..");

// Load .env manually (no dotenv dependency)
const envPath = resolve(rootDir, ".env");
const envContent = readFileSync(envPath, "utf-8");
for (const line of envContent.split("\n")) {
  const match = line.match(/^([A-Z_]+)=(.+)$/);
  if (match) process.env[match[1]] = match[2].trim();
}

const replicate = new Replicate({ auth: process.env.REPLICATE_API_TOKEN });

const prompt = `Professional technology banner, ultra-wide 3:1 cinematic format.

CENTER TEXT — exactly two lines, nothing else:
Line 1: "TLDR" in large bold white sans-serif uppercase letters with a subtle blue glow. No superscript, no symbols, no extra characters — just the four letters T L D R.
Line 2: "Local AI Document Summarizer" in smaller white sans-serif beneath. The word AI must be fully uppercase capital A capital I. No lowercase.

LEFT SIDE: Abstract document pages fanning out and dissolving into glowing blue particles streaming toward center.
RIGHT SIDE: Flowing luminous lines converging into a compact bright point of light, suggesting condensed information.

BACKGROUND: Deep smooth gradient from dark navy #0a192f on edges to midnight blue #1a365d at center. Subtle geometric grid at low opacity. Scattered soft bokeh particles in electric blue #60cdff and cyan #38bdf8.

STYLE: Photorealistic 3D rendering, modern tech startup aesthetic, cinematic quality, minimalist but impactful. Soft ambient glow from center, electric blue accent lighting.

CRITICAL: Only two text elements in the entire image. No other words, labels, watermarks, or symbols anywhere. Perfect spelling. Crystal clear typography.`;

console.log("Generating banner with Ideogram v3 Turbo...");
console.log(`Prompt length: ${prompt.length} chars`);

const output = await replicate.run("ideogram-ai/ideogram-v3-turbo", {
  input: {
    prompt,
    aspect_ratio: "3:1",
    magic_prompt_option: "On",
    output_format: "png",
  },
});

// Handle Ideogram URL quirk
let imageUrl;
if (output && typeof output.url === "function") {
  imageUrl = output.url().toString();
} else if (output && typeof output === "object" && output.href) {
  imageUrl = output.href;
} else if (typeof output === "string") {
  imageUrl = output;
} else {
  // Replicate sometimes returns FileOutput
  imageUrl = String(output);
}

console.log(`Downloading from: ${imageUrl}`);
const response = await fetch(imageUrl);
if (!response.ok) throw new Error(`Download failed: ${response.status}`);

const buffer = Buffer.from(await response.arrayBuffer());
const outPath = resolve(rootDir, "docs", "assets", "banner.png");

// Ensure directory exists
const { mkdirSync } = await import("node:fs");
mkdirSync(dirname(outPath), { recursive: true });

await writeFile(outPath, buffer);
console.log(
  `Banner saved to ${outPath} (${(buffer.length / 1024).toFixed(0)} KB)`,
);
