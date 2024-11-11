using GDWeave;
using GDWeave.Godot;
using GDWeave.Godot.Variants;
using GDWeave.Modding;

namespace ChalkFix;

public class Mod : IMod
{
    public Config Config;

    public static IModInterface ModInterface;
    public Mod(IModInterface modInterface)
    {
        ModInterface = modInterface;
        modInterface.RegisterScriptMod(new ChalkFixScriptMod());
        modInterface.Logger.Information("chalk desync fixed! 'v'");
        this.Config = modInterface.ReadConfig<Config>();
    }

    public void Dispose()
    {
        // Cleanup anything you do here
    }
    public class ChalkFixScriptMod : IScriptMod
    {
        public bool ShouldRun(string path) => path == "res://Scenes/Entities/ChalkCanvas/chalk_canvas.gdc";

        // returns a list of tokens for the new script, with the input being the original script's tokens
        public IEnumerable<Token> Modify(string path, IEnumerable<Token> tokens)
        {
            // wait for any newline token after any extends token


            List<MultiTokenWaiter> addAHundredTo = [
                new MultiTokenWaiter([
                    t => t is IdentifierToken { Name : "p"},
                    t => t.Type is TokenType.Period,
                    t => t is IdentifierToken { Name : "x"},
                ], allowPartialMatch: false),
                new MultiTokenWaiter([
                    t => t is IdentifierToken { Name : "p2"},
                    t => t.Type is TokenType.Period,
                    t => t is IdentifierToken { Name : "x"},
                ], allowPartialMatch: false),
                new MultiTokenWaiter([
                    t => t is IdentifierToken { Name : "p"},
                    t => t.Type is TokenType.Period,
                    t => t is IdentifierToken { Name : "z"},
                ], allowPartialMatch: false),
                new MultiTokenWaiter([
                    t => t is IdentifierToken { Name : "p2"},
                    t => t.Type is TokenType.Period,
                    t => t is IdentifierToken { Name : "z"},
                ], allowPartialMatch: false),
             ];

            var cellAdjustFinder = new MultiTokenWaiter([
                    t => t.Type is TokenType.ParenthesisOpen,
                    t => t is IdentifierToken { Name : "pos"},
                    t => t.Type is TokenType.Period,
                    t => t is IdentifierToken { Name : "x"},
                    t => t.Type is TokenType.OpAdd,
                ], allowPartialMatch: false);

            var newlineConsumer = new TokenConsumer(t => t.Type is TokenType.Newline);

            // loop through all tokens in the script
            foreach (var token in tokens)
            {
                if (cellAdjustFinder.Check(token))
                {
                    yield return new Token(TokenType.Comma);
                    yield return new IdentifierToken("pos");
                    yield return new Token(TokenType.Period);
                    yield return new IdentifierToken("y");
                    yield return new Token(TokenType.ParenthesisClose);
                    newlineConsumer.SetReady();
                }
                if (newlineConsumer.Check(token)) continue;
                yield return token;

                if (newlineConsumer.Ready)
                {
                    newlineConsumer.Reset();
                }

                foreach(var waiter in addAHundredTo)
                {
                    if (waiter.Check(token))
                    {
                        yield return new Token(TokenType.OpAdd);
                        yield return new ConstantToken(new IntVariant(100));
                        continue;
                    }
                }

            }



        }
    }
}