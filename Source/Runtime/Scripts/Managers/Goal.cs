using System;
using Glu.Localization;

[Serializable]
public class CumulativeGoal
{
	public int Target;

	public int Progress;

	public int Reward;

	public bool IsCompleted
	{
		get
		{
			return Progress >= Target;
		}
	}

	public CumulativeGoal()
	{
	}

	public CumulativeGoal(int target, int reward)
	{
		Target = target;
		Progress = 0;
		Reward = reward;
	}
}

[Serializable]
public class Goal
{
	public enum Type
	{
		CollectItems,
		KillCommonInfected,
		KillSpecialInfected,
		CompleteKillAllMissions,
		RescueSurvivors,
		KillPiper,
		KillPiperMinion,
		KillThrower,
		KillBomber,
		KillCharger,
		KillSpitter,
		EscortSurvivors,
		Headshot,
		Dismember,
		ExplodeKill,
		HandgunKill,
		ShotgunKill,
		ASRKill,
		SniperKill,
		MeleeKill,
		KillUncommonInfected,
		PlayEndlessMode,
		UseConsumables,
		MeleeKillSpecial,
		UseHealing,
		CompleteKillAllIn4Min,
		CompleteKillAllIn3Min,
		HandgunShots,
		ShotgunShots,
		ASRShots,
		SniperShots,
		KillBirds
	}

	public int Ticket;

	public Type GoalType;

	public int Target;

	public int Progress;

	public int Reward;

	public bool IsCompleted
	{
		get
		{
			return Progress >= Target;
		}
	}

	public string Description
	{
		get
		{
			switch (GoalType)
			{
			case Type.CollectItems:
				return !(Target <= 1) ? string.Format(Strings.GetString("COLLECT {0} SUPPLIES"), Target) : Strings.GetString("COLLECT SUPPLY");
			case Type.KillCommonInfected:
				return !(Target <= 1) ? string.Format(Strings.GetString("KILL {0} COMMON ZOMBIES"), Target) : Strings.GetString("KILL COMMON ZOMBIE");
			case Type.KillSpecialInfected:
				return !(Target <= 1) ? string.Format(Strings.GetString("KILL {0} SPECIAL ZOMBIES"), Target) : Strings.GetString("KILL SPECIAL ZOMBIE");
			case Type.CompleteKillAllMissions:
				return !(Target <= 1) ? string.Format(Strings.GetString("FINISH {0} \"KILL ALL\""), Target) : Strings.GetString("FINISH \"KILL ALL\" ");
			case Type.RescueSurvivors:
				return !(Target <= 1) ? string.Format(Strings.GetString("RESCUE {0} SURVIVORS"), Target) : Strings.GetString("RESCUE SURVIVOR");
			case Type.KillPiper:
				return !(Target <= 1) ? string.Format(Strings.GetString("KILL {0} PIPERS"), Target) : Strings.GetString("KILL PIPER");
			case Type.KillPiperMinion:
				return !(Target <= 1) ? string.Format(Strings.GetString("KILL {0} PIPER MINIONS"), Target) : Strings.GetString("KILL PIPER MINION");
			case Type.KillThrower:
				return !(Target <= 1) ? string.Format(Strings.GetString("KILL {0} THROWERS"), Target) : Strings.GetString("KILL THROWER");
			case Type.KillBomber:
				return !(Target <= 1) ? string.Format(Strings.GetString("KILL {0} BOMBERS"), Target) : Strings.GetString("KILL BOMBER");
			case Type.KillCharger:
				return !(Target <= 1) ? string.Format(Strings.GetString("KILL {0} CHARGERS"), Target) : Strings.GetString("KILL CHARGER");
			case Type.KillSpitter:
				return !(Target <= 1) ? string.Format(Strings.GetString("KILL {0} SPITTERS"), Target) : Strings.GetString("KILL SPITTER");
			case Type.EscortSurvivors:
				return !(Target <= 1) ? string.Format(Strings.GetString("ESCORT {0} SURVIVORS"), Target) : Strings.GetString("ESCORT SURVIVOR");
			case Type.Headshot:
				return !(Target <= 1) ? string.Format(Strings.GetString("HEADSHOT {0} ZOMBIES"), Target) : Strings.GetString("HEADSHOT A ZOMBIE");
			case Type.Dismember:
				return !(Target <= 1) ? string.Format(Strings.GetString("DISMEMBER {0} ZOMBIES"), Target) : Strings.GetString("DISMEMBER A ZOMBIE");
			case Type.ExplodeKill:
				return !(Target <= 1) ? string.Format(Strings.GetString("EXPLODE {0} ZOMBIES"), Target) : Strings.GetString("EXPLODE A ZOMBIE");
			case Type.HandgunKill:
				return !(Target <= 1) ? string.Format(Strings.GetString("GET {0} HANDGUN KILLS"), Target) : Strings.GetString("GET HANDGUN KILL");
			case Type.ShotgunKill:
				return !(Target <= 1) ? string.Format(Strings.GetString("GET {0} SHOTGUN KILLS"), Target) : Strings.GetString("GET SHOTGUN KILL");
			case Type.ASRKill:
				return !(Target <= 1) ? string.Format(Strings.GetString("GET {0} ASR KILLS"), Target) : Strings.GetString("GET ASR KILL");
			case Type.SniperKill:
				return !(Target <= 1) ? string.Format(Strings.GetString("GET {0} SNIPER KILLS"), Target) : Strings.GetString("GET SNIPER KILL");
			case Type.MeleeKill:
				return !(Target <= 1) ? string.Format(Strings.GetString("GET {0} MELEE KILLS"), Target) : Strings.GetString("GET MELEE KILL");
			case Type.KillUncommonInfected:
				return !(Target <= 1) ? string.Format(Strings.GetString("KILL {0} UNCOMMON ZOMBIES"), Target) : Strings.GetString("KILL UNCOMMON ZOMBIE");
			case Type.PlayEndlessMode:
				return !(Target <= 1) ? string.Format(Strings.GetString("PLAY ENDLESS MODE {0} TIMES"), Target) : Strings.GetString("PLAY ENDLESS MODE");
			case Type.UseConsumables:
				return !(Target <= 1) ? string.Format(Strings.GetString("USE {0} CONSUMABLES"), Target) : Strings.GetString("USE CONSUMABLE");
			case Type.MeleeKillSpecial:
				return !(Target <= 1) ? string.Format(Strings.GetString("KILL {0} SPECIALS WITH MELEE"), Target) : Strings.GetString("KILL SPECIAL WITH MELEE");
			case Type.UseHealing:
				return !(Target <= 1) ? string.Format(Strings.GetString("USE HEALING {0} TIMES"), Target) : Strings.GetString("USE HEALING");
			case Type.CompleteKillAllIn4Min:
				return !(Target <= 1) ? string.Format(Strings.GetString("FINISH \"KILL ALL\" IN 4 MIN {0} TIMES"), Target) : Strings.GetString("FINISH \"KILL ALL\" IN 4 MINUTES");
			case Type.CompleteKillAllIn3Min:
				return !(Target <= 1) ? string.Format(Strings.GetString("FINISH \"KILL ALL\" IN 3 MIN {0} TIMES"), Target) : Strings.GetString("FINISH \"KILL ALL\" IN 3 MINUTES");
			case Type.HandgunShots:
				return !(Target <= 1) ? string.Format(Strings.GetString("MAKE {0} HANDGUN SHOTS"), Target) : Strings.GetString("MAKE HANDGUN SHOT");
			case Type.ShotgunShots:
				return !(Target <= 1) ? string.Format(Strings.GetString("MAKE {0} SHOTGUN SHOTS"), Target) : Strings.GetString("MAKE SHOTGUN SHOT");
			case Type.ASRShots:
				return !(Target <= 1) ? string.Format(Strings.GetString("MAKE {0} ASR SHOTS"), Target) : Strings.GetString("MAKE ASR SHOT");
			case Type.SniperShots:
				return !(Target <= 1) ? string.Format(Strings.GetString("MAKE {0} SNIPER SHOTS"), Target) : Strings.GetString("MAKE SNIPER SHOT");
			case Type.KillBirds:
				return !(Target <= 1) ? string.Format(Strings.GetString("KILL {0} BIRDS"), Target) : Strings.GetString("KILL A BIRD");
			default:
				return string.Empty;
			}
		}
	}

	public Goal()
	{
	}

	public Goal(int ticket, Type goalType, int target, int reward)
	{
		Ticket = ticket;
		GoalType = goalType;
		Target = target;
		Progress = 0;
		Reward = reward;
	}
}
