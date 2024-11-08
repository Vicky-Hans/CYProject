using System;
using QD.Base;
using UnityEngine;

namespace DH.Base
{
    public class ObscuredTypeTest : MonoBehaviour
    {
        private ConfuseInt confuseFloat;
        
        private void Awake()
        {
            Debug.Log($"{confuseFloat}, {confuseFloat.GetValue()}");

            ConfuseInt testB = new ConfuseInt();
            Debug.Log($"{confuseFloat == testB}");
            
            confuseFloat.SetValue(225);
            Debug.Log($"{confuseFloat}, {confuseFloat.GetValue()}");
            Debug.Log($"{confuseFloat == testB}");

            Debug.Log("================");
            
            testB = confuseFloat;
            Debug.Log($"{testB.GetValue()}");
            
            Debug.Log("================");
            
            testB.SetValue(10);
            Debug.Log($"{confuseFloat}, {confuseFloat.GetValue()}");
            Debug.Log($"{confuseFloat == testB}");
            Debug.Log($"{testB.GetValue()}");
            
            Debug.Log("================");

            var temp = testB.GetValue() * confuseFloat.GetValue();
            var testC = new ConfuseLong();
            testC.SetValue(temp);
            Debug.Log($"{testC}, {testC.GetValue()}");
            Debug.Log($"{confuseFloat == testC}");
            Debug.Log($"{testC.GetValue()}");

            Debug.Log("================");
            Debug.Log($"{(ConfuseInt)10}");
            Debug.Log($"{(int)testC}");
            
            testC = 100;
            long dddd = testC;
            Debug.Log($"{(int)testC}, {dddd}");

            Debug.Log($"{testC + 1}, {testC + 1.5f}");

        }
    }
}