using Reusable.Collections;
using Reusable.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Reusable.Exceptionize;
using System.Linq.Custom;

namespace Reusable.Console.XTest
{
    public interface IAppConfigMerge
    {
        void Merge(XDocument source, XDocument target);
    }

    public class XMerge
    {
        // Specifies which paths require merging by key.
        // <Path, KeyGetter>
        private readonly IReadOnlyDictionary<IEnumerable<string>, Func<XElement, string>> _keyMap;

        public XMerge(IEnumerable<KeyValuePair<string, string>> keyMap)
        {
            _keyMap = keyMap.ToDictionary(
                x => x.Key.Split('/'),
                x => new Func<XElement, string>(e => e.Attribute(x.Value).Value),
                RelayEqualityComparer<IEnumerable<string>>.Create(
                    (left, right) => left.SequenceEqual(right),
                    (obj) => obj.CalcHashCode()
                )
            );
        }

        // Copies elements from source to target one by one. Attributes are copied all at once.
        public void Merge(XDocument sourceDocument, XDocument targetDocument)
        {
            var mode = GetRootMode(sourceDocument);
            var source = sourceDocument.Root;
            var target = targetDocument.Root;

            // Using a stack because not a fan of recursion.
            var stack = new Stack<(XElement Source, XElement Target, IEnumerable<string> Path, Mode Mode)>
        {
            (source, target, Enumerable.Empty<string>(), mode)
        };

            while (stack.Any())
            {
                var current = stack.Pop();
                var directives = new HashSet<Directive>();

                foreach (var node in current.Source.Nodes())
                {
                    switch (node)
                    {
                        case XComment comment:
                            if (Directive.TryParse(comment, out var directive))
                            {
                                directives.AddOrUpdate(directive);
                            }
                            break;

                        case XElement element:

                            // Update mode if new one is available.
                            mode = directives.ModeOrDefault() ?? current.Mode;

                            // Include mode requires elements to be explicitly marked as acm-incl in order to be copied.
                            var canCopyIncluded =
                                mode.Value == MergeMode.Include &&
                                directives.Copy().Mode == MergeMode.Include &&
                                directives.Copy().Scope.In(MergeScope.Single, MergeScope.Begin);

                            // Exclude mode requires elements to be explicitly marked as excluded in order to NOT be copied.
                            var canCopyNotExcluded =
                                mode.Value == MergeMode.Exclude &&
                                directives.Copy().Mode != MergeMode.Exclude &&
                                directives.Copy().Scope.In(MergeScope.Single, MergeScope.Begin);

                            // Indicates stop copying at the end of a scope.
                            var isEnd = directives.Copy().Scope.In(MergeScope.End);

                            if ((canCopyIncluded || canCopyNotExcluded) && !isEnd)
                            {
                                // If element is key-copyable then evaluate its existence.
                                var copyByKey = _keyMap.TryGetValue(current.Path, out var getKey);
                                var elementExists = copyByKey && current.Target.Elements().Any(x => getKey(x) == getKey(element));
                                if (copyByKey && !elementExists)
                                {
                                    ConsumeLabels(current.Target, directives);
                                    current.Target.Add(element);
                                }
                                else
                                {
                                    target = current.Target.Element(element.Name.LocalName);
                                    if (target is null)
                                    {
                                        ConsumeLabels(current.Target, directives);
                                        // Without default namespace it'll fail for some xmlns elements.
                                        var defaultNamespace = element.GetDefaultNamespace();

                                        // Child elements should be evaluated separately so copy only the current element.
                                        target = new XElement(defaultNamespace + element.Name.LocalName, element.Attributes());
                                        current.Target.Add(target);
                                    }
                                    // Append from .net 4.7.1 conflicts with my own extension so a full call is required.
                                    stack.Push((element, target, current.Path.Append(element.Name.LocalName), mode));
                                }
                            }

                            // In order to stop copying after a single-copy this directive needs to be removed.
                            if (directives.Copy().Scope.In(MergeScope.Single))
                            {
                                directives.Remove(directives.Copy());
                            }

                            break;
                    }
                }
            }
        }

        private static Mode GetRootMode(XDocument xDoc)
        {
            foreach (var node in xDoc.Nodes())
            {
                // It's not possible to use a helper variable here 
                // because it won't compile due to an unitialized 'directive' error.
                if (node is XComment comment && Directive.TryParse(comment, out var directive))
                {
                    // Mode must be the first directive.
                    return
                        directive is Mode mode
                            ? mode
                            : throw (
                                $"InvalidDirective{nameof(Exception)}",
                                $"The first directive must be {nameof(Mode)} (acm-mode: <incl|excl>)."
                            ).ToDynamicException();
                }
            }

            // Mode directive must be specified explicitly. There is no default value.
            throw (
                $"ModeDirectiveNotFound{nameof(Exception)}",
                $"You must specify the {nameof(Mode)} (acm-mode: <incl|excl>)."
            ).ToDynamicException();
        }

        private static void ConsumeLabels(XElement target, HashSet<Directive> directives)
        {
            var labels = directives.OfType<Label>();
            foreach (var label in labels)
            {
                target.Add(new XComment(label.Text.EncloseWith("  ")));
            }
            directives.RemoveWhere(x => labels.Contains(x));
        }
    }

    public abstract class Directive : IEquatable<Directive>
    {
        private delegate bool TryParseFunc(XComment comment, out Directive directive);

        private static readonly IEnumerable<TryParseFunc> _factories = new TryParseFunc[]
        {
            Mode.TryParse,
            Label.TryParse,
            Copy.TryParse
        };

        // By default there can be only one directive of each type name.
        [AutoEqualityProperty]
        public virtual string Name => GetType().Name;

        public static bool TryParse(XComment comment, out Directive directive)
        {
            foreach (var factory in _factories)
            {
                if (factory(comment, out directive))
                {
                    return true;
                }
            }
            directive = default;
            return false;
        }

        public bool Equals(Directive other) => AutoEquality<Directive>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is Directive d && Equals(d);

        public override int GetHashCode() => AutoEquality<Directive>.Comparer.GetHashCode(this);
    }

    // <!-- acm-mode: <incl|excl> -->
    public class Mode : Directive
    {
        public static readonly Mode None = new Mode { Value = MergeMode.None };

        public MergeMode Value { get; private set; }

        public static new bool TryParse(XComment comment, out Directive mode)
        {
            var match = Regex.Match(comment.Value, "acm-mode:(?<args>.+)");
            if (match.Success)
            {
                Enum.TryParse<MergeMode>(match.Groups["args"].Value.Trim(), true, out var value);
                mode = new Mode
                {
                    Value = value
                };
                return true;
            }
            else
            {
                mode = default;
                return false;
            }
        }
    }

    // <!-- acm-label: <"text"> -->
    public class Label : Directive
    {
        public string Text { get; private set; }

        // Labels are allowed to be defined multiple times.
        public override string Name => Text;

        public static new bool TryParse(XComment comment, out Directive label)
        {
            var match = Regex.Match(comment.Value, "acm-label:(?<args>.+)");
            if (match.Success)
            {
                label = new Label
                {
                    Text = match.Groups["args"].Value.Trim()
                };
                return true;
            }
            else
            {
                label = default;
                return false;
            }
        }
    }

    // <!-- acm-<include|exclude>[: begin|end] -->
    public class Copy : Directive
    {
        public static readonly Copy None = new Copy { Mode = MergeMode.None, Scope = MergeScope.None };

        public MergeMode Mode { get; internal set; }

        public MergeScope Scope { get; internal set; }

        public static new bool TryParse(XComment comment, out Directive copy)
        {
            var directiveMatch = Regex.Match(comment.Value, "acm-(?<mode>incl|excl(ude)?)(:(?<args>.+))?");
            if (directiveMatch.Success)
            {
                var mode = directiveMatch.Groups["mode"].Value;
                var args = directiveMatch.Groups["args"].Value.Split(',').Select(x => x.Trim());

                var ignoreCase = true;

                copy = new Copy
                {
                    Mode = (MergeMode)Enum.Parse(typeof(MergeMode), mode, ignoreCase),
                    Scope = Enum.TryParse<MergeScope>(args.FirstOrDefault(), ignoreCase, out var scope) ? scope : MergeScope.Single
                };
                return true;
            }
            else
            {
                copy = default;
                return false;
            }
        }
    }


    public enum MergeMode
    {
        None,
        Include,
        Incl = Include,
        Exclude,
        Excl = Exclude
    }

    public enum MergeScope
    {
        None,
        Single,
        Begin,
        End,
    }

    public static class HelperExtensions
    {
        public static HashSet<T> AddOrUpdate<T>(this HashSet<T> set, T item)
        {
            set.Remove(item);
            set.Add(item);
            return set;
        }

        public static Mode ModeOrDefault(this HashSet<Directive> directives)
        {
            return directives.OfType<Mode>().SingleOrDefault();
        }

        public static Copy Copy(this HashSet<Directive> directives)
        {
            return directives.OfType<Copy>().SingleOrDefault() ?? new Copy { Mode = MergeMode.None, Scope = MergeScope.Single }; ;
        }

        public static bool In<T>(this T value, params T[] others)
        {
            return others.Contains(value);
        }
    }
}
