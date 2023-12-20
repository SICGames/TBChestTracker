#pragma once
#include <Windows.h>
#include <algorithm>

#include <iostream>
#include <fstream>
#include <vector>
#include <iterator>

#pragma comment(lib,"User32.lib")

using namespace std;

class InputHookNative {
public:
	static InputHookNative& Instance() 
	{
		static InputHookNative inputHookSingleton;
		return inputHookSingleton;
	}
	//-- Input Keyboard Hook
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

	//--- Journal Record and Playback functions
	HHOOK GetJournalHook();
	DWORD getStartTime();
	bool Record();
	bool StopRocording();
	bool Playback();
	bool SaveEvent(const char*);
	bool LoadEvent(const char*);
	void SetStartTime();

	std::vector<EVENTMSG*> EventMessages;
	bool move_next;
	bool next_message_exists;
	UINT msg_count;

private:
	MSG msg;
	HHOOK hook;
	HHOOK journalHook;
	DWORD start_time;
	

protected:
};
LRESULT WINAPI hookCallback(int nCode, WPARAM wParam, LPARAM lParam);
LRESULT WINAPI JournalRecordProc(int nCode, WPARAM wParam, LPARAM lParam);
LRESULT WINAPI JournalPlaybackProc(int nCode, WPARAM wParam, LPARAM lParam);
