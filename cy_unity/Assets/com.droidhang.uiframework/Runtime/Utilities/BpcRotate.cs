using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DH.UChat.Support
{
    public class BpcRotate : MonoBehaviour
    {
        public float Speed = 100f;

        private Transform trans;

        private void Awake()
        {
            trans = transform;
        }

        void Update()
        {
            trans.Rotate(Vector3.back, Speed * Time.deltaTime);
        }
    }
}
