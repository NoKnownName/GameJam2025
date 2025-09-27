using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempVanish : MonoBehaviour
{
    private readonly List<Collider2D> _cols = new();
    private readonly List<bool> _colsEnabled = new();

    private readonly List<Renderer> _renders = new();
    private readonly List<bool> _rendersEnabled = new();

    private readonly List<Rigidbody2D> _rbs = new();
    private readonly List<bool> _rbsSim = new();

    public void Vanish(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(VanishCo(duration));
    }

    private IEnumerator VanishCo(float duration)
    {
        CacheIfNeeded();

        for (int i = 0; i < _renders.Count; i++) _renders[i].enabled = false;
        for (int i = 0; i < _cols.Count; i++) _cols[i].enabled = false;
        for (int i = 0; i < _rbs.Count; i++) _rbs[i].simulated = false;

        yield return new WaitForSeconds(duration);

        for (int i = 0; i < _renders.Count; i++) _renders[i].enabled = _rendersEnabled[i];
        for (int i = 0; i < _cols.Count; i++) _cols[i].enabled = _colsEnabled[i];
        for (int i = 0; i < _rbs.Count; i++) _rbs[i].simulated = _rbsSim[i];
    }

    private void CacheIfNeeded()
    {
        if (_renders.Count == 0 && _cols.Count == 0 && _rbs.Count == 0)
        {
            GetComponentsInChildren(true, _renders);
            GetComponentsInChildren(true, _cols);
            GetComponentsInChildren(true, _rbs);

            foreach (var r in _renders) _rendersEnabled.Add(r.enabled);
            foreach (var c in _cols) _colsEnabled.Add(c.enabled);
            foreach (var rb in _rbs) _rbsSim.Add(rb.simulated);
        }
    }
}
