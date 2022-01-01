using System.Collections;

public class Coroutine
{
    private IEnumerator _coroutine;
    
    public Coroutine(IEnumerator coroutine)
    {
        _coroutine = coroutine;
    }

    public IEnumerator Enumerator => _coroutine;
}