using System;
using System.Collections.Generic;
using System.Linq;

namespace compiles_lab_1
{
    public class AutomatonSearcher
    {
        private enum State { S0, S1, S2, S3 }

        public List<SearchResult> Find(string text)
        {
            var results = new List<SearchResult>();

            State state = State.S0;

            int startIndex = -1;

            bool wordIsValid = false; 
            bool insideWord = false;   

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                bool isWordChar =
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9') ||
                    c == '_';
 
                if (!isWordChar)
                {
                    state = State.S0;
                    startIndex = -1;
                    insideWord = false;
                    wordIsValid = false;
                    continue;
                }
 
                if (!insideWord)
                {
                    insideWord = true;
 
                    if (c >= 'a' && c <= 'z')
                    {
                        wordIsValid = true;
                        startIndex = i;
                        state = State.S1;
                    }
                    else
                    { 
                        wordIsValid = false;
                        startIndex = -1;
                        state = State.S0;
                    }

                    continue;
                }
 
                if (!wordIsValid)
                {
                    continue;
                }
 
                State prev = state;
                state = Next(state, c);
 
                if (state == State.S0)
                {
                    wordIsValid = false;
                    startIndex = -1;
                    continue;
                }
 
                bool nextIsBoundary = (i + 1 >= text.Length) ||
                    !IsWordChar(text[i + 1]);

                if (state == State.S3 && nextIsBoundary && startIndex != -1)
                {
                    int length = i - startIndex + 1;

                    results.Add(new SearchResult
                    {
                        Fragment = text.Substring(startIndex, length),
                        StartIndex = startIndex,
                        Length = length,
                        Line = GetLine(text, startIndex),
                        Column = GetColumn(text, startIndex)
                    });

                    state = State.S0;
                    startIndex = -1;
                    insideWord = false;
                    wordIsValid = false;
                }
            }

            return results;
        }

        private bool IsWordChar(char c)
        {
            return
                (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c >= '0' && c <= '9') ||
                c == '_';
        }

        private State Next(State s, char c)
        {
            bool letter = c >= 'a' && c <= 'z';
            bool underscore = c == '_';

            return s switch
            {
                State.S0 => letter ? State.S1 : State.S0,
                State.S1 => letter ? State.S1 : underscore ? State.S2 : State.S0,
                State.S2 => letter ? State.S3 : State.S0,
                State.S3 => letter ? State.S3 : underscore ? State.S2 : State.S0,
                _ => State.S0
            };
        }

        private int GetLine(string text, int index)
            => text.Take(index).Count(c => c == '\n') + 1;

        private int GetColumn(string text, int index)
        {
            int last = text.LastIndexOf('\n', index);
            return last == -1 ? index + 1 : index - last;
        }
    }
}
