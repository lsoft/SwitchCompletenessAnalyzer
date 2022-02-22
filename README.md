# Dotnet analyzer for switch over enum completeness

This analyzer gives a warning `SWITCHCOMPLETENESS001` in the situations like:

```C#
public enum SubjectEnum { S0, S1, S2 };

var subject = SubjectEnum.S0;
switch (subject) //no S1 case
{
    case SubjectEnum.S0:
        break;
    case SubjectEnum.S2:
        break;
    default:
        break;
}
```

or

```C#
public enum SubjectEnum { S0, S1, S2 };

var subject = SubjectEnum.S0;
var a = subject switch  //no S1 case
{
    SubjectEnum.S0 => 0,
    SubjectEnum.S2 => 2
};
```

You can convert the warning to the error via `<WarningsAsErrors>SWITCHCOMPLETENESS001</WarningsAsErrors>`, or suppress it:

1. At project level add `<NoWarn>SWITCHCOMPLETENESS001</NoWarn>` to your `csproj` or `props` files.
2. Locally use `#pragma warning disable SWITCHCOMPLETENESS001` - `#pragma warning restore SWITCHCOMPLETENESS001` in your source code.
3. You can mute a specific enums globally, add the following to your `csproj` or `props` files:
```xml
  <PropertyGroup>
    <SwitchCompletenessMuteEnums>global::SwitchCompletenessAnalyzer.Console.Silent1Enum|global::SwitchCompletenessAnalyzer.Console.Silent2Enum|</SwitchCompletenessMuteEnums>
  </PropertyGroup>

  <!-- explicitly allow the analyzer to access that variable -->
  <ItemGroup>
    <CompilerVisibleProperty Include="SwitchCompletenessMuteEnums" />
  </ItemGroup>
```

(enums must be divided with `|` symbol)
