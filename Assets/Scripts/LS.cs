using System.Collections.Generic;
using System.Text;

//l system logic
public static class LS
{

    //generates the final sequence based on rules and iterations
    public static string Sequencifier(LSInfo info)
    {
        Dictionary<char, string> ruleBook = new Dictionary<char, string>();
        foreach (var rule in info.rules)
        {
            ruleBook[rule.character] = rule.result;
        }

        string currentString = info.axiom;

        var sb = new StringBuilder();

        for (int i = 0; i < info.iterations; i++)
        {
            sb.Clear();

            foreach (char c in currentString)
            {
                sb.Append(ruleBook.TryGetValue(c, out string result) ? result : c.ToString());
            }
            currentString = sb.ToString();
        }
        return currentString;
    }
}