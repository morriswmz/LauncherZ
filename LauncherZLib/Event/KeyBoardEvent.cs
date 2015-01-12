using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LauncherZLib.Event
{
    public class KeyBoardEvent : EventBase
    {
        private readonly Key _key;

        public Key Key { get { return _key; } }

        public KeyBoardEvent(Key key)
        {
            _key = key;
        }
    }

    public class KeyDownEvent : KeyBoardEvent {
        public KeyDownEvent(Key key) : base(key)
        {
        }
    }

    public class KeyUpEvent : KeyBoardEvent
    {
        public KeyUpEvent(Key key) : base(key)
        {
        }
    }

}
