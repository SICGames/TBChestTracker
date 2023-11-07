#include "CaptainHookNative.h"

bool previousKeyState = false;

InputHookNative::InputHookNative() {

}
InputHookNative::~InputHookNative() {

}

bool InputHookNative::Install() {
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

int InputHookNative::InputMessages() {
	if (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE)) {
		TranslateMessage(&msg);
		DispatchMessage(&msg);
	}

	return (int)msg.wParam; //return the messages
}
HHOOK InputHookNative::GetHook() {
	return hook;
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