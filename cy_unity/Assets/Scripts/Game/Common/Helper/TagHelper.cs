using UnityEngine;

namespace DH.Game
{
    public static class TagHelper
    {
        public static bool CheckTags(string[] acceptTags, Component target)
        {
            bool accept = false;
            foreach (var tag in acceptTags)
            {
                if (target.CompareTag(tag))
                {
                    accept = true;
                    break;
                }
            }

            return accept;
        }
    }
}