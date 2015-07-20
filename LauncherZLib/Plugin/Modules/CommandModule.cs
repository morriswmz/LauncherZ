using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LauncherZLib.Launcher;

namespace LauncherZLib.Plugin.Modules
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
        /// Gets the arguments being handled. This property is updated when
        /// HandleQuery is called.
        /// </summary>
        public ArgumentCollection CurrentArguments { get; protected set; }

        /// <summary>
        /// Gets the command handler for current arguments. This property is updated
        /// when HandleQuery is called.
        /// </summary>
        public ICommandHandler CurrentCommandHandler { get; protected set; }

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
        /// Handles query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public virtual IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            if (query == null)
                throw new ArgumentNullException("query");
            if (query.InputArguments.Count == 0)
                return LauncherQuery.EmptyResult;

            CurrentArguments = query.InputArguments;
            CurrentCommandHandler = GetCommandHandler(CurrentArguments[0]);
            return CurrentCommandHandler == null ? LauncherQuery.EmptyResult : CurrentCommandHandler.HandleQuery(query);
        }

        /// <summary>
        /// Handles launch.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual PostLaunchAction HandleLaunch(LauncherData data, LaunchContext context)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            
            TC handler = GetCommandHandler(context.CurrentQuery.InputArguments[0]);
            return handler == null ? PostLaunchAction.Default : handler.HandleLaunch(data, context);
        }

    }
}
