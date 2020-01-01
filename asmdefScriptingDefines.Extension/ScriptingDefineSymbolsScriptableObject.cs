using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ForCuteIzmChan
{
    public class ScriptingDefineSymbolsScriptableObject : ScriptableObject
    {
        public List<ScriptingDefineSymbol> Symbols = new List<ScriptingDefineSymbol>();

        private static readonly char[] separator = new[]
        {
            '\r',
            '\n',
        };
        
        public void Parse(string text)
        {
            var lines = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if(ScriptingDefineSymbol.TryParse(line, out var symbol))
                    Symbols.Add(symbol);
            }
        }
        
        public override string ToString()
        {
            var buffer = new StringBuilder();
            foreach (var symbol in Symbols)
            {
                buffer.AppendLine(symbol.ToString());
            }
            return buffer.ToString();
        }
    }
}