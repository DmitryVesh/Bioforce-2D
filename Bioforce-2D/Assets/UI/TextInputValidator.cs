using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum TextType
{
    Normal,
    Filename
}
public class TextInputValidator
{
    //private static List<string> InvalidStrings = new List<string>() { 
    //    "shit", "fuck", "bitch", "crap", "bloody", "sod", "arse", "minger",
    //    "ahole", "bullshit", "piss", "son of a", "bollocks", "bellend",
    //    "tit", "fanny", "prick", "twat", "punani", "pussy", "cock", "cum",
    //    "knob", "dick", "bastard", "motherfuck", "wanker", "cunt", "nigg",
    //    "suck", "succ", "zuck", "zucc", "idiot", "imbecile", "moron",
    //    "retarded", "downy", "boob", "vagin", "cuck", "jizz", "dicck", "fucc", 
    //    "fuk", "fucck", 
    //    "\\n", "\\\\", "\\", "/" };
    private static Regex RegexInvalidStrings = new Regex(
        "s+h+i+t|f+(u+(c+k*|c*k+)|u*c+k*)|bi+t+c+|crap|blo+dy|sod|ar+se|minger|" +
        "ass+|piss+|so+n+ o+f+ a|bo+l+o+(c+k*|c*k+)|bellend|ti+t|fan+y|prick|tw+a+t|" +
        "punani|pus+y|co+(c+k*|c*k+)|cu+m|knob|di+(c+k*|c*k+)|bastard|" +
        "wa+n+k|(c+k*|c*k+)u+n+t|nigg+|(s|z|c)u+(c+k*|c*k+)|idiot|imbecile|moron|re+t+a+r+d|" +
        "downy|boo+b|va+g+i+n|j+i+z|" +
        "\\n|\\\\|\\|", 
        RegexOptions.IgnoreCase);

    private static Regex RegexValidCharsFilenameSafe = new Regex(
        @"[^0-9a-z\s\'!;+=\-\[\]()_]+", RegexOptions.IgnoreCase);
    //private static char[] ValidCharsFilenameSafe = new char[] {
    //    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
    //    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
    //    '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
    //    ' ', '\'', '!', ';', '+', '=', '-', '[', ']', '(', ')', '_' };
    private static Regex RegexValidCharsNormalInput = new Regex(@"[^0-9a-z\s'!;+=\-\[\]()_,.\\$&""?:*<>]+", RegexOptions.IgnoreCase);
    //private static char[] ValidCharsNormalInput = new char[] {
    //    'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
    //    'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
    //    '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
    //    ' ', '\'', '!', ';', '+', '=', '-', '[', ']', '(', ')', '_', 
    //    ',', '.', '/', '\\', '$', '&', '\"', '?', ':', '*', '<', '>' };

    //private static Dictionary<TextType, char[]> ValidCharDictionary = new Dictionary<TextType, char[]>() 
    //{
    //    { TextType.Normal, ValidCharsNormalInput },
    //    { TextType.Filename, ValidCharsFilenameSafe } 
    //};
    private static Dictionary<TextType, Regex> ValidCharRegexDictionary = new Dictionary<TextType, Regex>() 
    {
        { TextType.Normal, RegexValidCharsNormalInput },
        { TextType.Filename, RegexValidCharsFilenameSafe } 
    };

    public static void SanitiseText(ref string text, TextType textType = TextType.Normal) =>
        RemoveInvalidCharsInString(ref text, ValidCharRegexDictionary[textType]);
    private static void RemoveInvalidCharsInString(ref string text, Regex charRegex)
    {
        text = text.TrimStart();

        text = charRegex.Replace(text, "");

        text = RegexInvalidStrings.Replace(text, "");

        text = text.Replace("  ", " ");

        //Old manual way of doing it...
        //Still keeping because may need to do something like this 1 day

        //List<int> indexesRemove = new List<int>();
        //for (int count = 0; count < text.Length; count++)
        //{
        //    bool valid = false;
        //    foreach (char validChar in validChars)
        //    {
        //        if (text[count] == validChar)
        //        {
        //            valid = true;
        //            break;
        //        }
        //    }

        //    if (!valid)
        //        indexesRemove.Add(count);
        //}

        //int decrimentIndexer = 0;
        //foreach (int index in indexesRemove)
        //{
        //    text = text.Remove(index - decrimentIndexer);
        //    decrimentIndexer++;
        //}

        //foreach (string invalidString in InvalidStrings) 
        //{
        //    //text = text.ToLower().Replace(invalidString, "");
        //    int indexOfInvalidString = text.ToLower().IndexOf(invalidString);
        //    if (indexOfInvalidString == -1)
        //        continue;

        //    text = text.Remove(indexOfInvalidString, invalidString.Length);
        //}
    }

    public static bool IsTextLengthValid(string text) =>
        text.Length >= 1;

    public static string TrimAllSpaces(string serverNameSelected) =>
        serverNameSelected.Trim();
}
