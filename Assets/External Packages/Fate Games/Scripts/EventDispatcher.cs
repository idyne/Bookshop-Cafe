using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames
{
    public class EventDispatcher : MonoBehaviour
    {
        private int count = 0;
        private Dictionary<int, Action<object[]>> actions = new Dictionary<int, Action<object[]>>();

        public int Bind(Action<object[]> action)
        {
            int id = count++;
            actions.Add(id, action);
            return id;
        }

        public void Call(params object[] args)
        {
            foreach (Action<object[]> action in actions.Values)
                action(args);
        }

        public void Remove(int id)
        {
            if (actions.ContainsKey(id)) actions.Remove(id);
        }

        public void Clear()
        {
            actions.Clear();
        }
    }
}

