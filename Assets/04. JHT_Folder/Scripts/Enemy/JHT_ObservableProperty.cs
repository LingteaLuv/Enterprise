using System;
using UnityEngine;
using UnityEngine.Events;

namespace JHT
{
    public class JHT_ObservableProperty<T>
    {
        private T _value;
        public T Value
        {
            get { return _value; }
            set
            {
                if (_value.Equals(value))
                    return;

                _value = value;
                onValueChanged?.Invoke(_value);
            }
        }

        public JHT_ObservableProperty(T value = default)
        {
            _value = value;
        }


        private event Action<T> onValueChanged;

        public void Subscribe(Action<T> action)
        {
            onValueChanged += action;
        }

        public void UnSubscribe(Action<T> action)
        {
            onValueChanged -= action;
        }

        public void UnSubscribeAll()
        {
            foreach (Action<T> action in onValueChanged.GetInvocationList())
            {
                onValueChanged -= action;
            }
        }
    }
}
