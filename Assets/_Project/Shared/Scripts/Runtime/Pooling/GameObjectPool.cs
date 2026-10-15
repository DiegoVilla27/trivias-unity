using System.Collections.Generic;
using UnityEngine;

namespace Trivias.Shared.Pooling
{
    /// <summary>
    /// Pool de instancias de un prefab. Se precalienta al cargar para no instanciar durante la partida;
    /// si se agota, crece (y lo avisa en desarrollo para ajustar el tamaño inicial).
    /// </summary>
    public sealed class GameObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Stack<GameObject> _available;

        public GameObjectPool(GameObject prefab, Transform parent, int prewarm)
        {
            _prefab = prefab;
            _parent = parent;
            _available = new Stack<GameObject>(prewarm);
            for (var i = 0; i < prewarm; i++)
                _available.Push(CreateInstance());
        }

        public GameObject Get()
        {
            GameObject instance;
            if (_available.Count > 0)
            {
                instance = _available.Pop();
            }
            else
            {
                Diagnostics.GameLog.Warning($"[Pool] {_prefab.name} agotado; aumentar el precalentamiento.");
                instance = CreateInstance();
            }

            instance.SetActive(true);
            return instance;
        }

        public void Release(GameObject instance)
        {
            instance.SetActive(false);
            instance.transform.SetParent(_parent, false);
            _available.Push(instance);
        }

        private GameObject CreateInstance()
        {
            var instance = Object.Instantiate(_prefab, _parent);
            instance.SetActive(false);
            return instance;
        }
    }
}
