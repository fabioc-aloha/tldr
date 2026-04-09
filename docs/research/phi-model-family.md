# Microsoft Phi Model Family: Comprehensive Research

> Research compiled from official HuggingFace model cards, Microsoft Learn, Windows Blog, Azure product pages, and the PhiCookBook repository.
> Last updated: July 2025.

## Overview

Phi is Microsoft Research's family of small language models (SLMs) designed to deliver frontier-level reasoning in compact form factors. The core philosophy: high-quality, reasoning-dense training data can compensate for smaller parameter counts, enabling deployment on edge devices, mobile hardware, and resource-constrained environments.

All Phi models (except Phi Silica) are released under the **MIT license** and available on HuggingFace and Azure AI Foundry.

## Model Lineage

### Generation 1: Proof of Concept (2023)

| Model   | Parameters | Context | Training Data                 | Release  | Key Innovation                                |
| ------- | ---------- | ------- | ----------------------------- | -------- | --------------------------------------------- |
| Phi-1   | 1.3B       | 2K      | "Textbook quality" code data  | Jun 2023 | Proved synthetic data quality beats raw scale |
| Phi-1.5 | 1.3B       | 2K      | Extended synthetic + web data | Sep 2023 | Generalized beyond code to reasoning          |
| Phi-2   | 2.7B       | 2K      | 1.4T tokens                   | Dec 2023 | Matched 13B models on reasoning benchmarks    |

**Key paper**: "Textbooks Are All You Need" (arxiv:2306.11644)

The first generation established that carefully curated synthetic data ("textbook quality") could train models that punch far above their parameter weight class.

### Generation 2: Phi-3 Family (2024)

| Model             | Parameters | Context   | Training Data | GPUs         | Training Time | Release  |
| ----------------- | ---------- | --------- | ------------- | ------------ | ------------- | -------- |
| Phi-3-mini (4K)   | 3.8B       | 4K        | 4.9T tokens   | 512 H100-80G | 10 days       | Jun 2024 |
| Phi-3-mini (128K) | 3.8B       | 128K      | 4.9T tokens   | 512 H100-80G | 10 days       | Jun 2024 |
| Phi-3-small       | 7B         | 8K / 128K | -             | -            | -             | Jun 2024 |
| Phi-3-medium      | 14B        | 4K / 128K | -             | -            | -             | Jun 2024 |

**Architecture**: Dense decoder-only Transformer, 32,064 token vocabulary, SFT + DPO post-training.

**Key paper**: "Phi-3 Technical Report" (arxiv:2404.14219)

**Phi-3-mini-4K benchmarks** (verified from HF model card):

| Benchmark                  | Score |
| -------------------------- | ----- |
| MMLU (5-shot)              | 70.9  |
| BigBench Hard CoT (3-shot) | 73.5  |
| GSM8K CoT (8-shot)         | 85.7  |
| HumanEval (0-shot)         | 57.3  |
| GPQA (0-shot)              | 30.6  |
| ARC Challenge (10-shot)    | 86.3  |

### Generation 2.5: Phi-3.5 Family (August 2024)

| Model          | Parameters        | Context | Training Data | GPUs         | Training Time | Languages    |
| -------------- | ----------------- | ------- | ------------- | ------------ | ------------- | ------------ |
| Phi-3.5-mini   | 3.8B              | 128K    | 3.4T tokens   | 512 H100-80G | 10 days       | 23 languages |
| Phi-3.5-MoE    | 42B (6.6B active) | 128K    | -             | -            | -             | Multilingual |
| Phi-3.5-vision | 4.2B              | 128K    | -             | -            | -             | Multilingual |

**Key improvements over Phi-3**: Multilingual support (23 languages), multi-turn conversation quality, long context (128K), and the first Mixture-of-Experts variant.

**Phi-3.5-mini benchmarks** (verified from HF model card):

| Benchmark          | Score |
| ------------------ | ----- |
| MMLU (5-shot)      | 69.0  |
| GSM8K CoT (8-shot) | 86.2  |
| MATH (0-shot CoT)  | 48.5  |
| HumanEval (0-shot) | 62.8  |
| GPQA (0-shot CoT)  | 30.4  |

**Phi-3.5-MoE** is notable as the first Mixture-of-Experts Phi model: 42B total parameters with only 6.6B active per forward pass, achieving efficiency comparable to much larger dense models.

**Phi-3.5-vision** added multimodal (image) input processing to the Phi family for the first time.

**Safety paper**: "Phi-3 Safety Post-Training" (arxiv:2407.13833)

### Generation 3: Phi-4 Family (December 2024 onward)

#### Phi-4 (Flagship)

| Property          | Value                           |
| ----------------- | ------------------------------- |
| Parameters        | 14B                             |
| Architecture      | Dense decoder-only Transformer  |
| Context Length    | 16K tokens                      |
| Training Data     | 9.8T tokens                     |
| Training Hardware | 1,920 H100-80G GPUs             |
| Training Time     | 21 days                         |
| Release Date      | December 12, 2024               |
| License           | MIT                             |
| Primary Language  | English (~8% multilingual data) |

**Benchmarks** (verified from HF model card):

| Benchmark | Score |
| --------- | ----- |
| MMLU      | 84.8  |
| MATH      | 80.4  |
| HumanEval | 82.6  |
| GPQA      | 56.1  |
| DROP      | 75.5  |
| SimpleQA  | 3.0   |

Phi-4 excels at complex reasoning, particularly mathematical problem-solving, outperforming many larger models on competition-level math. The model was introduced with a focus on data quality over data quantity: innovative curriculum-based synthetic data generation, advanced filtering, and post-training alignment.

#### Phi-4-mini

| Property          | Value                          |
| ----------------- | ------------------------------ |
| Parameters        | 3.8B                           |
| Architecture      | Dense decoder-only Transformer |
| Context Length    | 128K tokens                    |
| Vocabulary        | 200,064 tokens                 |
| Training Data     | 5T tokens                      |
| Training Hardware | 512 A100-80G GPUs              |
| Training Time     | 21 days                        |
| Release Date      | February 2025                  |
| License           | MIT                            |
| Languages         | 23                             |

**Architecture changes from Phi-3-mini**: Vocabulary expanded from 32K to 200K tokens, grouped-query attention, shared input/output embedding layer.

**Benchmarks** (verified from HF model card):

| Benchmark | Score |
| --------- | ----- |
| Overall   | 63.5  |
| MMLU      | 67.3  |
| MATH      | 64.0  |
| GSM8K     | 88.6  |

#### Phi-4-multimodal

| Property               | Value                                                   |
| ---------------------- | ------------------------------------------------------- |
| Parameters             | 5.6B                                                    |
| Backbone               | Phi-4-Mini-Instruct + vision/speech encoders + adapters |
| Context Length         | 128K tokens                                             |
| Modalities             | Text + Image + Audio (input), Text (output)             |
| Training Data (text)   | 5T tokens                                               |
| Training Data (speech) | 2.3M hours                                              |
| Training Data (vision) | 1.1T image-text tokens                                  |
| Training Hardware      | 512 A100-80G GPUs                                       |
| Training Time          | 28 days                                                 |
| Release Date           | February 2025                                           |
| License                | MIT                                                     |
| Text Languages         | 23                                                      |
| Audio Languages        | 8                                                       |
| Vision Language        | English                                                 |

Uses **Mixture-of-LoRAs** architecture: separate LoRA adapters for speech and vision modalities attached to the Phi-4-Mini text backbone. Supports up to 64 image frames and 40-second audio clips (30 minutes for summarization tasks).

#### Phi-4-reasoning

| Property          | Value                                          |
| ----------------- | ---------------------------------------------- |
| Parameters        | 14B (finetuned from Phi-4)                     |
| Architecture      | Dense decoder-only Transformer (same as Phi-4) |
| Context Length    | 32K tokens                                     |
| Training Data     | 16B tokens (~8.3B unique)                      |
| Training Hardware | 32 H100-80G GPUs                               |
| Training Time     | 2.5 days                                       |
| Release Date      | April 30, 2025                                 |
| License           | MIT                                            |
| Primary Language  | English                                        |

**Training approach**: Supervised fine-tuning on chain-of-thought traces + reinforcement learning. Dataset blends synthetic prompts and filtered high-quality web data focused on math, science, and coding.

**Output format**: Responses contain two sections: a `<think>` reasoning chain-of-thought block followed by a Solution summary block.

**Benchmarks** (verified from HF model card):

| Benchmark     | Phi-4-reasoning | DeepSeek-R1 | o1-mini | o3-mini |
| ------------- | --------------- | ----------- | ------- | ------- |
| AIME 2025     | 75.3            | 78.7        | 63.6    | 88.0    |
| GPQA Diamond  | 62.9            | 70.4        | 54.8    | 78.0    |
| LiveCodeBench | 65.8            | 73.0        | 60.0    | 77.7    |
| HumanEvalPlus | 83.5            | -           | -       | 88.0    |
| MMLUPro       | 71.5            | -           | -       | 73.0    |

With only 14B parameters, approaches DeepSeek-R1 (671B) on many reasoning tasks. A **Phi-4-reasoning-plus** variant (same architecture, additional RL training) achieves even higher scores: AIME 2025 81.3, GPQA Diamond 78.0.

**Technical report**: arxiv:2504.21318

#### Phi-4-mini-reasoning

| Property          | Value                                  |
| ----------------- | -------------------------------------- |
| Parameters        | 3.8B (same architecture as Phi-4-Mini) |
| Context Length    | 128K tokens                            |
| Vocabulary        | 200,064 tokens                         |
| Training Data     | 150B tokens (fine-tuning)              |
| Training Hardware | 128 H100-80G GPUs                      |
| Training Time     | 2 days                                 |
| Release Date      | April 2025                             |
| License           | MIT                                    |

**Knowledge distillation**: Fine-tuned exclusively on synthetic mathematical content generated by DeepSeek-R1. Over 1 million math problems (middle school to PhD level), 8 solutions sampled per problem, only correct solutions retained (~30B tokens of math content).

**Benchmarks** (verified from HF model card):

| Benchmark    | Phi-4-mini-reasoning (3.8B) | Phi-4-Mini base (3.8B) | DeepSeek-R1-Distill-Qwen-7B | o1-mini |
| ------------ | --------------------------- | ---------------------- | --------------------------- | ------- |
| AIME 2024    | 57.5                        | 10.0                   | 53.3                        | 63.6    |
| MATH-500     | 94.6                        | 71.8                   | 91.4                        | 90.0    |
| GPQA Diamond | 52.0                        | 36.9                   | 49.5                        | 60.0    |

A 3.8B model matching or exceeding 7B distilled reasoning models on math benchmarks.

**Technical report**: arxiv:2504.21233

#### Additional Phi-4 Variants

| Model                        | Status        | Notes                                           |
| ---------------------------- | ------------- | ----------------------------------------------- |
| Phi-4-reasoning-vision (15B) | Released 2025 | Reasoning with vision input capabilities        |
| Phi-4-Mini-Flash-Reasoning   | Released 2025 | Optimized for fast inference on reasoning tasks |

## Phi Silica: On-Device NPU Model

Phi Silica is a distinct variant purpose-built for Windows Copilot+ PCs. It is **not** available as a downloadable model; it ships as a Windows inbox component.

| Property                   | Value                              |
| -------------------------- | ---------------------------------- |
| Base Model                 | Derived from Phi-3.5-mini          |
| Quantization               | 4-bit via QuaRot                   |
| Context Length             | 4K tokens (initially 2K)           |
| Target Hardware            | Snapdragon X Series NPU (40+ TOPS) |
| TTFT (short prompts)       | ~230ms                             |
| Throughput                 | Up to 20 tokens/s                  |
| Power (context processing) | 4.8 mWh on NPU                     |
| License                    | Proprietary (Windows inbox)        |
| Release                    | December 2024                      |

### Key Technical Innovations

**QuaRot** (Outlier-Free 4-Bit Inference in Rotated LLMs): Uses Hadamard rotations to achieve "computational invariance" and "incoherence processing," enabling aggressive 4-bit quantization without the usual accuracy loss from outlier weights.

**Memory optimizations**:
- Weight sharing between context processor and token iterator
- Memory-mapped embeddings (not loaded into RAM)
- Sliding window context (N=64 chunks)
- Dynamic shared KV cache
- Selective mixed precision: 4-8 of 128 weight matrices kept at 8-bit for critical layers
- Conv2D conversion for linear layers
- ~60% memory reduction, 56% power improvement vs CPU execution

**Speculative decoding**: Uses a smaller draft model to propose multiple tokens, then the main model verifies, improving throughput.

### Windows API

Namespace: `Microsoft.Windows.AI.Text`

```csharp
// Create a language model instance
var model = await LanguageModel.CreateAsync();

// Generate a response
var response = await model.GenerateResponseAsync("Summarize this text...");
```

**Text Intelligence Skills** (higher-level APIs):
- `TextSummarizer`: Document summarization
- Text-to-table conversion
- Rewrite/rephrase

**Content moderation**: Built-in `ContentFilterOptions` for safety filtering.

**Availability**: Limited Access Feature requiring an unlock token via Microsoft's LAF form. Part of Windows App SDK. Not available in China.

**Powers**: Click to Do, Word/Outlook rewrite and summarize features on Copilot+ PCs.

## Architecture Evolution

| Feature       | Phi-1/1.5/2       | Phi-3             | Phi-3.5                    | Phi-4             | Phi-4-mini                         |
| ------------- | ----------------- | ----------------- | -------------------------- | ----------------- | ---------------------------------- |
| Architecture  | Dense Transformer | Dense Transformer | Dense Transformer          | Dense Transformer | Dense Transformer                  |
| Vocabulary    | ~51K              | 32,064            | 32,064                     | -                 | 200,064                            |
| Attention     | Standard          | Standard          | Standard                   | Standard          | Grouped-query                      |
| Embedding     | Standard          | Standard          | Standard                   | Standard          | Shared input/output                |
| Context       | 2K                | 4K-128K           | 128K                       | 16K               | 128K                               |
| Post-training | SFT               | SFT + DPO         | SFT + PPO + DPO            | SFT + DPO         | SFT + DPO                          |
| Modalities    | Text              | Text              | Text + Vision (3.5-vision) | Text              | Text + Vision + Audio (multimodal) |

## Key Innovations Across the Family

1. **"Textbooks Are All You Need"**: The foundational insight that synthetic, curriculum-structured training data enables small models to rival much larger ones.

2. **Aggressive data quality filtering**: Public web data filtered for reasoning density, removing factual trivia to preserve model capacity for reasoning patterns.

3. **Synthetic data generation pipelines**: Purpose-built pipelines that generate textbook-quality training examples for math, coding, and common-sense reasoning.

4. **QuaRot quantization** (Phi Silica): Hadamard rotation-based 4-bit quantization that eliminates weight outliers, enabling NPU deployment.

5. **Mixture-of-LoRAs** (Phi-4-multimodal): Lightweight per-modality adapters on a shared text backbone, keeping the base model small while adding vision and speech.

6. **Knowledge distillation from reasoning models** (Phi-4-mini-reasoning): Distilling chain-of-thought reasoning from DeepSeek-R1 into a 3.8B model, with verified-correct solution filtering.

7. **Grouped-query attention** (Phi-4-mini onward): Reduces KV cache memory requirements for long-context inference.

## Deployment Options

| Platform          | Models Available                                 |
| ----------------- | ------------------------------------------------ |
| HuggingFace       | All Phi models (weights + model cards)           |
| Azure AI Foundry  | Phi-3, 3.5, 4, 4-mini, 4-multimodal, 4-reasoning |
| ONNX Runtime      | Phi-3, 3.5, 4-mini (CPU/GPU/Mobile)              |
| Ollama            | Phi-3, 3.5, 4, 4-reasoning                       |
| llama.cpp         | All models via GGUF quantizations                |
| LM Studio         | All models via community quantizations           |
| Windows NPU       | Phi Silica only (inbox, not downloadable)        |
| Foundry Local SDK | Phi-4-mini (on-device inference for apps)        |

## Academic Papers

| Paper                                 | Models                                | ArXiv      |
| ------------------------------------- | ------------------------------------- | ---------- |
| Textbooks Are All You Need            | Phi-1                                 | 2306.11644 |
| Phi-3 Technical Report                | Phi-3 family                          | 2404.14219 |
| Phi-3 Safety Post-Training            | Phi-3/3.5                             | 2407.13833 |
| Phi-4-reasoning Technical Report      | Phi-4-reasoning, Phi-4-reasoning-plus | 2504.21318 |
| Phi-4-mini-reasoning Technical Report | Phi-4-mini-reasoning                  | 2504.21233 |

## Resources

- [Phi Portal (Azure)](https://azure.microsoft.com/en-us/products/phi)
- [Phi CookBook (GitHub)](https://github.com/microsoft/PhiCookBook)
- [HuggingFace Collection: Phi-4](https://huggingface.co/collections/microsoft/phi-4)
- [HuggingFace Collection: Phi-3](https://huggingface.co/collections/microsoft/phi-3)
- [Phi Silica Developer Docs](https://learn.microsoft.com/en-us/windows/ai/apis/phi-silica)
