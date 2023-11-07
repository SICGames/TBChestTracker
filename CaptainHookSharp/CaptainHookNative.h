#pragma once
#include <Windows.h>
#include <iostream>
#pragma comment(lib,"User32.lib")

class InputHookNative {
public:
	static InputHookNative& Instance() 
	{
		static InputHookNative inputHookSingleton;
		return inputHookSingleton;
	}
	InputHookNative();
	~InputHookNative();
	bool Install();
	bool Uninstall();
	int InputMessages();
	HHOOK GetHook();
	UINT KeyCode;
	UINT KeyFlags;
	bool isKeyReleased;
	bool isExtendedKey;
	UINT scancode;
	bool isKeyPressed;
	UINT repeatCount;
private:
	MSG msg;
	HHOOK hook;
protected:
};
LRESULT WINAPI hookCallback(int nCode, WPARAM wParam, LPARAM lParam);