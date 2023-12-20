#pragma once
#include "CaptainHookNative.h"
#include <vcclr.h>
#include <msclr/marshal.h>
using namespace System;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;


namespace CaptainHookSharp 
{
	public ref class InputHookEventArguments : EventArgs 
	{
	public:
		property int KeyCode;
		InputHookEventArguments(int key) {
			KeyCode = key;
		}
	};

	public ref class InputHooks
	{
	public:
		static event EventHandler<InputHookEventArguments^>^ onKeyReleased;
		static event EventHandler<InputHookEventArguments^>^ onKeyDown;
		static event EventHandler<InputHookEventArguments^>^ onKeyPressed;
		static InputHooks() 
		{

		}
		static bool Install() 
		{
			return InputHookNative::Instance().Install();
		}
		static bool Uninstall() {
			return InputHookNative::Instance().Uninstall();
		}
		static int InputMessages() 
		{
			int msg = InputHookNative::Instance().InputMessages();
			bool keyPressed = InputHookNative::Instance().isKeyPressed;
			bool keyReleased = InputHookNative::Instance().isKeyReleased;
			int repeatCount = InputHookNative::Instance().repeatCount;
	
			if (!keyPressed) 
			{
				int vKey = InputHookNative::Instance().KeyCode;
				onKeyReleased(NULL, gcnew InputHookEventArguments(vKey));
			}
			else if (keyPressed) 
			{
				int vKey = InputHookNative::Instance().KeyCode;
				onKeyPressed(NULL, gcnew InputHookEventArguments(vKey));
			}
			return msg;
		}
		static bool Record() 
		{
			return InputHookNative::Instance().Record();
		}
		static bool StopRecoding() {
			return InputHookNative::Instance().StopRocording();
		}
		static bool Play() {
			return InputHookNative::Instance().Playback();
		}
		static bool LoadEventFile(String ^filename) 
		{
			marshal_context^ c = gcnew marshal_context();
			const char* cFile = c->marshal_as<const char*>(filename);
			bool result = InputHookNative::Instance().LoadEvent(cFile);
			delete c;
			return result;

		}
		static bool SaveEventFile(String ^filename) {
		
			marshal_context^ c = gcnew marshal_context();
			const char* cFile = c->marshal_as<const char*>(filename);
			bool result = InputHookNative::Instance().SaveEvent(cFile);
			
			delete c;
			return result;
		}
	private:
		
	protected:
	};
}
