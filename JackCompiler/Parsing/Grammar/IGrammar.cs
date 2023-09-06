using JackCompiler.Tokenizer;

namespace JackCompiler.Grammar;

public interface IGrammar
{
    abstract static bool Match(TokenReader tokenReader);

    abstract static IElement Compile(TokenReader tokenReader);
}
