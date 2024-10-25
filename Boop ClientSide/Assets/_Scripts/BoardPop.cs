using UnityEngine;

public class BoardPop : MonoBehaviour {
    [SerializeField] private ParticleSystem _particleSystem;

    public void Init(Color color) {
        var main = _particleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, color);
    }
}
