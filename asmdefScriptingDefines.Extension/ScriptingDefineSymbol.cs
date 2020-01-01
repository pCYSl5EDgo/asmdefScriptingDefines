using System;

namespace ForCuteIzmChan
{
    [Serializable]
    public struct ScriptingDefineSymbol
    {
        public ScriptingDefineSymbolType Type;
        public string Symbol;

        public ScriptingDefineSymbol(ScriptingDefineSymbolType type, string symbol)
        {
            Type = type;
            Symbol = symbol;
        }

        public static bool TryParse(string line, out ScriptingDefineSymbol value)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                value = default;
                return false;
            }
            switch (line[0])
            {
                case '#':
                    value = new ScriptingDefineSymbol(ScriptingDefineSymbolType.Comment, line.Substring(1));
                    break;
                case '/':
                    value = new ScriptingDefineSymbol(ScriptingDefineSymbolType.PlaceHolder, line.Substring(1));
                    break; 
                case '-':
                    value = new ScriptingDefineSymbol(ScriptingDefineSymbolType.Remove, line.Substring(1));
                    break; 
                case '+':
                    value = new ScriptingDefineSymbol(ScriptingDefineSymbolType.Add, line.Substring(1));
                    break; 
                default:
                    value = new ScriptingDefineSymbol(ScriptingDefineSymbolType.Comment, line);
                    break;
            }
            return true;
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ScriptingDefineSymbolType.Comment:
                    return "#" + Symbol;
                case ScriptingDefineSymbolType.Add:
                    return "+" + Symbol;
                case ScriptingDefineSymbolType.Remove:
                    return "-" + Symbol;
                case ScriptingDefineSymbolType.PlaceHolder:
                    return "/" + Symbol;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}