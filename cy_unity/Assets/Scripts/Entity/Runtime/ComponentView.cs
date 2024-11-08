using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DH
{
    public class ComponentView : MonoBehaviour
    {
        public string fullTypeName;
        [SerializeReference] public Entity component;

        private void Start()
        {
           Debug.Log("test"); 
        }
    }
}
