public enum ControlCopyFlags
{
	Appearance = 1,
	Sound = 2,
	Text = 4,
	Data = 8,
	State = 16,
	Transitions = 32,
	Invocation = 64,
	Settings = 128,
	DragDrop = 256,
	All = 65535
}

public interface IControl
{
	string Text { get; set; }

	bool IncludeTextInAutoCollider { get; set; }

	void UpdateCollider();

	void Copy(IControl c);

	void Copy(IControl c, ControlCopyFlags flags);

	int DrawPreStateSelectGUI(int selState, bool inspector);

	int DrawPostStateSelectGUI(int selState);

	void DrawPreTransitionUI(int selState, IGUIScriptSelector gui);

	string[] EnumStateElements();

	EZTransitionList GetTransitions(int index);

	EZTransitionList[] Transitions { get; set; }

	string GetStateLabel(int index);

	void SetStateLabel(int index, string label);

	ASCSEInfo GetStateElementInfo(int stateNum);

	object Data { get; set; }
}

public interface IKeyFocusable
{
	void LostFocus();

	string SetInputText(string inputText, ref int insertPt);

	string GetInputText(ref KEYBOARD_INFO info);

	void Commit();

	void SetCommitDelegate(EZKeyboardCommitDelegate del);

	string Content { get; }

	void GoUp();

	void GoDown();
}

public delegate void EZKeyboardCommitDelegate(IKeyFocusable control);
