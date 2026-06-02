using UnityEngine;

namespace Lumencuit
{
    public class LangLoader : MonoBehaviour
    {
        private void Awake()
        {
            Translator.Load("Korean (한국어)");
            Debug.Log("Success".Translate());
            Debug.Log("Fail".Translate("실패실패"));
        }
    }
}