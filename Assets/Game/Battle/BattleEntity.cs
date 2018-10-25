using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    class BattleEntity
    {
        private GameObject mObj;
        private Dictionary<Type, Component> componentDic = new Dictionary<Type, Component>();

        public BattleEntity(GameObject obj)
        {
            this.mObj = obj;
        }

        public void Init()
        {
            
        }

        public T AddComponent<T>() where T : Component, new()
        {
            Component component = null;
            if (!componentDic.TryGetValue(typeof(T), out component))
            {
                GameObject obj = GameObject.Instantiate(mObj);
                component = obj.AddComponent<T>();
                componentDic[typeof(T)] = component;
            }

            return (T)component;
        }

        public T GetComponent<T>() where T : Component, new()
        {
            Component component = null;
            if (componentDic.TryGetValue(typeof(T), out component))
            {
                return (T)component;
            }

            return null;
        }

        public void UpdateFixed(int deltaTime)
        {
            foreach(var item in componentDic)
            {
                item.Value.UpdateFixed(deltaTime);
            }
        }

        public void Update()
        {
            foreach (var item in componentDic)
            {
                item.Value.Update();
            }
        }
    }
}
