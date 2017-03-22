using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Shelly.Validators
{
    internal static class CommandValidator
    {
        //public static void ValidatePropertyNames(this Type commandType)
        //{
        //    try
        //    {
        //        var names = Command.GetParameters(commandType).SelectMany(x => x.Names);
        //        ValidatePropertyNames(names);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ArgumentException($"Command \"{commandType.FullName}\" has some invalid properties.", ex);
        //    }
        //}

        //private static void ValidatePropertyNames(IEnumerable<string> names)
        //{
        //    var duplicateNames = names
        //        .GroupBy(x => x)
        //        .Where(g => g.Count() > 1)
        //        .Select(g => g.Key)
        //        .ToList();

        //    if (duplicateNames.Any())
        //    {
        //        throw new ArgumentException($"Duplicate property names found: [{string.Join(", ", duplicateNames)}].");
        //    }
        //}

        //public static TCommand Validate<TCommand>(this TCommand command, StringComparer stringComparer) where TCommand : Command, new()
        //{
        //    var argumentPositionValidationExceptions = command.ValidateArgumentPositions();
        //    var argumentNameValidationExceptions = command.ValidateArgumentNames(stringComparer);

        //    var validationExceptions = new AggregateException($"Could not initialize the {command.GetType().Name}. See inner exeptions for details.", argumentPositionValidationExceptions.Concat(argumentNameValidationExceptions));

        //    if (validationExceptions.InnerExceptions.Any())
        //    {
        //        throw validationExceptions;
        //    }

        //    return command;
        //}

        //public static IEnumerable<Exception> ValidateArgumentPositions(this Command command)
        //{
        //    var arguments = command.Arguments.Order().Where(a => a.Properties.HasPosition).ToList();
        //    var position = 0;
        //    foreach (var argument in arguments)
        //    {
        //        if (argument.Properties.Position != position)
        //        {
        //            var ex = new Exception(
        //                $"{command.GetType().Name}'s argument {argument.Names.First()} has an invalid position. Expected {position} but found {argument.Properties.Position}. " +
        //                $"Argument positions are zero based, must be in ascending order and requiered arguments must come before optional ones.");
        //            yield return ex;
        //        }
        //        position++;
        //    }
        //}

        //public static IEnumerable<Exception> ValidateArgumentNames(this Command command, StringComparer stringComparer)
        //{
        //    var names = new HashSet<string>(stringComparer);
        //    var argumentNameExceptions =
        //        from argument in command.Arguments
        //        from name in argument.Names
        //        where !names.Add(name)
        //        select new Exception($"{command.GetType().Name}'s argument name <{name}> already exists.");
        //    return argumentNameExceptions;
        //}
    }
}
