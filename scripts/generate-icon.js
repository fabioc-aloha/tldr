// generate-icon.js — One-shot app icon generator for TLDR
// Usage: node scripts/generate-icon.js
import Replicate from "replicate";
import { writeFile } from "node:fs/promises";
import { readFileSync, mkdirSync } from "node:fs";
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

const prompt = `Square image, edge-to-edge design filling the ENTIRE canvas. No border, no padding, no inset shape, no rounded rectangle floating on a background.

The gradient fills the full square from corner to corner: dark navy #0a192f at top-left blending to midnight blue #1a365d at bottom-right, with a subtle electric blue #60cdff glow in the center.

Centered on this gradient, the four letters "TLDR" in bold white sans-serif uppercase. Large, crisp, high contrast. Subtle blue glow behind the text.

STYLE: Flat, clean, vector-like. No 3D, no shadows, no rounded corners, no icon-within-icon. The gradient IS the background, edge to edge.

CRITICAL: Only the letters T L D R. No other text, watermarks, or symbols. The design must touch all four edges of the canvas with no visible margin or frame.`;

console.log("Generating icon with Ideogram v3 Turbo...");
console.log(`Prompt length: ${prompt.length} chars`);

const output = await replicate.run("ideogram-ai/ideogram-v3-turbo", {
  input: {
    prompt,
    aspect_ratio: "1:1",
    magic_prompt_option: "On",
    output_format: "png",
  },
});

let imageUrl;
if (output && typeof output.url === "function") {
  imageUrl = output.url().toString();
} else if (output && typeof output === "object" && output.href) {
  imageUrl = output.href;
} else if (typeof output === "string") {
  imageUrl = output;
} else {
  imageUrl = String(output);
}

console.log(`Downloading from: ${imageUrl}`);
const response = await fetch(imageUrl);
if (!response.ok) throw new Error(`Download failed: ${response.status}`);

const buffer = Buffer.from(await response.arrayBuffer());
const pngPath = resolve(rootDir, "src", "Tldr", "Assets", "icon.png");
mkdirSync(dirname(pngPath), { recursive: true });
await writeFile(pngPath, buffer);
console.log(
  `Icon PNG saved to ${pngPath} (${(buffer.length / 1024).toFixed(0)} KB)`,
);
console.log("\nTo convert to .ico, run:");
console.log(
  `  magick "${pngPath}" -define icon:auto-resize=256,64,48,32,16 "${resolve(dirname(pngPath), "icon.ico")}"`,
);
