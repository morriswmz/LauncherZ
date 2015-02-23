using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace LauncherZLib.Event
{
    /// <summary>
    /// Provides a platform to post/subscribe event without explicit registeration between components.
    /// No more "MyClass.EventA += SomeHandler".
    /// This class is thread-safe.
    /// </summary>
    public class EventBus : IEventBus
    {

        // holds all registered handlers, flexible
        private readonly Dictionary<Type, List<EventHandlerNode>> _handlers = new Dictionary<Type, List<EventHandlerNode>>();
        
        // caches all registered handlers, used when posting events
        // so it is safe to register/unregister event handlers in event handlers
        private Dictionary<Type, EventHandlerNode[]> _handlerCache = new Dictionary<Type, EventHandlerNode[]>(); 
        
        // maps registered objects to associated handlers
        private readonly Dictionary<object, List<EventHandlerNode>> _objectMap = new Dictionary<object, List<EventHandlerNode>>();
        
        private bool _rebuildFlag = false;
        private object _lock = new object();

        /// <summary>
        /// Registers an event handler.
        /// No action will be taken if given event handler is already registered.
        /// </summary>
        /// <param name="obj"></param>
        /// <seealso cref="T:LauncherZLib.Event.SubscribeEventAttribute"/>
        public void Register(object obj)
        {
            lock (_lock)
            {
                // check if already registered
                if (_objectMap.ContainsKey(obj))
                    return;

                foreach (
                    var methodInfo in
                        obj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
                {
                    // check attribute
                    if (!methodInfo.GetCustomAttributes(typeof (SubscribeEventAttribute), false).Any())
                        continue;
                    // check method
                    if (methodInfo.IsStatic || methodInfo.IsAbstract || methodInfo.IsGenericMethod)
                        throw new InvalidHandlerException("Event handler cannot be static, abstract or generic.");

                    // create and add
                    EventHandlerNode node = CreateHandlerNode(obj, methodInfo);
                    if (node != null)
                    {
                        // add handler
                        if (!_handlers.ContainsKey(node.EventType))
                            _handlers[node.EventType] = new List<EventHandlerNode>();
                        _handlers[node.EventType].Add(node);

                        if (!_objectMap.ContainsKey(obj))
                            _objectMap[obj] = new List<EventHandlerNode>();
                        _objectMap[obj].Add(node);
                        _rebuildFlag = true;
                    }
                }
            }
        }

        /// <summary>
        /// Unregisters a specific event handler.
        /// No action will be taken if given event handler is not registered.
        /// </summary>
        /// <param name="obj"></param>
        public void Unregister(object obj)
        {
            lock (_lock)
            {
                UnregisterImpl(obj);
            }
        }

        /// <summary>
        /// Unregisters all event handlers.
        /// </summary>
        public void UnregisterAll()
        {
            lock (_lock)
            {
                object[] objects = _objectMap.Keys.ToArray();
                foreach (var o in objects)
                {
                    UnregisterImpl(o);
                }
            }
        }

        /// <summary>
        /// Posts an event.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// This method distributes given event to a cached event handler list. Adding/removing event handlers
        /// while handling the event will not affect the cached event handler list.
        /// Although this class is thread-safe, you should still think carefully in multi-threaded environment.
        /// </remarks>
        public void Post(EventBase e)
        {
            if (_rebuildFlag)
                RebuildCache();
            // do not put any lock here, you don't want a deadlock
            EventHandlerNode[] handlers;
            if (_handlerCache.TryGetValue(e.GetType(), out handlers))
            {
                foreach (var node in handlers)
                {
                    node.Handle(e);
                }
            }
        }

        private void UnregisterImpl(object obj)
        {
            if (!_objectMap.ContainsKey(obj))
                return;
            foreach (var node in _objectMap[obj])
            {
                _handlers[node.EventType].Remove(node);
            }
            _objectMap[obj].Clear();
            _objectMap.Remove(obj);
            _rebuildFlag = true;
        }

        private void RebuildCache()
        {
            lock (_lock)
            {
                // sometimes another post call may be blocked when caching is being rebuilt
                // when it continues, we don't want to rebuild the same cache twice
                if (!_rebuildFlag)
                    return;
                // we do not modify the original cache here
                // so it won't breaking ongoing post calls
                var newCache = new Dictionary<Type, EventHandlerNode[]>();
                foreach (var pair in _handlers)
                {
                    newCache[pair.Key] = pair.Value.ToArray();
                }
                _handlerCache = newCache;
                _rebuildFlag = false;
            }
        }
        
        private EventHandlerNode CreateHandlerNode(object obj, MethodInfo method)
        {
            // check parameters
            ParameterInfo[] paramInfos = method.GetParameters();
            if (paramInfos.Length != 1)
                throw new InvalidHandlerException("Expecting only one parameter.");
            if (paramInfos[0].ParameterType.IsByRef)
                throw new InvalidHandlerException("Event should not be passed by reference.");
            Type eventType = paramInfos[0].ParameterType;
            if (eventType.IsInterface)
                throw new InvalidHandlerException("Event should not be interface.");
            if (!typeof(EventBase).IsAssignableFrom(eventType))
                throw new InvalidHandlerException("Event should extend EventBase.");


            // create dynamic invoke
            // note that visibility check is turned off, and we will be able to invoke
            // non-public methods.
            var handler = new DynamicMethod("DynamicEventInvoker",
                typeof(void), new Type[]{typeof(object), typeof(EventBase)}, true);
            Type objType = obj.GetType();
            ILGenerator il = handler.GetILGenerator();
            // ObjectType o;
            il.DeclareLocal(objType);
            // EventType e;
            il.DeclareLocal(eventType);
            // o = (ObjectType) obj;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, objType);
            il.Emit(OpCodes.Stloc_0);
            // e = (EventType) event;
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Castclass, eventType);
            il.Emit(OpCodes.Stloc_1);
            // o.Handle(e);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldloc_1);
            il.Emit(OpCodes.Callvirt, method);
            // ret
            il.Emit(OpCodes.Ret);

            return new EventHandlerNode(obj, eventType,
                (Action<object, EventBase>)handler.CreateDelegate(typeof(Action<object, EventBase>)));
            
        }

        sealed class EventHandlerNode
        {
            public Type EventType { get; private set; }
            public object Instance { get; private set; }

            private readonly Action<object, EventBase> _dynamicHandler;
            
            private static readonly Action<object, EventBase> NopHandler = (o, e) => { }; 

            public EventHandlerNode(object instance, Type eventType, Action<object, EventBase> handler)
            {
                Instance = instance;
                EventType = eventType;
                _dynamicHandler = handler;
            }

            public void Handle(EventBase e)
            {
                 _dynamicHandler(Instance, e);
            }

        }
    }
}
