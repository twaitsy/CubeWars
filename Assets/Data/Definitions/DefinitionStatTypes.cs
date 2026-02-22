using System;
using System.Collections.Generic;
using UnityEngine;

public enum UnitClassType { Worker, Soldier, Scout, Builder, Support, Siege, Civilian, Other }
public enum ArmorType { None, Light, Medium, Heavy, Structure, Mechanical }
public enum AttackType { None, Melee, Ranged, Magic, Siege }
public enum UnitSizeCategory { Small, Medium, Large }
public enum JobTrainingLevel { Novice, Apprentice, Adept, Expert, Master }
public enum IdleBehaviourType { Wander, Socialize, Stand, Workaholic }
public enum RarityType { Common, Uncommon, Rare, Epic, Legendary }

[Serializable] public class StringFloatStat { public string id; public float value = 1f; }
[Serializable] public class StringIntStat { public string id; public int value; }
[Serializable] public class StringListStat { public string id; public List<string> values = new(); }
[Serializable] public class LevelBonusDefinition { public int level; public float damageBonus; public float speedBonus; public float healthBonus; }
