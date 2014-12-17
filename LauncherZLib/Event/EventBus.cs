using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
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

        private readonly Dictionary<Type, EventHandlerNode> _handlers = new Dictionary<Type, EventHandlerNode>();
        private readonly Dictionary<object, List<EventHandlerNode>> _objectMap = new Dictionary<object, List<EventHandlerNode>>();

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
                        _handlers[node.EventType] = new EventHandlerNode();
                    AddNodeToLinkedList(_handlers[node.EventType], node);

                    if (!_objectMap.ContainsKey(obj))
                        _objectMap[obj] = new List<EventHandlerNode>();
                    _objectMap[obj].Add(node);
                }
            }
        }

        public void Unregister(object obj)
        {
            if (!_objectMap.ContainsKey(obj))
                return;
            foreach (var node in _objectMap[obj])
            {
                EventHandlerNode head = _handlers[node.EventType];
                RemoveNodeFromLinkedList(head, node);
            }
            _objectMap[obj].Clear();
            _objectMap.Remove(obj);
        }

        public void UnregisterAll()
        {
            
        }

        public void Post(EventBase e)
        {
            EventHandlerNode node;
            if (_handlers.TryGetValue(e.GetType(), out node))
            {
                while (node.Next != null)
                {
                    node = node.Next;
                    node.Handle(e);
                }
            }
        }

        private void AddNodeToLinkedList(EventHandlerNode head, EventHandlerNode node)
        {
            EventHandlerNode tail = head;
            while (tail.Next != null)
                tail = tail.Next;
            tail.Next = node;
            node.Next = null;
        }

        private bool RemoveNodeFromLinkedList(EventHandlerNode head, EventHandlerNode node)
        {
            EventHandlerNode current = head;
            while (current.Next != null)
            {
                if (current.Next == node)
                {
                    current.Next = node.Next;
                    return true;
                }
                current = current.Next;
            }
            return false;
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
            public EventHandlerNode Next { get; set; }

            private readonly Action<object, EventBase> _dynamicHandler;
            
            private static readonly Action<object, EventBase> NopHandler = (o, e) => { }; 

            public EventHandlerNode(object instance, Type eventType, Action<object, EventBase> handler)
            {
                Instance = instance;
                EventType = eventType;
                _dynamicHandler = handler;
            }

            public EventHandlerNode()
            {
                Instance = null;
                EventType = typeof (void);
                _dynamicHandler = NopHandler;
            }

            public void Handle(EventBase e)
            {
                 _dynamicHandler(Instance, e);
            }

        }
    }
}
