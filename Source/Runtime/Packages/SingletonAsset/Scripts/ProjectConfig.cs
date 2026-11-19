using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ProjectConfigSection : Attribute
{
}

public class ProjectConfigMenu
{
}

public class ProjectConfig<T> : SingletonAsset<T> where T : ScriptableObject
{
}
