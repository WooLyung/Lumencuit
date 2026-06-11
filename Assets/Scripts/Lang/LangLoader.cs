using UnityEngine;

namespace Lumencuit
{
    public class LangLoader : MonoBehaviour
    {
        private void Awake()
        {
            Translator.Load("Korean (한국어)");
        }
    }
}