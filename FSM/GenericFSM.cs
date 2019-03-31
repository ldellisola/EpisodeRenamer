using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenericFSM
{
    public class FSM<T> where T:struct,IConvertible
    {
        public bool hasToLeave { get; private set; }

        protected T prevState;

        protected List<string> errors = new List<string>();

        public T State { get; private set; }
        
        private const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;
        
        private IDictionary<T, MethodInfo> states = new Dictionary<T, MethodInfo>();

        private IDictionary<T, MethodInfo> transitions = new Dictionary<T, MethodInfo>();

        protected void Leave()
        {
            
            hasToLeave = true;
        }
        
        public FSM(T init)
        {
            hasToLeave = false;

            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumeration");
            }
          
            // Cache state and transition functions

            foreach (T value in typeof(T).GetEnumValues())
            {

                var s = GetType().GetMethod(value.ToString() + "State", FLAGS);

                if (s != null)
                {

                    states.Add(value, s);

                }
                
                var t = GetType().GetMethod(value.ToString() + "Transition", FLAGS);

                if (t != null)
                {

                    transitions.Add(value, t);

                }

            }
            State = init;
        }



        public void Transition(T next)
        {
            MethodInfo method;

            prevState = State;

            if (transitions.TryGetValue(next, out method))
            {

                method.Invoke(this, new object[] { State });

            }
            State = next;
        }



        public void StateDo()
        {
            MethodInfo method;

            if (states.TryGetValue(State, out method))
            {
                method.Invoke(this, null);
            }
        }
    }
}
