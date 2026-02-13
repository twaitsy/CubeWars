# Analysis — Civilian.cs
Generated: 2026-02-07 20:58:14

## Namespace


## Classes
- Civilian

## Base Classes
- IHasHealth
- ITargetable
- MonoBehaviour

## Interfaces
- expects

## Methods
- AssignGatherJob
- GetTransform
- RefreshJobManagerRegistration
- SetRole
- TakeDamage

## Properties
- CurrentAssignedSite
- CurrentDeliverySite
- CurrentReservedNode

## Events
- None

## Fields
- agent
- carriedAmount
- carriedType
- currentHealth
- CurrentNode
- gatherTimer
- HasJob
- registeredWithJobManager
- retargetTimer
- searchTimer
- started
- state
- targetNode
- targetSite
- targetStorage
- teamID

## Serialized Fields
- None

## Attributes
- 0
- Header
- i
- RequireComponent

## Dependencies

### Using Namespaces
- UnityEngine
- UnityEngine.AI

### Type References
- >
- after
- already
- bool
- CharacterStats
- CivilianRole
- compat
- ConstructionSite
- drop
- float
- int
- NavMeshAgent
- ResourceNode
- ResourceStorageContainer
- ResourceType
- return
- State
- storage
- tick
- Transform
- using
- var
- void

### GetComponent<T>()
- CharacterStats
- NavMeshAgent

### RequireComponent
- NavMeshAgent

### Event Subscriptions
- carriedAmount
- gatherTimer
- retargetTimer
- searchTimer

### Attribute Types
- 0
- Header
- i
- RequireComponent

## Latest Update Notes
- Added assigned-house food-first consumption attempt in `TickSeekFoodStorage()`.
- Added crafting return enforcement in `ResumeAfterNeed()` to force `GoingToWorkPoint` for active crafting assignments.
- Added work-point stall recovery in `TickGoWorkPoint()` with assignment clear after 10 seconds.
- Added `IsAtAssignedCraftingWorkPoint` debug/runtime property for crafting activity checks.

