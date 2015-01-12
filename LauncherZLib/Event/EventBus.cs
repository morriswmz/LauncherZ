using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.API;

namespace LauncherZLib.Event
{
    /// <summary>
    /// Provides a platform to post/subscribe event without explicit registeration between components.
    /// No more "MyClass.EventA += SomeHandler".
    /// This class is not thread-safe.
    /// </summary>
    public class EventBus : IEventBus
    {

        // holds all registered handlers, flexible
        private readonly Dictionary<Type, List<EventHandlerNode>> _handlers = new Dictionary<Type, List<EventHandlerNode>>();
        
        // caches all registered handlers, used when posting events
        // so it is safe to register/unregister event handlers in event handlers
        private readonly Dictionary<Type, EventHandlerNode[]> _handlerCache = new Dictionary<Type, EventHandlerNode[]>(); 
        
        // maps registered objects to associated handlers
        private readonly Dictionary<object, List<EventHandlerNode>> _objectMap = new Dictionary<object, List<EventHandlerNode>>();
        
        private bool _rebuildFlag = false;

        public void Register(object obj)
        {
            // check if already registered
            if (_objectMap.ContainsKey(obj))
                return;

            foreach (var methodInfo in obj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                // check attribute
                if (!methodInfo.GetCustomAttributes(typeof(SubscribeEventAttribute), false).Any())
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

        public void Unregister(object obj)
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

        public void UnregisterAll()
        {
            object[] objects = _objectMap.Keys.ToArray();
            foreach (var o in objects)
            {
                Unregister(o);
            }
        }

        public void Post(EventBase e)
        {
            if (_rebuildFlag)
                RebuildCache();

            EventHandlerNode[] handlers;
            if (_handlerCache.TryGetValue(e.GetType(), out handlers))
            {
                foreach (var node in handlers)
                {
                    node.Handle(e);
                }
            }
        }

        private void RebuildCache()
        {
            _handlerCache.Clear();
            foreach (var pair in _handlers)
            {
                _handlerCache[pair.Key] = pair.Value.ToArray();
            }
            _rebuildFlag = false;
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
