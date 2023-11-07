#pragma once
#include "CaptainHookNative.h"

using namespace System;


namespace CaptainHookSharp 
{
	public ref class InputHookEventArguments : EventArgs {
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

	private:
		
	protected:
	};
}
