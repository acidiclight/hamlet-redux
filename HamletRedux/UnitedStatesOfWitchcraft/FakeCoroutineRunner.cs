using System.Collections;

namespace HamletRedux.UnitedStatesOfWitchcraft;

public static class FakeCoroutineRunner
{
    public static void FakeCoroutine(IEnumerator coroutine)
    {
        while (coroutine.MoveNext())
        {
            var data = coroutine.Current;
            
            if (data is Coroutine subroutine)
                FakeCoroutine(subroutine.Enumerator);
        }
    }

    public static Coroutine StartCoroutine(this object caller, IEnumerator coroutine)
    {
        return new Coroutine(coroutine);
    }
}