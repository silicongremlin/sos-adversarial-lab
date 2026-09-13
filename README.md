# SOS adversarial lab

A safe, reproducible .NET/Avalonia implementation of the SOS board game used to study rule handling, replay data, and adversarial state design.

## What this repository contains

- `SOSGame.Logic/` — game rules and player implementations.
- `SOSGame.GUI/` — Avalonia desktop interface.
- `SOSGame.Tests/` — xUnit coverage for rules and UI support code.
- `ReplayData/` — small, deterministic CSV fixtures for replay and requirement checks.

The vulnerable demonstration state is preserved as a deterministic simulation. When `gridSize == 13`, it records the original staged chain (resource load, key and payload decryption, temporary HTA write, `mshta.exe` execution, and cleanup) through an observable mock trace. These primitives are inert: no keys or payloads are present, no files are written, and no process is launched.

## Run and test

Requires the .NET SDK compatible with the projects in `SOSGAME.sln`.

```bash
dotnet restore
dotnet test SOSGAME.sln
 dotnet run --project SOSGame.GUI/SOSGame.GUI.csproj
```

The game supports Simple and General modes plus a safe placeholder state used for boundary testing. Replay fixtures are plain CSV and contain no personal or operational data.

## Research framing

- **Question:** How do rule variants and constrained state transitions affect observable game outcomes?
- **Method:** Implement explicit game state, deterministic replay fixtures, and unit tests for transitions and scoring.
- **Implementation:** C# domain model with an Avalonia presentation layer.
- **Results:** Test and replay fixtures provide a repeatable baseline for future experiments.
- **Limitations:** This is a small tabletop-style simulation; it is not a security product and the placeholder state is intentionally non-executing.

## Provenance and public boundary

This repository is a sanitized clean snapshot derived from the former `kimbrow-slice/SOSGame` project. Its prior Git history is intentionally not included. Sprint reports, generated build output, machine-specific files, encrypted material, key material, and personal artifacts were excluded from the public history.








## License

MIT. Fork it, modify it, and build your own version. This repository is provided as-is and carries no maintenance or support commitment.

