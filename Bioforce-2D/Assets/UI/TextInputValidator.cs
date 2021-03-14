using System.Collections.Generic;

public class TextInputValidator
{
    private static List<string> InvalidStrings
    = new List<string>() {  "shit", "fuck", "bitch", "crap", "bloody", "sod", "arse", "minger",
                            "ahole", "bullshit", "piss", "son of a", "bollocks", "bellend", 
                            "tit", "fanny", "prick", "twat", "punani", "pussy", "cock", "cum", 
                            "knob", "dick", "bastard", "motherfuck", "wanker", "cunt", "nigg",
                            "suck", "succ", "zuck", "zucc", "idiot", "imbecile", "moron",
                            "retarded", "down", "boob", "vagin"};

    private static char[] ValidChars = new char[] { 
                                                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y',
                                                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y',
                                                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                                                ' ', '\''};

    public static void RemoveInvalidCharsInString(ref string text)
    {
        List<int> indexesRemove = new List<int>();

        for (int count = 0; count < text.Length; count++)
        {
            bool valid = false;
            foreach (char validChar in ValidChars)
            {
                if (text[count] == validChar)
                {
                    valid = true;
                    break;
                }
            }

            if (!valid)
                indexesRemove.Add(count);
        }

        int decrimentIndexer = 0;
        foreach (int index in indexesRemove)
        {
            text = text.Remove(index - decrimentIndexer);
            decrimentIndexer++;
        }

        foreach (string invalidString in InvalidStrings)
            text = text.ToLower().Replace(invalidString, "");

        text = text.Replace("  ", " ");
    }

    public static bool IsTextLengthValid(string text) =>
        text.Length >= 3;
}
