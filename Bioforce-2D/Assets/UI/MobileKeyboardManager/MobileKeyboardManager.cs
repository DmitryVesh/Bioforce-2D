using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MobileKeyboardManager
{
    //TODO: Test on android, on my iOS device when click on inputField, it auto opens a keyboard for me...
    //So might be useless.
    public static TouchScreenKeyboard Keyboard
    {
        get
        {
            if (!TouchScreenKeyboard.isSupported)
                return null;

            if (keyboard == null)
            {
                TouchScreenKeyboard.hideInput = true;
                keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, true);
                keyboard.active = false;
            }
            return keyboard;
        }
    }
    private static TouchScreenKeyboard keyboard = null;
    
}
