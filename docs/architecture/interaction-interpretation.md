# Atlas Interaction Interpretation Architecture

## Overview

Atlas separates interaction interpretation from language-model infrastructure.

The `Atlas.Interaction` project owns the meaning and semantics of an Atlas interaction. The `Atlas.AI` project provides provider-independent abstractions for language-model generation.

This separation allows Atlas to use either deterministic interpretation or a language model without coupling the AI infrastructure to Atlas-specific interaction concepts.

The current architecture is:

```text
                         AtlasInteraction
                                │
                                ▼
                    IAtlasInteractionInterpreter
                         │                │
                         │                │
                         ▼                ▼
             AtlasInteractionInterpreter   AtlasLanguageModelInteractionInterpreter
                    (deterministic)                │
                                                   ▼
                                  IAtlasStructuredLanguageModel
                                                   │
                                                   ▼
                                  structured model response
                                                   │
                                                   ▼
                                  AtlasInteractionInterpretationParser
                                                   │
                                                   ▼
                               AtlasInteractionInterpretationResult
```

The resulting interpretation is then consumed by `AtlasInteractionProcessor`, which determines whether the interaction can safely proceed.

---

## Project Responsibilities

### Atlas.AI

`Atlas.AI` contains provider-independent language-model abstractions.

It does not know anything about Atlas interactions, memory, commands, handlers, or intents.

The primary abstractions are:

```text
IAtlasLanguageModel
IAtlasStructuredLanguageModel
```

The associated request and response models are:

```text
AtlasLanguageModelRequest
AtlasLanguageModelResponse

AtlasStructuredLanguageModelRequest
AtlasStructuredLanguageModelResponse
```

The structured request identifies the expected response type:

```csharp
new AtlasStructuredLanguageModelRequest(
    prompt,
    typeof(AtlasStructuredInteractionInterpretation));
```

`Atlas.AI` treats the structured result as model-generated content. It does not interpret the meaning of fields such as `Intent`, `Query`, or `MemoryContent`.

### Atlas.Interaction

`Atlas.Interaction` owns interaction semantics.

It defines:

```text
AtlasInteraction
AtlasInteractionIntent
AtlasInteractionInterpretation
AtlasInteractionInterpretationResult
AtlasStructuredInteractionInterpretation
AtlasInteractionConfidence
AtlasInteractionInterpreterMode
```

It also owns the interpretation implementations:

```text
AtlasInteractionInterpreter
AtlasLanguageModelInteractionInterpreter
```

and the structured-output parser:

```text
AtlasInteractionInterpretationParser
```

This means `Atlas.Interaction` is responsible for translating model output into something that Atlas understands.

---

## Interaction Interpretation

All interaction interpreters implement:

```csharp
IAtlasInteractionInterpreter
```

The interface is asynchronous because language-model interpretation requires asynchronous I/O.

Conceptually:

```csharp
Task<AtlasInteractionInterpretationResult> InterpretAsync(
    AtlasInteraction interaction,
    CancellationToken cancellationToken = default);
```

The processor does not need to know which implementation is being used.

It only depends on:

```text
IAtlasInteractionInterpreter
```

This creates a replaceable interpretation boundary.

---

## Deterministic Interpretation

The deterministic implementation is:

```text
AtlasInteractionInterpreter
```

It uses the existing Atlas heuristics:

```text
IAtlasInteractionIntentDetector
IAtlasInteractionQueryExtractor
IAtlasInteractionMemoryContentExtractor
```

The process is:

```text
AtlasInteraction
       │
       ▼
IntentDetector
       │
       ├── SearchMemory
       │       ↓
       │   QueryExtractor
       │
       ├── StoreMemory
       │       ↓
       │   MemoryContentExtractor
       │
       └── Unknown
```

The result is converted into an `AtlasInteractionInterpretationResult`.

For the deterministic implementation, confidence currently represents the certainty of the deterministic rules:

```text
Known supported intent
    → High confidence
    → Not ambiguous

Unknown intent
    → Low confidence
    → Ambiguous
```

This is deliberately simple. It does not attempt to turn heuristic matching into a numerical probability.

---

## Language-Model Interpretation

The language-model implementation is:

```text
AtlasLanguageModelInteractionInterpreter
```

It depends on:

```text
IAtlasStructuredLanguageModel
IAtlasInteractionInterpretationParser
```

Its process is:

```text
AtlasInteraction
       │
       ▼
Build structured language-model request
       │
       ▼
IAtlasStructuredLanguageModel
       │
       ▼
AtlasStructuredLanguageModelResponse
       │
       ▼
JSON content
       │
       ▼
AtlasInteractionInterpretationParser
       │
       ▼
AtlasInteractionInterpretationResult
```

The AI implementation therefore does not directly construct `AtlasInteractionInterpretation`.

Instead, model output is first represented as:

```text
AtlasStructuredInteractionInterpretation
```

and validated before becoming the internal interpretation result.

---

## Structured Interpretation Contract

The structured model output currently contains:

```text
Intent
Query
MemoryContent
Confidence
IsAmbiguous
```

A valid search interpretation looks conceptually like:

```json
{
  "intent": "SearchMemory",
  "query": "camera",
  "memoryContent": null,
  "confidence": "High",
  "isAmbiguous": false
}
```

A valid store-memory interpretation looks like:

```json
{
  "intent": "StoreMemory",
  "query": null,
  "memoryContent": "I bought a Canon EOS 350D camera.",
  "confidence": "High",
  "isAmbiguous": false
}
```

An ambiguous interpretation may look like:

```json
{
  "intent": "SearchMemory",
  "query": "camera",
  "memoryContent": null,
  "confidence": "Medium",
  "isAmbiguous": true
}
```

---

## Structured Interpretation Validation

The parser validates the semantic relationship between the fields.

### SearchMemory

A `SearchMemory` interpretation requires:

```text
Query != null
MemoryContent == null
```

### StoreMemory

A `StoreMemory` interpretation requires:

```text
MemoryContent != null
Query == null
```

### Unknown

An `Unknown` interpretation requires:

```text
Query == null
MemoryContent == null
```

Required values cannot be empty or whitespace.

The parser also validates confidence and ambiguity:

```text
High + ambiguous
    → invalid

Low + non-ambiguous
    → invalid
```

This prevents malformed or contradictory model output from entering the normal interaction pipeline.

---

## Interpretation Result

The distinction between interpretation and interpretation result is intentional.

### AtlasInteractionInterpretation

Represents what Atlas believes the user intends:

```text
Intent
Query
MemoryContent
```

### AtlasInteractionInterpretationResult

Represents both the interpretation and the certainty associated with it:

```text
Interpretation
Confidence
IsAmbiguous
```

Conceptually:

```text
AtlasInteractionInterpretationResult
├── Interpretation
│   ├── Intent
│   ├── Query
│   └── MemoryContent
├── Confidence
└── IsAmbiguous
```

This prevents confidence information from being mixed into the actual semantic interpretation.

---

## Processor Behavior

`AtlasInteractionProcessor` depends on `IAtlasInteractionInterpreter`.

After interpretation, the processor checks ambiguity before selecting an interaction handler.

```text
Interpret interaction
       │
       ▼
IsAmbiguous?
    │       │
   Yes      No
    │        │
    ▼        ▼
Clarify   Select handler
              │
              ▼
          HandleAsync
```

An ambiguous interpretation therefore cannot execute an interaction handler.

The current clarification response is intentionally simple:

```text
I'm not quite sure what you mean. Could you clarify?
```

This is a safety boundary: uncertain interpretation should not accidentally cause a memory write or another action.

---

## Handler Boundary

Interaction handlers continue to receive:

```text
AtlasInteraction
AtlasInteractionInterpretation
```

They do not currently receive the complete `AtlasInteractionInterpretationResult`.

This keeps confidence and ambiguity concerns at the processor level.

The architecture therefore remains:

```text
InterpretationResult
        │
        ▼
InteractionProcessor
        │
        ├── ambiguous
        │      └── clarification
        │
        └── not ambiguous
               │
               ▼
        AtlasInteractionInterpretation
               │
               ▼
        InteractionHandler
```

---

## Interpreter Selection

Atlas supports multiple interpreter implementations through:

```text
AtlasInteractionInterpreterMode
```

The available modes are:

```text
Deterministic
LanguageModel
```

The default is:

```text
Deterministic
```

Selection is performed by the composition root in `Atlas.Hosting`.

Configuration is:

```json
{
  "Atlas": {
    "Name": "Atlas",
    "Interaction": {
      "InterpreterMode": "Deterministic"
    }
  }
}
```

To select the language-model implementation:

```json
{
  "Atlas": {
    "Name": "Atlas",
    "Interaction": {
      "InterpreterMode": "LanguageModel"
    }
  }
}
```

The configuration value is read as a string by `Atlas.Hosting` and explicitly converted to `AtlasInteractionInterpreterMode`.

Unsupported values fail during service registration rather than silently selecting an unintended implementation.

---

## Dependency Direction

The dependency direction is intentional:

```text
Atlas.Abstractions
        ↑
Atlas.Commands
        ↑
Atlas.Memory
        ↑
Atlas.Interaction
        ↑
Atlas.Hosting
```

`Atlas.AI` remains a separate capability and does not depend on `Atlas.Interaction`.

The language-model interaction interpreter creates the integration point:

```text
Atlas.Interaction
        ↓
Atlas.AI
```

The reverse dependency is prohibited:

```text
Atlas.AI
    X
    ↓
Atlas.Interaction
```

This prevents AI infrastructure from becoming coupled to Atlas-specific application semantics.

---

## Why This Boundary Exists

The separation allows Atlas to change its language-model implementation without changing interaction semantics.

For example, `Atlas.AI` can eventually support:

```text
OpenAI
Azure OpenAI
Ollama
Local models
Other OpenAI-compatible providers
```

without requiring those providers to understand:

```text
AtlasInteractionIntent
AtlasMemory
AtlasInteractionHandler
```

Likewise, Atlas can improve interaction semantics without requiring a language-model provider to change.

The boundary is therefore:

```text
                    Generic AI
                       │
                       │ structured output
                       ▼
              Atlas Interaction
                       │
                 Atlas semantics
                       │
                       ▼
                Atlas capabilities
```

---

## Current State

The interaction interpretation foundation currently provides:

* Deterministic interaction interpretation.
* Language-model-backed interaction interpretation.
* Structured language-model requests and responses.
* Structured interpretation parsing.
* Semantic validation.
* Confidence levels.
* Ambiguity detection.
* Safe clarification behavior.
* Configurable interpreter selection.
* Fully tested deterministic and language-model interpretation paths.

The current implementation deliberately keeps the AI layer provider-neutral.

A production language-model provider has not yet been selected or implemented. The deterministic interpreter therefore remains the default.

---

## Future Extensions

Potential future improvements include:

```text
More precise ambiguity handling
Clarification questions generated from context
Confidence-aware handler policies
Conversation context
Provider-specific language-model implementations
Structured tool/function calling
Streaming language-model responses
```

These should build on the existing boundaries rather than bypassing them.

The key architectural principle remains:

> `Atlas.AI` provides language-model capabilities; `Atlas.Interaction` determines what the language-model output means to Atlas.
