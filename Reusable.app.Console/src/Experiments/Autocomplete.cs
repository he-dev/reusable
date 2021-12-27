using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Text.RegularExpressions;
using Reusable.Essentials;

namespace Reusable;

internal static class Autocomplete
{
    public static IObserver<ConsoleKeyInfo> Create(IEnumerable<string> entries)
    {
        //const int maxMatchCount = 3;
        const int unselected = -1;
        var buffer = new StringBuilder();
        var position = default((int CursorTop, int CursorLeft));
        var lastCursorLeft = 0;
        var isTyping = false;
        var matches = new List<string>();
        var activeEntryIndex = -1;


        return Observer.Create<ConsoleKeyInfo>(onNext: console =>
        {
            isTyping = Console.CursorLeft - lastCursorLeft > 0;

            switch (console.Key)
            {
                case ConsoleKey.Enter:
                    buffer.Clear();
                    break;
                case ConsoleKey.Escape:
                    // todo hide autocomplete
                    break;
                case ConsoleKey.Spacebar when console.Modifiers.HasFlag(ConsoleModifiers.Control):
                    // todo show autocomplete
                    break;
                case ConsoleKey.Tab:
                    if (activeEntryIndex != unselected)
                    {
                        var completion = new string(matches.ElementAt(activeEntryIndex).Skip(buffer.Length).ToArray());
                        Console.Write(completion);
                        buffer.Clear();
                        activeEntryIndex = unselected;
                    }

                    break;
                case ConsoleKey.LeftArrow:
                    break;
                case ConsoleKey.DownArrow:
                case ConsoleKey.UpArrow:
                    activeEntryIndex =
                        activeEntryIndex
                            .MoveActiveIndex(console.Key)
                            .CycleActiveIndex(matches.Count);
                    break;
                case ConsoleKey.Backspace: // when Console.CursorLeft >= 0 && buffer.Any():
                    buffer.RemoveLast();
                    Console.Write(console.KeyChar);
                    //ClearLine((Console.CursorTop, Console.CursorLeft));
                    break;
                default:
                    // todo show autocomplete
                {
                    Console.Write(console.KeyChar);
                    position =
                        buffer.Length == 0
                            ? (Console.CursorTop, Console.CursorLeft)
                            : position;
                    buffer.Append(console.KeyChar);
                }
                    break;
            }

            var incomplete = FindIncomplete(buffer);

            Debug.WriteLine(incomplete);

            //matches = Match(entries, buffer.ToString(), maxMatchCount).ToList();
            //WriteMatches(position, matches, maxMatchCount, activeEntryIndex);
            //lastCursorLeft = Console.CursorLeft;
        });
    }

    private static string FindIncomplete(StringBuilder buffer)
    {
        var startIndex = Console.CursorLeft - 1;
        while (IsEntryChar(startIndex))
        {
            startIndex--;
        }

        // we're too far to the left; move back.
        startIndex++;

        return buffer.ToString().Substring(startIndex, Console.CursorLeft - startIndex);

        bool IsEntryChar(int i) => startIndex >= 0 && Regex.IsMatch(buffer[i].ToString(), "(?i)[a-z0-9_-]");
    }

    private static int MoveActiveIndex(this int activeEntryIndex, ConsoleKey consoleKey)
    {
        switch (consoleKey)
        {
            case ConsoleKey.UpArrow:
                return activeEntryIndex - 1;
            case ConsoleKey.DownArrow:
                return activeEntryIndex + 1;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static int CycleActiveIndex(this int activeEntryIndex, in int matchCount)
    {
        if (activeEntryIndex < 0) return matchCount - 1;
        if (activeEntryIndex > matchCount - 1) return 0;
        return activeEntryIndex;
    }

    private static IEnumerable<string> Match(in IEnumerable<string> entries, string current, in int maxMatchCount)
    {
        return
            entries
                .Where(entry => !string.IsNullOrEmpty(current) && entry.StartsWith(current, StringComparison.OrdinalIgnoreCase))
                .Take(maxMatchCount);
    }

    private static void WriteMatches(in (int CursorTop, int CursorLeft) position, in IEnumerable<string> entries, in int maxMatchCount, in int activeEntryIndex)
    {
        var autocompletePosition = (CursorTop: position.CursorTop + 1, CursorLeft: position.CursorLeft - 1);

        using (RestorePosition())
        {
            foreach (var (entry, index) in entries.Concat(Enumerable.Repeat(string.Empty, maxMatchCount)).Select((entry, index) => (entry, index)))
            {
                SetCursorPosition(autocompletePosition);
                ClearLine(autocompletePosition);
                using (RestoreStyle())
                {
                    if (index == activeEntryIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }

                    Console.WriteLine(entry);
                }

                autocompletePosition.CursorTop++;
            }
        }
    }

    private static void ClearLine(in (int CursorTop, int CursorLeft) position)
    {
        if (position.CursorLeft >= 0)
        {
            using (RestorePosition())
            {
                SetCursorPosition(position);
                Console.Write(new string(' ', Console.WindowWidth));
            }
        }
    }

    private static IDisposable RestorePosition()
    {
        var lastPosition = (Console.CursorTop, Console.CursorLeft);
        return Disposable.Create(() => SetCursorPosition(lastPosition));
    }

    private static IDisposable RestoreStyle()
    {
        var lastStyle = (Console.BackgroundColor, Console.ForegroundColor);
        return Disposable.Create(() => (Console.BackgroundColor, Console.ForegroundColor) = lastStyle);
    }

    private static void SetCursorPosition(in (int ConsoleTop, int ConsoleLeft) position) => (Console.CursorTop, Console.CursorLeft) = position;
}