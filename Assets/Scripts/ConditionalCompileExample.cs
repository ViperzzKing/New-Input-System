using UnityEngine;

public class ConditionalCompileExample : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        print("The current platform is " + Application.platform);
        
        #if UNITY_EDITOR
        print("This is a special msg");
        #elif UNITY_STANDALONE_WIN
        print("We're in the standalone player on windows");
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
