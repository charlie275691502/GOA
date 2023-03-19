using Rayark.Mast;
using System.Collections;
using UnityEngine;
using Coroutine = Rayark.Mast.Coroutine;

public class Test : MonoBehaviour
{
    Executor executor = new Executor();

    IEnumerator A()
    {

        Debug.Log("1");
        yield return null;
        Debug.Log("2");
    }

    IEnumerator B()
    {

        Debug.Log("3");
        yield return null;
        Debug.Log("4");
    }

    void Start()
    {
        executor.Add(A());
        executor.Add(B());
    }

    void Update()
    {
        if (!executor.Finished)
        {
            executor.Resume(Time.deltaTime);
        }
    }
}
