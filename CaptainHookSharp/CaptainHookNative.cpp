#include "CaptainHookNative.h"

bool previousKeyState = false;

InputHookNative::InputHookNative() {

}
InputHookNative::~InputHookNative() {

}

bool InputHookNative::Install() 
{
	if (!(hook = SetWindowsHookEx(WH_KEYBOARD_LL, hookCallback, NULL, 0)))
	{
		return false;
	}
	return true;
}
bool InputHookNative::Uninstall()
{
	return UnhookWindowsHookEx(hook);
}

bool InputHookNative::Record() 
{
	if (!(journalHook = SetWindowsHookEx(WH_JOURNALRECORD, JournalRecordProc, NULL, 0))) {
		return false;
	}
	start_time = GetTickCount64();

	return true;
}

bool InputHookNative::StopRocording() 
{
	return UnhookWindowsHookEx(journalHook);
}
bool InputHookNative::Playback() 
{
	if (!(journalHook = SetWindowsHookEx(WH_JOURNALPLAYBACK, JournalPlaybackProc, NULL, 0))) 
	{
		return false;
	}
	return true;
}
int InputHookNative::InputMessages() {
	if (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE)) 
	{
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return (int)msg.wParam; //return the messages
}

HHOOK InputHookNative::GetHook() {
	return hook;
}
HHOOK InputHookNative::GetJournalHook() {
	return journalHook;
}

DWORD InputHookNative::getStartTime() 
{
	return start_time;
}
void InputHookNative::SetStartTime() 
{
	start_time = GetTickCount64();
}

bool InputHookNative::LoadEvent(const char* filename) 
{
	
	EventMessages.clear();
	ifstream fin(filename, ios::binary);
	auto sz = fin.tellg();
	fin.seekg(0, ios::beg);
	EventMessages.resize(sz);
	fin.read(reinterpret_cast<char*>(EventMessages.data()), static_cast<long>(sz));
	fin.close();
	return true;
}
bool InputHookNative::SaveEvent(const char* filename) 
{
	ofstream fout(filename, ios_base::binary);
	fout.write((char*)EventMessages[0], EventMessages.size());
	fout.close();
	return true;
}
LRESULT WINAPI hookCallback(int nCode, WPARAM wParam, LPARAM lParam) {
	//MSLLHOOKSTRUCT* pMouseStruct = (MSLLHOOKSTRUCT*)lParam; // WH_MOUSE_LL struct
	KBDLLHOOKSTRUCT* pKeyStruct = (KBDLLHOOKSTRUCT*)lParam;
	
	if (nCode == HC_ACTION) 
	{
		if (wParam == WM_SYSKEYDOWN || wParam == WM_KEYDOWN)
		{
			if (pKeyStruct->vkCode)
			{
				if (!previousKeyState)
				{
					previousKeyState = true;
					int key = pKeyStruct->vkCode;
					InputHookNative::Instance().KeyCode = pKeyStruct->vkCode;
					InputHookNative::Instance().isKeyPressed = true;
				}
			}
		}
		else if (wParam == WM_SYSKEYUP || wParam == WM_KEYUP) 
		{
			if (previousKeyState) {
				previousKeyState = false;
				InputHookNative::Instance().KeyCode = pKeyStruct->vkCode;
				InputHookNative::Instance().isKeyPressed = false;
			}
		}
	}
	return CallNextHookEx(InputHookNative::Instance().GetHook(), nCode, wParam, lParam);
}

LRESULT WINAPI JournalRecordProc(int nCode, WPARAM wParam, LPARAM lParam) 
{
	if (nCode < 0)
		return CallNextHookEx(InputHookNative::Instance().GetJournalHook(), nCode, wParam, lParam);

	EVENTMSG* eventmsg = NULL;

	switch (nCode)
	{
		case HC_ACTION:
		{
			eventmsg = (EVENTMSG*)lParam;
			eventmsg->time = eventmsg->time - InputHookNative::Instance().getStartTime();
			InputHookNative::Instance().EventMessages.push_back(eventmsg);
			break;
		}
		default:
			return CallNextHookEx(InputHookNative::Instance().GetJournalHook(), nCode, wParam, lParam);
	}
	return 0;
}
LRESULT WINAPI JournalPlaybackProc(int nCode, WPARAM wParam, LPARAM lParam) 
{
	EVENTMSG* eventmsg = NULL;
	LRESULT delta = 0;
	
	static EVENTMSG* msg_from_file;

	InputHookNative pThis = InputHookNative::Instance();

	switch (nCode)
	{
	case HC_GETNEXT:
		eventmsg = (EVENTMSG*)lParam;

		if (pThis.move_next) {
			if (pThis.next_message_exists) {
				msg_from_file = pThis.EventMessages[pThis.msg_count];
				if (pThis.msg_count == pThis.EventMessages.size()) {
					pThis.next_message_exists = false;
				}
			}
			else {
				pThis.SetStartTime();
				pThis.msg_count = 0;
				pThis.move_next = true;
				pThis.next_message_exists = true;
				return 0;
			}
			pThis.move_next = false;
		}

		eventmsg->hwnd = msg_from_file->hwnd;
		eventmsg->message = msg_from_file->message;
		eventmsg->paramH = msg_from_file->paramH;
		eventmsg->paramL = msg_from_file->paramL;
		eventmsg->time = pThis.getStartTime() + msg_from_file->time;

		delta = eventmsg->time - GetTickCount64();

		if (delta < 0)
			return delta;
		else
			return 0;
		break;
	case HC_SKIP:
		pThis.move_next = true;
		pThis.msg_count++;
		break;
	default:
		return CallNextHookEx(pThis.GetJournalHook(), nCode, wParam, lParam);
	}

	return 0;
}