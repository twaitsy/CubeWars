# Analysis — Civilian.cs
Generated: 2026-02-13 21:13:13

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
- AssignCraftingBuilding
- AssignGatherJob
- AssignPreferredNode
- CanTakeCraftingAssignment
- ClearCraftingAssignment
- GetCraftingSpeedMultiplierFromTools
- GetTransform
- IssueMoveCommand
- RefreshJobManagerRegistration
- SetJobType
- SetRole
- SetTeamID
- TakeDamage

## Properties
- CurrentAssignedSite
- CurrentDeliverySite
- CurrentReservedNode
- CurrentTargetName

## Events
- None

## Fields
- agent
- assignedHouse
- carriedAmount
- carriedType
- currentHealth
- currentHunger
- CurrentNode
- currentTiredness
- foodDatabase
- forcedNode
- gatherTimer
- HasJob
- hasStoredRoleState
- idleWanderTarget
- idleWanderTimer
- manualCraftingAssignment
- needActionTimer
- pendingFoodAmount
- registeredWithJobManager
- retargetTimer
- searchTimer
- stalledAtWorkPointTimer
- started
- state
- stateBeforeNeed
- targetCraftingBuilding
- targetFoodStorage
- targetFoodType
- targetHouse
- targetNode
- targetSite
- targetStorage
- targetWorkPoint
- teamID
- toolPickupTimer

## Serialized Fields
- None

## Attributes
- 0
- Header
- i
- Min
- Range
- RequireComponent
- s
- Tooltip

## Dependencies

### Using Namespaces
- System.Collections.Generic
- UnityEngine
- UnityEngine.AI

### Type References
- >
- after
- already
- bool
- BuildingStopDistanceType
- CharacterStats
- CivilianJobType
- CivilianRole
- compat
- ConstructionSite
- CraftingBuilding
- else
- float
- FoodResourceDatabase
- HashSet<CivilianToolType>
- House
- int
- NavMeshAgent
- new
- ResourceNode
- ResourceStorageContainer
- ResourceType
- return
- State
- storage
- string
- ToolItem
- Transform
- using
- var
- Vector2
- Vector3
- void

### GetComponent<T>()
- CharacterStats
- NavMeshAgent

### RequireComponent
- NavMeshAgent

### Event Subscriptions
- carriedAmount
- gatherTimer
- needActionTimer
- retargetTimer
- searchTimer
- stalledAtWorkPointTimer
- toolPickupTimer

### Attribute Types
- 0
- Header
- i
- Min
- Range
- RequireComponent
- s
- Tooltip

