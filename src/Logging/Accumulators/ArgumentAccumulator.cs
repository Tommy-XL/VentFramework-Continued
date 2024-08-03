using System;
using System.Collections.Generic;
using System.Text;

namespace VentLib.Logging.Accumulators;

internal class ArgumentAccumulator : ILogAccumulator
{
    public string AccumulatorID => nameof(ArgumentAccumulator);

    public LogComposite Accumulate(LogComposite composite, LogArguments arguments)
    {
        string initialString = composite.Message;

        StringBuilder builder = new();
        builder.EnsureCapacity(initialString.Length + arguments.Arguments.Length * 8);
        
        Stack<string> stack = new();
        int nArgs = 0;
        int pos = 0;

        while (true)
        {
            if (pos >= initialString.Length)
            {
                if (stack.Count == 0) break;
                ThrowFormatException(stack.Pop(), pos - 1, initialString);
            }
            
            char c = initialString[pos++];

            switch (c)
            {
                case '}':
                    {
                        if (stack.Count == 0) FormatException(c, "Unmatched closing brace.");
                        string stackValue = stack.Pop();

                        if (stackValue == "\\")
                        {
                            // Append the backslash followed by the closing brace
                            builder.Append('\\').Append(c);
                            continue;
                        }

                        if (!int.TryParse(stackValue, out int argNum))
                        {
                            FormatException(c, $"Invalid placeholder index '{stackValue}'.");
                            break;
                        }

                        if (stack.Count == 0 || stack.Pop() != "{") FormatException(c, "Expected opening brace before closing brace.");

                        if (argNum >= arguments.Arguments.Length)
                        {
                            throw new FormatException($"Not enough arguments provided for index {argNum}. (Total={arguments.Arguments.Length})");
                        }

                        builder.Append(arguments.Arguments[argNum]);
                        continue;
                    }
                case '{':
                    if (stack.Count == 0 || stack.Peek() != "\\")
                    {
                        stack.Push("{");
                        continue;
                    }
                    stack.Pop(); // Handle escaped '{'
                    builder.Append('\\').Append(c);
                    continue;
                case '\\':
                    if (stack.Count > 0 && stack.Peek() == "\\")
                    {
                        // Double backslash -> Append one backslash
                        stack.Pop();
                        builder.Append('\\');
                        continue;
                    }
                    stack.Push("\\");
                    continue;
                default:
                    if (stack.Count > 0)
                    {
                        string peek = stack.Peek();
                        if (peek == "{" && char.IsDigit(c))
                        {
                            stack.Push(stack.Pop() + c); // Building placeholder index
                        }
                        else if (peek == "{" || IsNumeric(peek))
                        {
                            FormatException(c, $"Invalid character '{c}' inside argument placeholder.");
                        }
                        else if (peek == "\\")
                        {
                            stack.Pop(); // Append the backslash followed by the character
                            builder.Append('\\').Append(c);
                        }
                        else
                        {
                            builder.Append(c); // Add literal '{' or other characters
                        }
                    }
                    else
                    {
                        builder.Append(c); // Normal character
                    }
                    break;
            }
        }

        return composite.SetMessage(builder.ToString());

        void FormatException(char currentChar, string msg = "Invalid format string.")
        {
            ThrowFormatException($"{msg} ('{currentChar}')", pos, initialString);
        }
    }

    private static bool IsNumeric(string s)
    {
        foreach (char c in s)
        {
            if (!char.IsDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    private static void ThrowFormatException(string str, int position, string sourceString)
    {
        throw new FormatException($"{str} at Position {position} of String \"{sourceString}\"");
    }
}