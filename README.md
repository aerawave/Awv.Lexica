# What is Lexica?

Lexica serves two purposes.

The first purposes is that I could not find a good formatting solution for strings which allowed a little bit of basic code without serious security flaws (I did not want to randomly compile and run C# code for example). So I wrote my own.

The second purpose is to provide a basic parser in the form of the `Parser` class. There may be libraries out there which can accomplish what I wanted from this, but I had already wrote the code so I just put it in this repo and utilized it.

# How does it work?

Example:
```
Have a `ri(1,100)`(chance)% chance when struck in combat of increasing armor by `ri(1,999)`(armor) for `randomt(30s,1m30s)`. That chance is: `chance`%.
```

Using the above example with the `CompositionParser.Transpile()` class/method, this is converted into a `Composition` with the following `ILexigram` values:
- `Lexigram`: "Have a "
- `CodeLexigram`: `ri(1,100)`(chance)
- `Lexigram`: "% chance when struck in combat of increasing armor by "
- `CodeLexigram`: `ri(1,999)`(armor)
- `Lexigram`: " for "
- `CodeLexigram`: `randomt(30s,1m30s)`
- `Lexigram`: ". That chance is: "
- `CodeLexigram`: `chance`
- `Lexigram`: "%."

When you then call `Composition.build(CompositionEngine)` with that Composition, each token is processed through said engine. Default Lexigrams are just treated as raw strings and will be outputted accordingly. CodeLexigrams are treated as JavaScript however.

The CompositionEngine has an underlying `V8Engine` from [V8.NET](https://www.nuget.org/packages/V8.Net/). The entire CompositionEngine is passed to each ILexigram in the event that JavaScript execution is needed. The CodeLexigrams are executed as JavaScript.

Additionally, the CodeLexigrams can have a parantheses set at the end (for instance: `(chance)`). This will denote their "ID". Or what functionally is a variable name. If that ID is provided, then that name is used to set a global variable in the V8Engine which can be referenced from anywhere in the Composition.

Also, C# code can be loaded to run from the JavaScript provided. See below for more information on that.

## Time Marks

Something that I wrote that I think will be useful for my future projects utilizing this was converting time marks in the `CompositionEngine`. By default, this will be set to false.

What it essentially does is looks for a string which can be described with the following format:
```
[364d] [23h] [59m] [59s] [999ms]
```
It then converts it to a `TimeSpan`, then replaces it with a long which is the number of milliseconds (or whatever you want to replace it with).

It does not convert years or anything higher than days, so it's definitely not usable for all cases. I figured if anyone uses this, it may be useful for some. Or I could get advice on how to do this matter.

## Usage Example
``` c#
var example = "Have a `ri(1,100)`(chance)% chance when struck in combat of increasing armor by `ri(1,999)`(armor) for `randomt(30s,1m30s)`. That chance is: `chance`%.";
// Example of what this produces:
// Have a 43% chance when struck in combat of increasing armor by 679 for 1 min, 4 sec. The chance is: 43%.

var composition = new CompositionParser(example).Transpile();

Console.WriteLine($"Input string: {composition.ToString()}");
Console.WriteLine();

var engine = new CompositionEngine();
engine.ConvertTimemarks = true;

engine.RegisterLibrary<SystemLibrary>();

for (var i = 0; i < 5; i++)
{
    Console.WriteLine($"Example {i + 1}: {composition.Build(engine)}");
}
Console.ReadLine();
```

## ILibrary for C# code
In the above example, the code `engine.RegisterLibrary<SystemLibrary>();` was called.

In order to add C# code which can be called from the JavaScript, you must have a class which is not static and implements the ILibrary interface, and has static methods in it that return types that work with the internal `V8Engine` that `CompositionEngine` is using.

Additionally, multiple overloads cannot be used (at least at this time).

The `SystemLibrary` used in the example above was created for testing purposes. This is what the code for that class looked like:

``` c#
public class SystemLibrary : ILibrary
{
    public static Random Random { get; set; } = new Random();

    public static int ri(int min, int max) => randomi(min, max);

    public static int randomi(int min, int max)
    {
        return Random.Next(max - min) + min;
    }

    ///<summary>
    /// Should be used in other ILibrary classes to provide formatting in whatever form is preferred.
    ///</summary>
    internal static TimeSpan rt(int min, int max) => randomt(min, max);
    internal static TimeSpan randomt(int min, int max)
        => TimeSpan.FromMilliseconds(ri(min, max));

    public static double round(double value) => Math.Round(value);

    public static double ceil(double value) => Math.Ceiling(value);

    public static double floor(double value) => Math.Floor(value);
}
```