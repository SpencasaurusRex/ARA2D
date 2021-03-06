﻿using System;
using ARA2D.Core;
using ARA2D.Movement;
using ARA2D.Ticks;
using MoonSharp.Interpreter;
using Nez;

namespace ARA2D.Commands
{
    public class CommandScriptRunner : EntityProcessingSystem
    {
        readonly IComponentProvider componentProvider;

        readonly MoveRequester moveRequester;

        public CommandScriptRunner(IComponentProvider componentProvider, MoveRequester moveRequester) : base(
            new Matcher().all(typeof(CommandScript)))
        {
            this.componentProvider = componentProvider;
            this.moveRequester = moveRequester;
        }

        public override void process(Entity entity)
        {
            if (!componentProvider.GetComponent<TickInfo>().Ticking) return;

            var commandScript = entity.getComponent<CommandScript>();
            commandScript.ReceivedYield = false;

            while (!commandScript.ReceivedYield && commandScript.Running)
            {
                var nextCall = commandScript.CommandCalls[commandScript.CurrentLine];
                if (commandScript.Coroutine == null)
                {
                    CreateCoroutine(commandScript, nextCall);
                }

                RunCoroutine(commandScript);
                if (commandScript.Coroutine == null && ++commandScript.CurrentLine >= commandScript.CommandCalls.Count)
                {
                    commandScript.Status = ScriptStatus.Done;
                }
            }
        }

        public void CreateCoroutine(CommandScript script, CommandCall command)
        {
            Console.WriteLine($"Creating coroutine for {command.Name}");
            var repo = componentProvider.GetComponent<CommandRepository>();

            if (!repo.Commands.ContainsKey(command.Name))
            {
                script.Status = ScriptStatus.CommandNotFound;
                script.StatusDescription = $"Unable to find command \"{command.Name}\"";
                return;
            }

            DynValue args;
            if (string.IsNullOrEmpty(command.Arguments))
            {
                args = DynValue.Nil;
            }
            else
            {
                args = DynValue.NewString(command.Arguments);
            }

            script.Lua.Globals["args"] = args;
            script.Coroutine = script.Lua.CreateCoroutine(repo.Commands[command.Name]).Coroutine;
        }

        public void RunCoroutine(CommandScript script)
        {
            var commandActions = componentProvider.GetComponent<CommandActions>();

            // Continue lua script
            DynValue result = script.Coroutine.Resume();
            if (script.Coroutine.State == CoroutineState.Suspended)
            {
                script.ReceivedYield = true;
                commandActions.Add(script.Lua, (CommandActionType)result.Number, moveRequester);
            }
            else if (script.Coroutine.State == CoroutineState.Dead)
            {
                script.Coroutine = null;
            }
        }

    }
}
