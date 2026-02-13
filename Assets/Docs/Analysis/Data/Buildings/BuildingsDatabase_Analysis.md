# Analysis — BuildingsDatabase.cs
Generated: 2026-02-14

## Purpose
`BuildingsDatabase` is the central catalog asset for buildable structures. It supports both:
- Legacy build-menu entries (`items: List<BuildItemDefinition>`)
- Detailed building records (`buildings: List<BuildingDefinition>`)

## Fields
- `categoryOrder: List<string>`
  - Preferred tab order used by build-menu grouping.
- `items: List<BuildItemDefinition>`
  - Legacy/explicit build item assets.
- `buildings: List<BuildingDefinition>`
  - New detailed format with expanded gameplay metadata.

## Runtime Consumers
- `BuildMenuUI`
  - Prefers `buildings` entries and converts them to runtime build items.
  - Falls back to `items` when no valid detailed records are present.
- `TeamAIBuild`
  - Uses buildable catalog content for AI placement selection.

## Data Expectations
For detailed `buildings` entries, minimally provide:
- `prefab`
- `displayName` or `id`
- `category` or `subCategory`
- Optional `constructionCost` with resource IDs that map to runtime `ResourceType` values.
