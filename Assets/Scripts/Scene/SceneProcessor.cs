using System;
using System.Collections.Generic;
using UnityEngine;

public class SceneProcessor
{
	public const string trimPostfix = "_trimmed";

	private const string trimDirectory = "_Trimmed";

	private const float linkDistance = 1.5f;

	private static Predicate<GameObject> isScaledUp = (GameObject obj) => obj.transform.localScale.magnitude > 2f;

	private static Predicate<GameObject> notScaledUp = (GameObject obj) => !isScaledUp(obj);

	private static List<Dictionary<string, object>> trimWhat = new List<Dictionary<string, object>>
	{
		new Dictionary<string, object> { { "name", "^EyeCandy$" } },
		new Dictionary<string, object> { { "name", "^Airbox" } },
		new Dictionary<string, object> { { "name", "^FX$" } },
		new Dictionary<string, object> { { "name", "^FX_" } },
		new Dictionary<string, object> { { "name", "^b\\) \\{Type:GetItems\\}" } },
		new Dictionary<string, object> { { "name", "^d\\) \\{Type:KillAll\\}" } },
		new Dictionary<string, object> { { "name", "^f\\) \\{Type:Escort\\}" } },
		new Dictionary<string, object> { { "name", "^h\\) \\{Type:Rescue\\}" } },
		new Dictionary<string, object> { { "name", "^Lamp_Ceiling" } },
		new Dictionary<string, object> { { "name", "^Lamp_" } },
		new Dictionary<string, object>
		{
			{ "name", "^Access_Panel_" },
			{
				"link",
				string.Empty
			}
		},
		new Dictionary<string, object> { { "name", "^Server_Panel" } },
		new Dictionary<string, object> { { "name", "^Dead_Bodies$" } },
		new Dictionary<string, object> { { "name", "^BloodSphash" } },
		new Dictionary<string, object> { { "name", "^LightGlow" } },
		new Dictionary<string, object> { { "name", "^Decal_" } },
		new Dictionary<string, object> { { "name", "^Background_" } },
		new Dictionary<string, object> { { "name", "^Roof_Aggregate" } },
		new Dictionary<string, object> { { "name", "^Obj_Air_Conditioner" } },
		new Dictionary<string, object> { { "name", "^Obj_Metal_Shield" } },
		new Dictionary<string, object> { { "name", "^Obj_Plank" } },
		new Dictionary<string, object> { { "name", "^Roof_Airbox" } },
		new Dictionary<string, object>
		{
			{ "name", "^Obj_.*_Shield_" },
			{ "chance", 40f }
		},
		new Dictionary<string, object> { { "name", "^Raven" } },
		new Dictionary<string, object> { { "name", "^Body_Bag" } },
		new Dictionary<string, object> { { "name", "^Obj_Carton" } },
		new Dictionary<string, object> { { "name", "^zombie_.*_dead_" } },
		new Dictionary<string, object> { { "name", "^Clone_Complex_Pipe" } },
		new Dictionary<string, object> { { "name", "^Clone_Complex_Rail" } },
		new Dictionary<string, object> { { "name", "^Clone_Complex_Lock" } },
		new Dictionary<string, object> { { "name", "^Clone_Complex_Support" } },
		new Dictionary<string, object> { { "name", "^Clone_Tube_Down_..$" } },
		new Dictionary<string, object> { { "name", "^Clone_Tube_Top_..$" } },
		new Dictionary<string, object> { { "name", "^Clone_Complex_Caps_Top" } },
		new Dictionary<string, object>
		{
			{ "name", "^Computer_Block" },
			{ "chance", 50f }
		},
		new Dictionary<string, object>
		{
			{ "name", "^Computer_Keyboard$" },
			{
				"link",
				string.Empty
			}
		},
		new Dictionary<string, object>
		{
			{ "name", "^Shelves" },
			{ "chance", 40f }
		},
		new Dictionary<string, object>
		{
			{ "name", "^Monitor_Off" },
			{ "condition", notScaledUp },
			{ "chance", 70f }
		},
		new Dictionary<string, object>
		{
			{ "name", "^Monitor_Off" },
			{ "condition", isScaledUp }
		},
		new Dictionary<string, object>
		{
			{ "name", "^Monitor_Off_Float" },
			{ "condition", notScaledUp }
		},
		new Dictionary<string, object>
		{
			{ "name", "^Monitor_Locked_Float$" },
			{
				"link",
				string.Empty
			}
		},
		new Dictionary<string, object>
		{
			{ "name", "^Monitor_Research_._Float$" },
			{
				"link",
				string.Empty
			},
			{ "condition", notScaledUp }
		},
		new Dictionary<string, object>
		{
			{ "name", "^Monitor_Research_.$" },
			{
				"link",
				string.Empty
			},
			{ "chance", 30f }
		},
		new Dictionary<string, object>
		{
			{ "name", "^Server_" },
			{
				"link",
				string.Empty
			},
			{ "chance", 25f }
		}
	};

	private static List<string> linkerNames = new List<string> { "GetItem", "ActivateItem" };
}
