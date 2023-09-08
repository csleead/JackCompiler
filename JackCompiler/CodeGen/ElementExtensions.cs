using System.Diagnostics.CodeAnalysis;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public static class ElementExtensions
{
    public static bool IsTerminalOfToken<T>(this IElement element, [NotNullWhen(true)] out T? token)
        where T : IToken
    {
        if (element is TerminalElement te && te.Token is T tt)
        {
            token = tt;
            return true;
        }

        token = default;
        return false;
    }

    public static bool IsTerminalOfToken<T>(this IElement element, Func<T, bool> predicate)
      where T : IToken
    {
        return element is TerminalElement te && te.Token is T tt && predicate(tt);
    }

    public static T AsTerminalOfToken<T>(this IElement element)
        where T : IToken
    {
        if (element is TerminalElement te && te.Token is T tt)
        {
            return tt;
        }

        throw new Exception($"Expect a terminal element of token type {typeof(T)}, but got {element}");
    }
}
