using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LauncherZLib.Launcher;

namespace LauncherZLib.Plugin.Template
{
    /// <summary>
    /// Provides basic command management.
    /// </summary>
    /// <typeparam name="TC"></typeparam>
    public class CommandModule<TC> where TC : class, ICommandHandler
    {
        protected readonly bool CaseSensitive;
        protected readonly Dictionary<string, TC> CommandHandlers; 

        public CommandModule(bool caseSensitive)
        {
            CaseSensitive = caseSensitive;
            CommandHandlers = caseSensitive
                ? new Dictionary<string, TC>()
                : new Dictionary<string, TC>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets whether command names are case insensitive.
        /// </summary>
        public bool IsCaseSensitive
        {
            get { return CaseSensitive; }
        }

        /// <summary>
        /// Adds or updates a command handler.
        /// </summary>
        /// <param name="handler">Command handler. Cannot be null.</param>
        /// <returns>True if update is performed.</returns>
        public virtual bool AddOrUpdateCommandHanlder(TC handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            if (string.IsNullOrWhiteSpace(handler.CommandName))
                throw new Exception("Command name cannot be null or whitespace.");
            if (Regex.IsMatch(handler.CommandName, @"\s"))
            {
                throw new Exception("Command name cannot contain whitespaces.");
            }
            bool update = CommandHandlers.ContainsKey(handler.CommandName) &&
                          CommandHandlers[handler.CommandName].Equals(handler);
            CommandHandlers[handler.CommandName] = handler;
            return update;
        }

        /// <summary>
        /// Removes a command handler.
        /// </summary>
        /// <param name="handler">Command handler. Cannot be null.</param>
        /// <returns>True if actually removed.</returns>
        public virtual bool RemoveCommandHandler(TC handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            return CommandHandlers.Remove(handler.CommandName);
        }

        /// <summary>
        /// Remove all command handlers.
        /// </summary>
        public virtual void RemoveAllCommandHandlers()
        {
            CommandHandlers.Clear();
        }

        /// <summary>
        /// Retrieves a command handler by command name.
        /// </summary>
        /// <param name="cmd">Commands name. Null is allowed since return value may be null.</param>
        /// <returns>Corresponding command handler. Null if not found or command name is null.</returns>
        public virtual TC GetCommandHandler(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return null;
            TC handler;
            return CommandHandlers.TryGetValue(cmd, out handler) ? handler : null;
        }

        /// <summary>
        /// Retrieves a command handler by launcher cmdData.
        /// </summary>
        /// <param name="cmdData">Launcher cmdData. Null is allowed since return value may be null.</param>
        /// <returns>
        /// Corresponding command handler. Null if not found, cmdData is null, or arguments are not valid.
        /// </returns>
        public virtual TC GetCommandHandler(CommandLauncherData cmdData)
        {
            if (cmdData == null || cmdData.CommandArgs == null || cmdData.CommandArgs.Count == 0)
                return null;
            return GetCommandHandler(cmdData.CommandArgs[0]);
        }

        /// <summary>
        /// Handles query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual IEnumerable<CommandLauncherData> HandleQuery(LauncherQuery query)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (query.Arguments.Count == 0)
                return Enumerable.Empty<CommandLauncherData>();
            TC handler = GetCommandHandler(query.Arguments[0]);
            return handler == null ? Enumerable.Empty<CommandLauncherData>() : handler.HandleQuery(query);
        }

        /// <summary>
        /// Handles launch.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual PostLaunchAction HandleLaunch(CommandLauncherData data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            TC handler = GetCommandHandler(data);
            return handler == null ? PostLaunchAction.Default : handler.HandleLaunch(data);
        }

    }
}
