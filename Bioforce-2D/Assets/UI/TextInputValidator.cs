using System.Collections.Generic;

public enum TextType
{
    Normal,
    File
}
public class TextInputValidator
{
    private static List<string> InvalidStrings = new List<string>() { 
        "shit", "fuck", "bitch", "crap", "bloody", "sod", "arse", "minger",
        "ahole", "bullshit", "piss", "son of a", "bollocks", "bellend",
        "tit", "fanny", "prick", "twat", "punani", "pussy", "cock", "cum",
        "knob", "dick", "bastard", "motherfuck", "wanker", "cunt", "nigg",
        "suck", "succ", "zuck", "zucc", "idiot", "imbecile", "moron",
        "retarded", "downy", "boob", "vagin", "cuck", "jizz", "dicck", "fucc", 
        "fuk", "fucck", 
        "\\n", "\\\\", "\\", "/" };

    private static char[] ValidCharsFilenameSafe = new char[] {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        ' ', '\'', '!', ';', '+', '=', '-', '[', ']', '(', ')', '_' };
    private static char[] ValidCharsNormalInput = new char[] {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        ' ', '\'', '!', ';', '+', '=', '-', '[', ']', '(', ')', '_', 
        ',', '.', '/', '\\', '$', '&', '\"', '?', ':', '*', '<', '>' };

    private static Dictionary<TextType, char[]> ValidCharDictionary = new Dictionary<TextType, char[]>() 
    {
        { TextType.Normal, ValidCharsNormalInput },
        { TextType.File, ValidCharsFilenameSafe } 
    };

    public static void SanitiseText(ref string text, TextType textType = TextType.Normal) =>
        RemoveInvalidCharsInString(ref text, ValidCharDictionary[textType]);
    private static void RemoveInvalidCharsInString(ref string text, char[] validChars)
    {
        List<int> indexesRemove = new List<int>();
        text = text.TrimStart();

        for (int count = 0; count < text.Length; count++)
        {
            bool valid = false;
            foreach (char validChar in validChars)
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
        {
            //text = text.ToLower().Replace(invalidString, "");
            int indexOfInvalidString = text.ToLower().IndexOf(invalidString);
            if (indexOfInvalidString == -1)
                continue;

            text = text.Remove(indexOfInvalidString, invalidString.Length);
        }

        text = text.Replace("  ", " ");
    }

    public static bool IsTextLengthValid(string text) =>
        text.Length >= 3;

    public static string TrimAllSpaces(string serverNameSelected) =>
        serverNameSelected.Trim();
}
